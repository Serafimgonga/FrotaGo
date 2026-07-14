using System;
using System.Linq;
using System.Threading.Tasks;
using FrotaGo.Api.Hubs;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GpsController : ControllerBase
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<GpsHub> _hubContext;

    public GpsController(
        IVehicleRepository vehicleRepository,
        ApplicationDbContext context,
        IHubContext<GpsHub> hubContext)
    {
        _vehicleRepository = vehicleRepository;
        _context = context;
        _hubContext = hubContext;
    }

    public record TrackLocationRequest(string LicensePlate, double Latitude, double Longitude, double Speed);

    [HttpPost("track")]
    public async Task<IActionResult> TrackLocation([FromBody] TrackLocationRequest request)
    {
        var vehicle = await _vehicleRepository.GetByLicensePlateAsync(request.LicensePlate);
        if (vehicle == null)
        {
            return NotFound(new { message = $"Veículo com a matrícula {request.LicensePlate} não foi encontrado." });
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n>>> [GPS RECEBIDO] Viatura {request.LicensePlate} -> Lat: {request.Latitude:F5}, Lng: {request.Longitude:F5}, Vel: {request.Speed:0.0} km/h");
        Console.ResetColor();

        var location = new VehicleLocation
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Speed = request.Speed,
            Timestamp = DateTime.UtcNow
        };

        _context.VehicleLocations.Add(location);
        await _context.SaveChangesAsync();

        // Broadcast a atualização em tempo real via SignalR
        await _hubContext.Clients.All.SendAsync("LocationUpdated", vehicle.Id, request.Latitude, request.Longitude, request.Speed);

        return Ok(new { message = "Coordenadas GPS registadas e transmitidas com sucesso.", vehicleId = vehicle.Id });
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
                vl.Latitude,
                vl.Longitude,
                vl.Speed,
                vl.Timestamp
            })
            .ToListAsync();

        return Ok(history);
    }
}
