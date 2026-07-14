using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Students;

public record CreateStudentCommand(
    string Name,
    string Email,
    string PhoneNumber,
    string IdentityCardNumber,
    LicenseCategory Category
) : IRequest<Guid>;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Guid>
{
    private readonly IStudentRepository _studentRepository;

    public CreateStudentCommandHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Guid> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            IdentityCardNumber = request.IdentityCardNumber,
            Category = request.Category,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _studentRepository.AddAsync(student);
        return student.Id;
    }
}
