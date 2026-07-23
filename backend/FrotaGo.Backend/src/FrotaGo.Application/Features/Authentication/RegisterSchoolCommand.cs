using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Authentication;

public record RegisterSchoolCommand(
    // Passo 1 — Proprietário
    string OwnerName,
    string? Gender,
    string Phone,
    string Email,
    string? IdentityCardNumber,
    string Password,

    // Passo 2 — Escola de Condução
    string SchoolName,
    string ShortName,
    string Slug,
    string NIF,
    string LicenseNumber,
    string LicenseIssuer,
    DateTime? LicenseIssueDate,
    string? SchoolPhone,
    string? SchoolEmail,
    string? Website,
    string Province,
    string Municipality,
    string Address,
    double? Latitude,
    double? Longitude,

    // Passo 3 — Primeira Unidade (Sede)
    string BranchName,
    string? BranchPhone,
    string? BranchAddress,

    // Passo 4 — Plano SaaS Escolhido
    string Plan
) : IRequest<LoginResult>;

public class RegisterSchoolCommandHandler : IRequestHandler<RegisterSchoolCommand, LoginResult>
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterSchoolCommandHandler(
        ISchoolRepository schoolRepository,
        IUserRepository userRepository,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _schoolRepository = schoolRepository;
        _userRepository = userRepository;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResult> Handle(RegisterSchoolCommand request, CancellationToken cancellationToken)
    {
        var existingSchool = await _schoolRepository.GetByNifAsync(request.NIF);
        if (existingSchool != null)
        {
            throw new Exception("Já existe uma escola registada com este NIF.");
        }

        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new Exception("Já existe um utilizador registado com este email.");
        }

        var school = new School
        {
            Id = Guid.NewGuid(),
            Name = request.SchoolName,
            ShortName = string.IsNullOrWhiteSpace(request.ShortName) ? request.SchoolName : request.ShortName,
            Slug = string.IsNullOrWhiteSpace(request.Slug) ? request.SchoolName.ToLowerInvariant().Replace(" ", "-") : request.Slug.ToLowerInvariant(),
            NIF = request.NIF,
            LicenseNumber = request.LicenseNumber,
            LicenseIssuer = string.IsNullOrWhiteSpace(request.LicenseIssuer) ? "INATRO" : request.LicenseIssuer,
            LicenseIssueDate = request.LicenseIssueDate,
            Phone = string.IsNullOrWhiteSpace(request.SchoolPhone) ? request.Phone : request.SchoolPhone,
            Email = string.IsNullOrWhiteSpace(request.SchoolEmail) ? request.Email : request.SchoolEmail,
            Website = request.Website,
            Province = string.IsNullOrWhiteSpace(request.Province) ? "Luanda" : request.Province,
            Municipality = request.Municipality,
            Address = request.Address,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Plan = string.IsNullOrWhiteSpace(request.Plan) ? "Gratuito" : request.Plan,
            TrialExpiresAt = (string.IsNullOrWhiteSpace(request.Plan) || request.Plan == "Gratuito")
                ? DateTime.UtcNow.AddDays(30) : null, // Trial gratuito de 30 dias
            Status = (string.IsNullOrWhiteSpace(request.Plan) || request.Plan == "Gratuito")
                ? SchoolStatus.Approved // Plano Gratuito desbloqueia imediatamente por 30 dias
                : SchoolStatus.Pending,  // Planos pagos aguardam validação legal
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Adicionar primeira unidade (Sede)
        var primaryBranch = new Branch
        {
            Id = Guid.NewGuid(),
            SchoolId = school.Id,
            Name = string.IsNullOrWhiteSpace(request.BranchName) ? "Sede Principal" : request.BranchName,
            Phone = string.IsNullOrWhiteSpace(request.BranchPhone) ? school.Phone : request.BranchPhone,
            Address = string.IsNullOrWhiteSpace(request.BranchAddress) ? school.Address : request.BranchAddress,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CreatedAt = DateTime.UtcNow
        };

        school.Branches.Add(primaryBranch);

        await _schoolRepository.AddAsync(school);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.OwnerName,
            Gender = request.Gender,
            Phone = request.Phone,
            IdentityCardNumber = request.IdentityCardNumber,
            Email = request.Email,
            PasswordHash = passwordHash,
            Role = UserRole.SchoolOwner,
            SchoolId = school.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        var token = _jwtTokenGenerator.GenerateToken(user);

        var permissions = new List<string>
        {
            "MANAGE_USERS", "CREATE_STUDENT", "CREATE_LESSON", "MANAGE_VEHICLES", "VIEW_REPORTS", "VIEW_FINANCIAL"
        };

        return new LoginResult(
            token,
            user.Id,
            user.Name,
            user.Email,
            user.Role.ToString(),
            user.SchoolId,
            school.Name,
            school.Status.ToString(),
            permissions
        );
    }
}
