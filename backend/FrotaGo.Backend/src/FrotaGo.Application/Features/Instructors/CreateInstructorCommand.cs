using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using MediatR;

namespace FrotaGo.Application.Features.Instructors;

public record CreateInstructorCommand(
    string Name,
    string Email,
    string PhoneNumber,
    string LicenseNumber
) : IRequest<Guid>;

public class CreateInstructorCommandHandler : IRequestHandler<CreateInstructorCommand, Guid>
{
    private readonly IInstructorRepository _instructorRepository;
    private readonly ITenantProvider _tenantProvider;

    public CreateInstructorCommandHandler(IInstructorRepository instructorRepository, ITenantProvider tenantProvider)
    {
        _instructorRepository = instructorRepository;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateInstructorCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenantProvider.SchoolId 
            ?? throw new Exception("Não foi possível determinar a escola do utilizador.");

        var instructor = new Instructor
        {
            Id = Guid.NewGuid(),
            SchoolId = schoolId,
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            LicenseNumber = request.LicenseNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _instructorRepository.AddAsync(instructor);
        return instructor.Id;
    }
}
