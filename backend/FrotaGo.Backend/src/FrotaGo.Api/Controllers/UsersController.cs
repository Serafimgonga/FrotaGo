using System;
using System.Threading.Tasks;
using FrotaGo.Application.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FrotaGo.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("school/{schoolId:guid}")]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin")]
    public async Task<IActionResult> GetSchoolUsers(Guid schoolId)
    {
        var result = await _mediator.Send(new GetUsersQuery(schoolId));
        return Ok(result);
    }

    [HttpPost("invite")]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin")]
    public async Task<IActionResult> InviteUser([FromBody] InviteUserCommand command)
    {
        try
        {
            var token = await _mediator.Send(command);
            return Ok(new { token, message = "Convite gerado com sucesso!" });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("accept-invitation")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationCommand command)
    {
        try
        {
            var success = await _mediator.Send(command);
            return Ok(new { success, message = "Conta ativada com sucesso! Já pode iniciar sessão." });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{userId:guid}/toggle-status")]
    [Authorize(Roles = "SuperAdmin,SchoolOwner,SchoolAdmin")]
    public async Task<IActionResult> ToggleStatus(Guid userId)
    {
        try
        {
            var success = await _mediator.Send(new ToggleUserStatusCommand(userId));
            if (!success) return NotFound(new { message = "Utilizador não encontrado." });
            return Ok(new { message = "Estado do utilizador alterado com sucesso." });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
