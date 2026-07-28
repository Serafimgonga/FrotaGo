using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FrotaGo.Application.Features.Lessons.Commands.FinishLesson;
using FrotaGo.Application.Features.Lessons.Commands.StartLesson;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using FrotaGo.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FrotaGo.Api.Controllers;

[ApiController]
[Route("api/mobile")]
[Authorize]
public class MobileController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMediator _mediator;
    private readonly ITrackingHubContext _hubContext;

    public MobileController(
        ApplicationDbContext context,
        IMediator mediator,
        ITrackingHubContext hubContext)
    {
        _context = context;
        _mediator = mediator;
        _hubContext = hubContext;
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        var name = User.FindFirst(ClaimTypes.Name)?.Value ?? "Instrutor";

        var schoolName = "FrotaGo Driving School";
        var schoolClaim = User.FindFirst("schoolId")?.Value;
        if (Guid.TryParse(schoolClaim, out var schoolId))
        {
            var school = await _context.Schools.FindAsync(schoolId);
            if (school != null) schoolName = school.Name;
        }

        return Ok(new
        {
            id = userId?.ToString() ?? Guid.NewGuid().ToString(),
            nome = name,
            email = email,
            telefone = "+244 923 000 000",
            fotografia = "assets/images/default-avatar.png",
            escola = schoolName
        });
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var today = DateTime.UtcNow.Date;
        var lessonsToday = await _context.Lessons
            .Where(l => l.ScheduledDate.Date == today)
            .CountAsync();

        var nextLesson = await _context.Lessons
            .Where(l => l.ScheduledDate >= DateTime.UtcNow && l.Status == LessonStatus.Agendada)
            .OrderBy(l => l.ScheduledDate)
            .Select(l => l.ScheduledDate.ToString("HH:mm"))
            .FirstOrDefaultAsync() ?? "14:00";

        return Ok(new
        {
            lessonsToday = Math.Max(lessonsToday, 1),
            nextLesson = nextLesson,
            notifications = 2
        });
    }

    [HttpGet("lessons/today")]
    public async Task<IActionResult> GetTodayLessons()
    {
        var lessons = await _context.Lessons
            .Include(l => l.Student)
            .Include(l => l.Vehicle)
            .Include(l => l.Instructor)
            .OrderBy(l => l.ScheduledDate)
            .ToListAsync();

        if (!lessons.Any())
        {
            return Ok(new object[] { });
        }

        var result = lessons.Select(l => new
        {
            lessonId = l.Id,
            student = l.Student != null ? l.Student.Name : "Aluno Desconhecido",
            vehicle = l.Vehicle != null ? $"{l.Vehicle.Brand} {l.Vehicle.Model} ({l.Vehicle.LicensePlate})" : "Viatura Não Atribuída",
            start = l.ScheduledDate.ToString("HH:mm"),
            status = l.Status.ToString(),
            topic = l.Topic
        });

        return Ok(result);
    }

    [HttpGet("lessons/{id:guid}")]
    public async Task<IActionResult> GetLessonById(Guid id)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Student)
            .Include(l => l.Vehicle)
            .Include(l => l.Instructor)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lesson == null)
        {
            return NotFound(new { message = "Aula prática não encontrada." });
        }

        return Ok(new
        {
            lessonId = lesson.Id,
            student = new
            {
                id = lesson.StudentId,
                name = lesson.Student?.Name ?? "Aluno",
                category = lesson.Student?.Category.ToString() ?? "Ligeiro Amador (B)",
                phone = lesson.Student?.PhoneNumber ?? "+244 923 111 222"
            },
            vehicle = new
            {
                id = lesson.VehicleId,
                brand = lesson.Vehicle?.Brand ?? "Toyota",
                model = lesson.Vehicle?.Model ?? "Corolla",
                licensePlate = lesson.Vehicle?.LicensePlate ?? "LD-45-89-AA",
                fuelType = lesson.Vehicle?.Fuel.ToString() ?? "Gasolina"
            },
            schedule = new
            {
                date = lesson.ScheduledDate.ToString("dd/MM/yyyy"),
                time = lesson.ScheduledDate.ToString("HH:mm"),
                durationMinutes = lesson.DurationMinutes
            },
            meetingPoint = "Instalações Principais da Escola de Condução",
            status = lesson.Status.ToString(),
            topic = lesson.Topic,
            observations = lesson.Observations
        });
    }

    [HttpGet("lessons/{id:guid}/resources")]
    public async Task<IActionResult> GetLessonResources(Guid id)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Student)
            .Include(l => l.Vehicle)
            .FirstOrDefaultAsync(l => l.Id == id);

        return Ok(new
        {
            student = new
            {
                id = lesson?.StudentId,
                name = lesson?.Student?.Name ?? "Aluno",
                bi = lesson?.Student?.IdentityCardNumber ?? "004598123LA042"
            },
            vehicle = new
            {
                id = lesson?.VehicleId,
                plate = lesson?.Vehicle?.LicensePlate ?? "LD-45-89-AA",
                currentKm = lesson?.Vehicle?.Odometer ?? 45000
            },
            route = new
            {
                name = "Circuito de Exame Talatona / Marginal",
                recommendedSpeedLimit = 50
            },
            documents = new[]
            {
                "Seguro Automóvel Válido",
                "Inspeção Técnica Válida",
                "Licença de Aprendizagem do Aluno"
            }
        });
    }

    [HttpPost("lessons/{id:guid}/start")]
    public async Task<IActionResult> StartLesson(Guid id)
    {
        try
        {
            var resultId = await _mediator.Send(new StartLessonCommand(id));
            return Ok(new { message = "Aula iniciada com sucesso. Rastreamento GPS ativo.", lessonId = resultId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public record FinishMobileLessonRequest(string Evaluation, string Notes, int Odometer);

    [HttpPost("lessons/{id:guid}/finish")]
    public async Task<IActionResult> FinishLesson(Guid id, [FromBody] FinishMobileLessonRequest request)
    {
        try
        {
            Enum.TryParse<LessonEvaluation>(request.Evaluation, true, out var eval);

            var command = new FinishLessonCommand(id, eval, "[]", request.Notes ?? "");
            var success = await _mediator.Send(command);

            if (!success)
            {
                return NotFound(new { message = "Aula não encontrada para finalização." });
            }

            return Ok(new { message = "Aula concluída e relatório enviado com sucesso!", lessonId = id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("tracking/location")]
    public async Task<IActionResult> LocationProxy([FromBody] TrackingController.TrackLocationRequest request)
    {
        var session = await _context.TrackingSessions.FindAsync(request.TrackingSessionId);
        if (session == null)
        {
            return NotFound(new { message = "Sessão de rastreamento não encontrada." });
        }

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

        await _hubContext.BroadcastLocationAsync(session.VehicleId, request.Latitude, request.Longitude, request.Speed);

        return Ok(new { message = "Localização registada com sucesso." });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        return Ok(new { message = "Sessão terminada com sucesso." });
    }
}
