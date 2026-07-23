using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Lessons.Commands.StartLesson;

public class StartLessonCommandHandler : IRequestHandler<StartLessonCommand, Guid>
{
    private readonly ILessonRepository _lessonRepository;

    public StartLessonCommandHandler(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<Guid> Handle(StartLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetByIdAsync(request.LessonId);
        if (lesson == null)
        {
            throw new Exception("Aula prática não encontrada.");
        }

        if (lesson.Status == LessonStatus.Realizada)
        {
            throw new Exception("Esta aula já se encontra realizada.");
        }

        if (lesson.Status == LessonStatus.Cancelada)
        {
            throw new Exception("Não é possível iniciar uma aula cancelada.");
        }

        lesson.Status = LessonStatus.EmCurso;
        lesson.StartedAt = DateTime.UtcNow;

        await _lessonRepository.UpdateAsync(lesson);
        return lesson.Id;
    }
}
