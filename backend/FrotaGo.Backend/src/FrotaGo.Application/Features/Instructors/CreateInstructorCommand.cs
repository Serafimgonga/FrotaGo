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

    public CreateInstructorCommandHandler(IInstructorRepository instructorRepository)
    {
        _instructorRepository = instructorRepository;
    }

    public async Task<Guid> Handle(CreateInstructorCommand request, CancellationToken cancellationToken)
    {
        var instructor = new Instructor
        {
            Id = Guid.NewGuid(),
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
