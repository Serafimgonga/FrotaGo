using System;
using System.Threading.Tasks;
using FrotaGo.Application.Features.Accidents;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrotaGo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccidentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccidentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAccidentsQuery());
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetAccidentByIdQuery(id));
        if (result == null) return NotFound(new { message = "Registo de acidente não encontrado." });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateAccidentCommand command)
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
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccidentCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "ID no URL não coincide com o ID do pedido." });
        }

        try
        {
            var success = await _mediator.Send(command);
            if (!success) return NotFound(new { message = "Registo de acidente não encontrado." });
            return Ok(new { message = "Registo de acidente atualizado com sucesso." });
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
            var success = await _mediator.Send(new DeleteAccidentCommand(id));
            if (!success) return NotFound(new { message = "Registo de acidente não encontrado." });
            return Ok(new { message = "Registo de acidente eliminado com sucesso." });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
