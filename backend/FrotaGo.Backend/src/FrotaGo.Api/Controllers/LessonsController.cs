using System;
using System.Threading.Tasks;
using FrotaGo.Application.Features.Lessons;
using FrotaGo.Domain.Enums;
using FrotaGo.Infrastructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrotaGo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LessonsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _context;

    public LessonsController(IMediator mediator, ApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetLessonsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLessonCommand command)
    {
        try
        {
            var id = await _mediator.Send(command);
            return Ok(new { id });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{lessonId:guid}/start")]
    public async Task<IActionResult> Start(Guid lessonId)
    {
        var lesson = await _context.Lessons.FindAsync(lessonId);
        if (lesson == null)
        {
            return NotFound(new { message = "Aula não encontrada." });
        }

        lesson.Status = LessonStatus.Realizada;
        _context.Lessons.Update(lesson);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Aula iniciada com sucesso.", lessonId = lesson.Id });
    }

    [HttpPost("{lessonId:guid}/stop")]
    public async Task<IActionResult> Stop(Guid lessonId)
    {
        var lesson = await _context.Lessons.FindAsync(lessonId);
        if (lesson == null)
        {
            return NotFound(new { message = "Aula não encontrada." });
        }

        lesson.Status = LessonStatus.Realizada;
        _context.Lessons.Update(lesson);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Aula finalizada com sucesso.", lessonId = lesson.Id });
    }
}
