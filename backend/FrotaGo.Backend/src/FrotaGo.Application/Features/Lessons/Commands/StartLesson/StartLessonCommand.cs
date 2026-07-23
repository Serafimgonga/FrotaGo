using System;
using MediatR;

namespace FrotaGo.Application.Features.Lessons.Commands.StartLesson;

public record StartLessonCommand(Guid LessonId) : IRequest<Guid>;
