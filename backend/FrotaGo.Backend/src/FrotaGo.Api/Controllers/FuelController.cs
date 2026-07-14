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

    [HttpPost]
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
}
