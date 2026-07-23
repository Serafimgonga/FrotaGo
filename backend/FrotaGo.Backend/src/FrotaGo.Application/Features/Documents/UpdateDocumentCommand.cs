using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Documents;

public record UpdateDocumentCommand(
    Guid Id,
    Guid VehicleId,
    DocumentType Type,
    string DocumentNumber,
    DateTime ExpiryDate,
    DateTime IssueDate,
    string? FileUrl
) : IRequest<bool>;

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand, bool>
{
    private readonly IVehicleDocumentRepository _documentRepository;

    public UpdateDocumentCommandHandler(IVehicleDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<bool> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await _documentRepository.GetByIdAsync(request.Id);
        if (doc == null) return false;

        doc.VehicleId = request.VehicleId;
        doc.Type = request.Type;
        doc.DocumentNumber = request.DocumentNumber;
        doc.ExpiryDate = request.ExpiryDate.ToUniversalTime();
        doc.IssueDate = request.IssueDate.ToUniversalTime();
        doc.FileUrl = request.FileUrl;

        await _documentRepository.UpdateAsync(doc);
        return true;
    }
}

public record DeleteDocumentCommand(Guid Id) : IRequest<bool>;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, bool>
{
    private readonly IVehicleDocumentRepository _documentRepository;

    public DeleteDocumentCommandHandler(IVehicleDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<bool> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var doc = await _documentRepository.GetByIdAsync(request.Id);
        if (doc == null) return false;

        await _documentRepository.DeleteAsync(doc);
        return true;
    }
}

public record GetDocumentByIdQuery(Guid Id) : IRequest<VehicleDocument?>;

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, VehicleDocument?>
{
    private readonly IVehicleDocumentRepository _documentRepository;

    public GetDocumentByIdQueryHandler(IVehicleDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<VehicleDocument?> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _documentRepository.GetByIdAsync(request.Id);
    }
}
