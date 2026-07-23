using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Students;

public record UpdateStudentProgressCommand(
    Guid StudentId,
    bool DocumentsSubmitted,
    bool RegistrationFeePaid,
    bool TheoryCompleted,
    bool PracticalLessonsStarted,
    bool ExamScheduled,
    StudentProgressStatus ProgressStatus
) : IRequest<bool>;

public class UpdateStudentProgressCommandHandler : IRequestHandler<UpdateStudentProgressCommand, bool>
{
    private readonly IStudentRepository _studentRepository;

    public UpdateStudentProgressCommandHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<bool> Handle(UpdateStudentProgressCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null) return false;

        student.DocumentsSubmitted = request.DocumentsSubmitted;
        student.RegistrationFeePaid = request.RegistrationFeePaid;
        student.TheoryCompleted = request.TheoryCompleted;
        student.PracticalLessonsStarted = request.PracticalLessonsStarted;
        student.ExamScheduled = request.ExamScheduled;
        student.ProgressStatus = request.ProgressStatus;

        await _studentRepository.UpdateAsync(student);
        return true;
    }
}
