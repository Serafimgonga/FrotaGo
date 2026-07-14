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

    public CreateDocumentCommandHandler(IVehicleDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<Guid> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = new VehicleDocument
        {
            Id = Guid.NewGuid(),
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
