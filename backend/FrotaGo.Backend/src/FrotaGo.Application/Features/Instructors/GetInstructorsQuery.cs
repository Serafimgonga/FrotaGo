using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using MediatR;

namespace FrotaGo.Application.Features.Instructors;

public record GetInstructorsQuery : IRequest<IEnumerable<Instructor>>;

public class GetInstructorsQueryHandler : IRequestHandler<GetInstructorsQuery, IEnumerable<Instructor>>
{
    private readonly IInstructorRepository _instructorRepository;

    public GetInstructorsQueryHandler(IInstructorRepository instructorRepository)
    {
        _instructorRepository = instructorRepository;
    }

    public async Task<IEnumerable<Instructor>> Handle(GetInstructorsQuery request, CancellationToken cancellationToken)
    {
        return await _instructorRepository.GetAllAsync();
    }
}
