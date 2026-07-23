using System.Threading.Tasks;
using FrotaGo.Application.Features.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrotaGo.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        try
        {
            var token = await _mediator.Send(command);
            return Ok(new { token });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("register-school")]
    public async Task<IActionResult> RegisterSchool([FromBody] RegisterSchoolCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("update-plan")]
    [Authorize]
    public async Task<IActionResult> UpdatePlan([FromBody] UpdateSchoolPlanCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(new { success = result, message = "Plano atualizado com sucesso!" });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
