using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using MediatR;

namespace FrotaGo.Application.Features.Lessons;

public record GetLessonsQuery : IRequest<IEnumerable<Lesson>>;

public class GetLessonsQueryHandler : IRequestHandler<GetLessonsQuery, IEnumerable<Lesson>>
{
    private readonly ILessonRepository _lessonRepository;

    public GetLessonsQueryHandler(ILessonRepository lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<IEnumerable<Lesson>> Handle(GetLessonsQuery request, CancellationToken cancellationToken)
    {
        return await _lessonRepository.GetAllAsync();
    }
}
