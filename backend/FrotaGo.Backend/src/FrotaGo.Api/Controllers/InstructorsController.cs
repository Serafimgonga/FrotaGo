using System.Threading.Tasks;
using FrotaGo.Application.Features.Instructors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrotaGo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InstructorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public InstructorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetInstructorsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInstructorCommand command)
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
