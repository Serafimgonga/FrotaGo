using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Documents;

public record CreateDocumentCommand(
    Guid VehicleId,
    DocumentType Type,
    string DocumentNumber,
    DateTime ExpiryDate,
    DateTime IssueDate,
    string? FileUrl
) : IRequest<Guid>;

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, Guid>
{
    private readonly IVehicleDocumentRepository _documentRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITenantProvider _tenantProvider;

    public CreateDocumentCommandHandler(
        IVehicleDocumentRepository documentRepository,
        IVehicleRepository vehicleRepository,
        ITenantProvider tenantProvider)
    {
        _documentRepository = documentRepository;
        _vehicleRepository = vehicleRepository;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        var schoolId = _tenantProvider.SchoolId ?? vehicle?.SchoolId ?? throw new Exception("Não foi possível determinar a escola do veículo.");

        var document = new VehicleDocument
        {
            Id = Guid.NewGuid(),
            SchoolId = schoolId,
            VehicleId = request.VehicleId,
            Type = request.Type,
            DocumentNumber = request.DocumentNumber,
            ExpiryDate = request.ExpiryDate.ToUniversalTime(),
            IssueDate = request.IssueDate.ToUniversalTime(),
            FileUrl = request.FileUrl,
            CreatedAt = DateTime.UtcNow
        };

        await _documentRepository.AddAsync(document);
        return document.Id;
    }
}
