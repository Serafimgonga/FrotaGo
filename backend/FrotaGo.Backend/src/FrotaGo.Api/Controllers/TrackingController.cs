using System;
using System.Linq;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using FrotaGo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrackingController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ITrackingHubContext _hubContext;

    public TrackingController(
        ApplicationDbContext context,
        ITrackingHubContext hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public record StartTrackingRequest(Guid VehicleId, Guid? InstructorId, Guid? LessonId, string Provider);
    public record TrackLocationRequest(Guid TrackingSessionId, double Latitude, double Longitude, double Speed);

    [HttpPost("start")]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin,Instructor")]
    public async Task<IActionResult> StartTracking([FromBody] StartTrackingRequest request)
    {
        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
        if (vehicle == null)
        {
            return NotFound(new { message = $"Veículo com o ID {request.VehicleId} não foi encontrado." });
        }

        // Encerrar quaisquer sessões ativas ou perdidas anteriores para este veículo para evitar sessões órfãs
        var orphanSessions = await _context.TrackingSessions
            .Where(s => s.VehicleId == request.VehicleId && 
                       (s.Status == TrackingStatus.Active || s.Status == TrackingStatus.LostConnection || s.Status == TrackingStatus.Starting))
            .ToListAsync();

        foreach (var orphan in orphanSessions)
        {
            orphan.Status = TrackingStatus.Stopped;
            orphan.EndedAt = DateTime.UtcNow;
            _context.TrackingSessions.Update(orphan);
            await _hubContext.BroadcastTrackingStoppedAsync(orphan.VehicleId);
        }

        var session = new TrackingSession
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId,
            InstructorId = request.InstructorId,
            LessonId = request.LessonId,
            Status = TrackingStatus.Starting,
            Provider = string.IsNullOrWhiteSpace(request.Provider) ? "mobile" : request.Provider,
            StartedAt = DateTime.UtcNow
        };

        _context.TrackingSessions.Add(session);
        await _context.SaveChangesAsync();

        // Broadcast de início de sessão
        await _hubContext.BroadcastStatusChangedAsync(session.Id, session.VehicleId, "Starting");

        return Ok(session);
    }

    [HttpPost("location")]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin,Instructor")]
    public async Task<IActionResult> TrackLocation([FromBody] TrackLocationRequest request)
    {
        var session = await _context.TrackingSessions.FindAsync(request.TrackingSessionId);
        if (session == null)
        {
            return NotFound(new { message = $"Sessão de tracking com o ID {request.TrackingSessionId} não existe." });
        }

        if (session.Status == TrackingStatus.Stopped)
        {
            return BadRequest(new { message = "Esta sessão de tracking já foi encerrada." });
        }

        var previousStatus = session.Status;
        session.Status = TrackingStatus.Active;
        _context.TrackingSessions.Update(session);

        var location = new VehicleLocation
        {
            Id = Guid.NewGuid(),
            VehicleId = session.VehicleId,
            TrackingSessionId = session.Id,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Speed = request.Speed,
            Timestamp = DateTime.UtcNow
        };

        _context.VehicleLocations.Add(location);
        await _context.SaveChangesAsync();

        // Emitir atualização de localização em tempo real via SignalR
        await _hubContext.BroadcastLocationAsync(session.VehicleId, request.Latitude, request.Longitude, request.Speed);

        // Se o status da sessão mudou para ativo agora, emite a mudança de status
        if (previousStatus != TrackingStatus.Active)
        {
            await _hubContext.BroadcastStatusChangedAsync(session.Id, session.VehicleId, "Active");
        }

        return Ok(new { message = "Coordenadas GPS registadas e transmitidas com sucesso.", vehicleId = session.VehicleId });
    }

    [HttpPost("stop/{sessionId}")]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin,Instructor")]
    public async Task<IActionResult> StopTracking(Guid sessionId)
    {
        var session = await _context.TrackingSessions.FindAsync(sessionId);
        if (session == null)
        {
            return NotFound(new { message = $"Sessão com o ID {sessionId} não foi encontrada." });
        }

        session.Status = TrackingStatus.Stopped;
        session.EndedAt = DateTime.UtcNow;
        _context.TrackingSessions.Update(session);
        await _context.SaveChangesAsync();

        // Notificar via SignalR os admins sobre o encerramento do sinalizador e da sessão
        await _hubContext.BroadcastTrackingStoppedAsync(session.VehicleId);
        await _hubContext.BroadcastStatusChangedAsync(session.Id, session.VehicleId, "Stopped");

        return Ok(new { message = "Transmissão encerrada com sucesso." });
    }

    [HttpGet("session/{sessionId}")]
    public async Task<IActionResult> GetSession(Guid sessionId)
    {
        var session = await _context.TrackingSessions
            .Include(s => s.Vehicle)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
        {
            return Ok(new { isValid = false });
        }

        var isValid = session.Status == TrackingStatus.Starting || 
                      session.Status == TrackingStatus.Active || 
                      session.Status == TrackingStatus.LostConnection;

        return Ok(new { isValid, status = session.Status.ToString(), vehicle = session.Vehicle });
    }

    [HttpGet("active")]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin")]
    public async Task<IActionResult> GetActiveSessions()
    {
        // Obter todas as sessões que estão ativas ou sem sinal temporário
        var activeSessions = await _context.TrackingSessions
            .Include(s => s.Vehicle)
            .Include(s => s.Instructor)
            .Where(s => s.Status == TrackingStatus.Active || s.Status == TrackingStatus.LostConnection || s.Status == TrackingStatus.Starting)
            .ToListAsync();

        var result = await Task.WhenAll(activeSessions.Select(async s =>
        {
            var lastLoc = await _context.VehicleLocations
                .Where(vl => vl.TrackingSessionId == s.Id)
                .OrderByDescending(vl => vl.Timestamp)
                .FirstOrDefaultAsync();

            return new
            {
                SessionId = s.Id,
                VehicleId = s.VehicleId,
                Vehicle = s.Vehicle,
                Instructor = s.Instructor,
                Status = s.Status.ToString(),
                Provider = s.Provider,
                StartedAt = s.StartedAt,
                Latitude = lastLoc?.Latitude,
                Longitude = lastLoc?.Longitude,
                Speed = lastLoc?.Speed,
                LastUpdate = lastLoc?.Timestamp ?? s.StartedAt
            };
        }));

        return Ok(result);
    }

    [HttpGet("history/{vehicleId}")]
    public async Task<IActionResult> GetHistory(Guid vehicleId)
    {
        var history = await _context.VehicleLocations
            .Where(vl => vl.VehicleId == vehicleId)
            .OrderBy(vl => vl.Timestamp)
            .Take(100)
            .Select(vl => new {
                vl.Id,
                vl.VehicleId,
                vl.TrackingSessionId,
                vl.Latitude,
                vl.Longitude,
                vl.Speed,
                vl.Timestamp
            })
            .ToListAsync();

        return Ok(history);
    }
}
