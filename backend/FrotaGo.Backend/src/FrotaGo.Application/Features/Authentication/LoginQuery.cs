using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using MediatR;

namespace FrotaGo.Application.Features.Authentication;

public record LoginQuery(
    string Email,
    string Password
) : IRequest<LoginResult>;

public record LoginResult(
    string Token,
    Guid UserId,
    string Name,
    string Email,
    string Role,
    Guid? SchoolId,
    string? SchoolName,
    string? SchoolStatus,
    List<string> Permissions
);

public class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginQueryHandler(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResult> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new Exception("Credenciais inválidas.");
        }

        if (!user.IsActive)
        {
            throw new Exception("Conta inativa ou pendente de ativação por convite.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            throw new Exception("Credenciais inválidas.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        var permissions = GetPermissionsForRole(user.Role.ToString());

        return new LoginResult(
            token,
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString(),
            user.SchoolId,
            user.School?.Name,
            user.School?.Status.ToString(),
            permissions
        );
    }

    public static List<string> GetPermissionsForRole(string role)
    {
        return role switch
        {
            "SuperAdmin" => new List<string>
            {
                "School.Manage", "Users.Manage", "Students.Create", "Students.Edit", "Students.View",
                "Lessons.Create", "Lessons.Assign", "Lessons.Execute", "Vehicles.Manage", "Vehicles.ViewGps",
                "Payments.View", "Payments.Create", "Reports.Full"
            },
            "SchoolOwner" => new List<string>
            {
                "School.Manage", "Users.Manage", "Students.Create", "Students.Edit", "Students.View",
                "Lessons.Create", "Lessons.Assign", "Vehicles.Manage", "Vehicles.ViewGps",
                "Payments.View", "Payments.Create", "Reports.Full"
            },
            "SchoolAdmin" => new List<string>
            {
                "Users.Manage", "Students.Create", "Students.Edit", "Students.View",
                "Lessons.Create", "Lessons.Assign", "Vehicles.Manage", "Vehicles.ViewGps",
                "Payments.View", "Reports.Operational"
            },
            "Receptionist" => new List<string>
            {
                "Students.Create", "Students.Edit", "Students.View",
                "Lessons.Create", "Lessons.Assign", "Reports.Basic"
            },
            "Instructor" => new List<string>
            {
                "Lessons.Execute", "Vehicles.ViewGps", "Reports.Personal"
            },
            "Financial" => new List<string>
            {
                "Payments.View", "Payments.Create", "Reports.Financial"
            },
            _ => new List<string>()
        };
    }
}
