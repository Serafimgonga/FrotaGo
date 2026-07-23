using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Accidents;

public record CreateAccidentCommand(
    Guid VehicleId,
    DateTime Date,
    string Description,
    AccidentSeverity Severity,
    decimal EstimatedCost,
    string Location
) : IRequest<Guid>;

public class CreateAccidentCommandHandler : IRequestHandler<CreateAccidentCommand, Guid>
{
    private readonly IAccidentRepository _accidentRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public CreateAccidentCommandHandler(IAccidentRepository accidentRepository, IVehicleRepository vehicleRepository)
    {
        _accidentRepository = accidentRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Guid> Handle(CreateAccidentCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null) throw new Exception("Veículo não encontrado.");

        // Atualizar estado do veículo para Acidentado
        vehicle.Status = VehicleStatus.Acidentado;
        await _vehicleRepository.UpdateAsync(vehicle);

        var accident = new Accident
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId,
            Date = request.Date.ToUniversalTime(),
            Description = request.Description,
            Severity = request.Severity,
            EstimatedCost = request.EstimatedCost,
            Location = request.Location,
            Status = AccidentStatus.Pendente,
            CreatedAt = DateTime.UtcNow
        };

        await _accidentRepository.AddAsync(accident);
        return accident.Id;
    }
}

public record GetAccidentsQuery : IRequest<IEnumerable<Accident>>;

public class GetAccidentsQueryHandler : IRequestHandler<GetAccidentsQuery, IEnumerable<Accident>>
{
    private readonly IAccidentRepository _accidentRepository;

    public GetAccidentsQueryHandler(IAccidentRepository accidentRepository)
    {
        _accidentRepository = accidentRepository;
    }

    public async Task<IEnumerable<Accident>> Handle(GetAccidentsQuery request, CancellationToken cancellationToken)
    {
        return await _accidentRepository.GetAllAsync();
    }
}

public record GetAccidentByIdQuery(Guid Id) : IRequest<Accident?>;

public class GetAccidentByIdQueryHandler : IRequestHandler<GetAccidentByIdQuery, Accident?>
{
    private readonly IAccidentRepository _accidentRepository;

    public GetAccidentByIdQueryHandler(IAccidentRepository accidentRepository)
    {
        _accidentRepository = accidentRepository;
    }

    public async Task<Accident?> Handle(GetAccidentByIdQuery request, CancellationToken cancellationToken)
    {
        return await _accidentRepository.GetByIdAsync(request.Id);
    }
}

public record UpdateAccidentCommand(
    Guid Id,
    Guid VehicleId,
    DateTime Date,
    string Description,
    AccidentSeverity Severity,
    decimal EstimatedCost,
    string Location,
    AccidentStatus Status
) : IRequest<bool>;

public class UpdateAccidentCommandHandler : IRequestHandler<UpdateAccidentCommand, bool>
{
    private readonly IAccidentRepository _accidentRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public UpdateAccidentCommandHandler(IAccidentRepository accidentRepository, IVehicleRepository vehicleRepository)
    {
        _accidentRepository = accidentRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<bool> Handle(UpdateAccidentCommand request, CancellationToken cancellationToken)
    {
        var accident = await _accidentRepository.GetByIdAsync(request.Id);
        if (accident == null) return false;

        accident.VehicleId = request.VehicleId;
        accident.Date = request.Date.ToUniversalTime();
        accident.Description = request.Description;
        accident.Severity = request.Severity;
        accident.EstimatedCost = request.EstimatedCost;
        accident.Location = request.Location;
        accident.Status = request.Status;

        if (request.Status == AccidentStatus.Resolvido)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
            if (vehicle != null && vehicle.Status == VehicleStatus.Acidentado)
            {
                vehicle.Status = VehicleStatus.Disponivel;
                await _vehicleRepository.UpdateAsync(vehicle);
            }
        }

        await _accidentRepository.UpdateAsync(accident);
        return true;
    }
}

public record DeleteAccidentCommand(Guid Id) : IRequest<bool>;

public class DeleteAccidentCommandHandler : IRequestHandler<DeleteAccidentCommand, bool>
{
    private readonly IAccidentRepository _accidentRepository;

    public DeleteAccidentCommandHandler(IAccidentRepository accidentRepository)
    {
        _accidentRepository = accidentRepository;
    }

    public async Task<bool> Handle(DeleteAccidentCommand request, CancellationToken cancellationToken)
    {
        var accident = await _accidentRepository.GetByIdAsync(request.Id);
        if (accident == null) return false;

        await _accidentRepository.DeleteAsync(accident);
        return true;
    }
}
