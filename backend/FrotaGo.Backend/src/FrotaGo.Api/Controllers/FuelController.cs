using System;
using System.Threading.Tasks;
using FrotaGo.Application.Features.Fuel;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrotaGo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FuelController : ControllerBase
{
    private readonly IMediator _mediator;

    public FuelController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetFuelRecordsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetFuelRecordByIdQuery(id));
        if (result == null) return NotFound(new { message = "Registo de combustível não encontrado." });
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateFuelRecordCommand command)
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFuelRecordCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { message = "ID no URL não coincide com o ID do pedido." });
        }

        try
        {
            var success = await _mediator.Send(command);
            if (!success) return NotFound(new { message = "Registo de combustível não encontrado." });
            return Ok(new { message = "Registo de combustível atualizado com sucesso." });
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
            var success = await _mediator.Send(new DeleteFuelRecordCommand(id));
            if (!success) return NotFound(new { message = "Registo de combustível não encontrado." });
            return Ok(new { message = "Registo de combustível eliminado com sucesso." });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
