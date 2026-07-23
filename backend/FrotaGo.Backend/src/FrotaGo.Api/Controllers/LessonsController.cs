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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetLessonByIdQuery(id));
        if (result == null) return NotFound(new { message = "Aula prática não encontrada." });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin,Receptionist")]
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

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin,Receptionist")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "ID no URL não coincide com o ID do pedido." });
        }

        try
        {
            var success = await _mediator.Send(command);
            if (!success) return NotFound(new { message = "Aula prática não encontrada." });
            return Ok(new { message = "Aula prática atualizada com sucesso." });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var success = await _mediator.Send(new DeleteLessonCommand(id));
            if (!success) return NotFound(new { message = "Aula prática não encontrada." });
            return Ok(new { message = "Aula prática eliminada com sucesso." });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable([FromQuery] DateTime scheduledDate, [FromQuery] int durationMinutes = 60)
    {
        try
        {
            var result = await _mediator.Send(new GetAvailableResourcesQuery(scheduledDate, durationMinutes));
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("auto-dispatch")]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin,Receptionist")]
    public async Task<IActionResult> AutoDispatch([FromBody] AutoDispatchLessonCommand command)
    {
        try
        {
            var id = await _mediator.Send(command);
            return Ok(new { id, message = "Aula despachada e agendada automaticamente com sucesso!" });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{lessonId:guid}/start")]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin,Instructor")]
    public async Task<IActionResult> Start(Guid lessonId)
    {
        try
        {
            var id = await _mediator.Send(new Application.Features.Lessons.Commands.StartLesson.StartLessonCommand(lessonId));
            return Ok(new { message = "Aula prática iniciada com sucesso. Rastreamento GPS ativo.", lessonId = id });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{lessonId:guid}/complete")]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin,Instructor")]
    public async Task<IActionResult> Complete(Guid lessonId, [FromBody] Application.Features.Lessons.Commands.FinishLesson.FinishLessonCommand command)
    {
        if (lessonId != command.LessonId)
        {
            return BadRequest(new { message = "ID no URL não coincide com o ID do pedido." });
        }

        try
        {
            var success = await _mediator.Send(command);
            if (!success) return NotFound(new { message = "Aula prática não encontrada." });
            return Ok(new { message = "Aula prática finalizada com sucesso! Avaliação e progresso do aluno registados." });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
