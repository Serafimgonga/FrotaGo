using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using MediatR;

namespace FrotaGo.Application.Features.Documents;

public record GetDocumentsQuery : IRequest<IEnumerable<VehicleDocument>>;

public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, IEnumerable<VehicleDocument>>
{
    private readonly IVehicleDocumentRepository _documentRepository;

    public GetDocumentsQueryHandler(IVehicleDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<IEnumerable<VehicleDocument>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        return await _documentRepository.GetAllAsync();
    }
}
