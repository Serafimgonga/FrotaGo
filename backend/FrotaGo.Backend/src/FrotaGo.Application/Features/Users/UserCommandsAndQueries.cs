using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Users;

public record UserDto(
    Guid Id,
    string Name,
    string Email,
    string Role,
    bool IsActive,
    Guid? SchoolId,
    DateTime CreatedAt
);

public record GetUsersQuery(Guid SchoolId) : IRequest<IEnumerable<UserDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IEnumerable<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetBySchoolIdAsync(request.SchoolId);

        return users.Select(u => new UserDto(
            u.Id,
            u.Name,
            u.Email,
            u.Role.ToString(),
            u.IsActive,
            u.SchoolId,
            u.CreatedAt
        ));
    }
}

public record InviteUserCommand(
    Guid SchoolId,
    string Name,
    string Email,
    UserRole Role
) : IRequest<string>;

public class InviteUserCommandHandler : IRequestHandler<InviteUserCommand, string>
{
    private readonly IUserRepository _userRepository;

    public InviteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<string> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new Exception("Utilizador com este email já existe no sistema.");
        }

        var token = Guid.NewGuid().ToString("N");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PasswordHash = string.Empty,
            Role = request.Role,
            SchoolId = request.SchoolId,
            IsActive = false,
            InvitationToken = token,
            InvitationExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        return token;
    }
}

public record AcceptInvitationCommand(
    string Token,
    string Password
) : IRequest<bool>;

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public AcceptInvitationCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByInvitationTokenAsync(request.Token);
        if (user == null)
        {
            throw new Exception("Convite inválido ou não encontrado.");
        }

        if (user.InvitationExpiresAt.HasValue && user.InvitationExpiresAt.Value < DateTime.UtcNow)
        {
            throw new Exception("O convite expirou. Solicite um novo convite ao administrador.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.IsActive = true;
        user.InvitationToken = null;
        user.InvitationExpiresAt = null;

        await _userRepository.UpdateAsync(user);
        return true;
    }
}

public record ToggleUserStatusCommand(Guid UserId) : IRequest<bool>;

public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, bool>
{
    private readonly IUserRepository _userRepository;

    public ToggleUserStatusCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> Handle(ToggleUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null) return false;

        // Regra de Proteção: Se estiver a tentar desativar o dono da escola (SchoolOwner)
        if (user.IsActive && user.Role == Domain.Enums.UserRole.SchoolOwner)
        {
            throw new Exception("Não é possível desativar o Proprietário Principal da escola. Transfira a propriedade antes de inativar este utilizador.");
        }

        user.IsActive = !user.IsActive;
        await _userRepository.UpdateAsync(user);
        return true;
    }
}
