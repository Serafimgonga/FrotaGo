using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Lessons.Commands.FinishLesson;

public class FinishLessonCommandHandler : IRequestHandler<FinishLessonCommand, bool>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IStudentRepository _studentRepository;

    public FinishLessonCommandHandler(
        ILessonRepository lessonRepository,
        IStudentRepository studentRepository)
    {
        _lessonRepository = lessonRepository;
        _studentRepository = studentRepository;
    }

    public async Task<bool> Handle(FinishLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _lessonRepository.GetByIdAsync(request.LessonId);
        if (lesson == null)
        {
            throw new Exception("Aula prática não encontrada.");
        }

        if (lesson.Status == LessonStatus.Realizada)
        {
            throw new Exception("Esta aula já foi finalizada anteriormente.");
        }

        lesson.Status = LessonStatus.Realizada;
        lesson.CompletedAt = DateTime.UtcNow;
        lesson.Evaluation = request.Evaluation;
        if (!string.IsNullOrWhiteSpace(request.ExercisesCompletedJson))
        {
            lesson.ExercisesCompletedJson = request.ExercisesCompletedJson;
        }
        if (!string.IsNullOrWhiteSpace(request.Observations))
        {
            lesson.Observations = request.Observations;
        }

        await _lessonRepository.UpdateAsync(lesson);

        // Atualizar automaticamente a esteira de progresso do aluno
        var student = await _studentRepository.GetByIdAsync(lesson.StudentId);
        if (student != null)
        {
            student.CompletedLessonsCount++;
            student.PracticalLessonsStarted = true;

            if (student.ProgressStatus == StudentProgressStatus.Registered || student.ProgressStatus == StudentProgressStatus.TheoreticalTraining)
            {
                student.ProgressStatus = StudentProgressStatus.PracticalTraining;
            }

            if (student.CompletedLessonsCount >= student.RequiredLessonsCount)
            {
                student.ProgressStatus = StudentProgressStatus.ExamReady;
            }

            await _studentRepository.UpdateAsync(student);
        }

        return true;
    }
}
