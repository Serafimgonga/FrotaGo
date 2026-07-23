using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using MediatR;

namespace FrotaGo.Application.Features.Instructors;

public record UpdateInstructorCommand(
    Guid Id,
    string Name,
    string Email,
    string PhoneNumber,
    string LicenseNumber,
    bool IsActive
) : IRequest<bool>;

public class UpdateInstructorCommandHandler : IRequestHandler<UpdateInstructorCommand, bool>
{
    private readonly IInstructorRepository _instructorRepository;

    public UpdateInstructorCommandHandler(IInstructorRepository instructorRepository)
    {
        _instructorRepository = instructorRepository;
    }

    public async Task<bool> Handle(UpdateInstructorCommand request, CancellationToken cancellationToken)
    {
        var instructor = await _instructorRepository.GetByIdAsync(request.Id);
        if (instructor == null) return false;

        instructor.Name = request.Name;
        instructor.Email = request.Email;
        instructor.PhoneNumber = request.PhoneNumber;
        instructor.LicenseNumber = request.LicenseNumber;
        instructor.IsActive = request.IsActive;

        await _instructorRepository.UpdateAsync(instructor);
        return true;
    }
}

public record DeleteInstructorCommand(Guid Id) : IRequest<bool>;

public class DeleteInstructorCommandHandler : IRequestHandler<DeleteInstructorCommand, bool>
{
    private readonly IInstructorRepository _instructorRepository;

    public DeleteInstructorCommandHandler(IInstructorRepository instructorRepository)
    {
        _instructorRepository = instructorRepository;
    }

    public async Task<bool> Handle(DeleteInstructorCommand request, CancellationToken cancellationToken)
    {
        var instructor = await _instructorRepository.GetByIdAsync(request.Id);
        if (instructor == null) return false;

        await _instructorRepository.DeleteAsync(instructor);
        return true;
    }
}

public record GetInstructorByIdQuery(Guid Id) : IRequest<Instructor?>;

public class GetInstructorByIdQueryHandler : IRequestHandler<GetInstructorByIdQuery, Instructor?>
{
    private readonly IInstructorRepository _instructorRepository;

    public GetInstructorByIdQueryHandler(IInstructorRepository instructorRepository)
    {
        _instructorRepository = instructorRepository;
    }

    public async Task<Instructor?> Handle(GetInstructorByIdQuery request, CancellationToken cancellationToken)
    {
        return await _instructorRepository.GetByIdAsync(request.Id);
    }
}
