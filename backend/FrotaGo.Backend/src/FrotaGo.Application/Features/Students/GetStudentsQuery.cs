using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using MediatR;

namespace FrotaGo.Application.Features.Students;

public record GetStudentsQuery : IRequest<IEnumerable<Student>>;

public class GetStudentsQueryHandler : IRequestHandler<GetStudentsQuery, IEnumerable<Student>>
{
    private readonly IStudentRepository _studentRepository;

    public GetStudentsQueryHandler(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<IEnumerable<Student>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
    {
        return await _studentRepository.GetAllAsync();
    }
}
