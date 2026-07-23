using System;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Lessons.Commands.FinishLesson;

public record FinishLessonCommand(
    Guid LessonId,
    LessonEvaluation Evaluation,
    string ExercisesCompletedJson,
    string Observations
) : IRequest<bool>;
