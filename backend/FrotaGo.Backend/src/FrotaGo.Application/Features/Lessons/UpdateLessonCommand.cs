using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Lessons;

public record UpdateLessonCommand(
    Guid Id,
    Guid StudentId,
    Guid InstructorId,
    Guid VehicleId,
    DateTime ScheduledDate,
    int DurationMinutes,
    string Topic,
    LessonStatus Status,
    string Observations
) : IRequest<bool>;

public class UpdateLessonCommandHandler : IRequestHandler<UpdateLessonCommand, bool>
{
    private readonly ILessonRepository _lessonRepository;

    public UpdateLessonCommandHandler(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<bool> Handle(UpdateLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetByIdAsync(request.Id);
        if (lesson == null) return false;

        lesson.StudentId = request.StudentId;
        lesson.InstructorId = request.InstructorId;
        lesson.VehicleId = request.VehicleId;
        lesson.ScheduledDate = request.ScheduledDate.ToUniversalTime();
        lesson.DurationMinutes = request.DurationMinutes;
        lesson.Topic = request.Topic;
        lesson.Status = request.Status;
        lesson.Observations = request.Observations;

        await _lessonRepository.UpdateAsync(lesson);
        return true;
    }
}

public record DeleteLessonCommand(Guid Id) : IRequest<bool>;

public class DeleteLessonCommandHandler : IRequestHandler<DeleteLessonCommand, bool>
{
    private readonly ILessonRepository _lessonRepository;

    public DeleteLessonCommandHandler(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<bool> Handle(DeleteLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetByIdAsync(request.Id);
        if (lesson == null) return false;

        await _lessonRepository.DeleteAsync(lesson);
        return true;
    }
}

public record GetLessonByIdQuery(Guid Id) : IRequest<Lesson?>;

public class GetLessonByIdQueryHandler : IRequestHandler<GetLessonByIdQuery, Lesson?>
{
    private readonly ILessonRepository _lessonRepository;

    public GetLessonByIdQueryHandler(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<Lesson?> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        return await _lessonRepository.GetByIdAsync(request.Id);
    }
}
