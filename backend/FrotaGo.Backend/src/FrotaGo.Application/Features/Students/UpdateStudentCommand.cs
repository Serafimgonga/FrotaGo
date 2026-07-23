using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Students;

public record UpdateStudentCommand(
    Guid Id,
    string Name,
    string Email,
    string PhoneNumber,
    string IdentityCardNumber,
    LicenseCategory Category,
    bool IsActive
) : IRequest<bool>;

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, bool>
{
    private readonly IStudentRepository _studentRepository;

    public UpdateStudentCommandHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<bool> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.Id);
        if (student == null) return false;

        student.Name = request.Name;
        student.Email = request.Email;
        student.PhoneNumber = request.PhoneNumber;
        student.IdentityCardNumber = request.IdentityCardNumber;
        student.Category = request.Category;
        student.IsActive = request.IsActive;

        await _studentRepository.UpdateAsync(student);
        return true;
    }
}

public record DeleteStudentCommand(Guid Id) : IRequest<bool>;

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, bool>
{
    private readonly IStudentRepository _studentRepository;

    public DeleteStudentCommandHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<bool> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.Id);
        if (student == null) return false;

        await _studentRepository.DeleteAsync(student);
        return true;
    }
}

public record GetStudentByIdQuery(Guid Id) : IRequest<Student?>;

public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, Student?>
{
    private readonly IStudentRepository _studentRepository;

    public GetStudentByIdQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Student?> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _studentRepository.GetByIdAsync(request.Id);
    }
}
