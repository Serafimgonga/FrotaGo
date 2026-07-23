using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Maintenance;

public record UpdateMaintenanceCommand(
    Guid Id,
    Guid VehicleId,
    string Description,
    decimal Cost,
    DateTime MaintenanceDate,
    MaintenanceType Type,
    MaintenanceStatus Status,
    int Odometer
) : IRequest<bool>;

public class UpdateMaintenanceCommandHandler : IRequestHandler<UpdateMaintenanceCommand, bool>
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public UpdateMaintenanceCommandHandler(IMaintenanceRepository maintenanceRepository, IVehicleRepository vehicleRepository)
    {
        _maintenanceRepository = maintenanceRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<bool> Handle(UpdateMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var maintenance = await _maintenanceRepository.GetByIdAsync(request.Id);
        if (maintenance == null) return false;

        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null) throw new Exception("Veículo não encontrado.");

        maintenance.VehicleId = request.VehicleId;
        maintenance.Description = request.Description;
        maintenance.Cost = request.Cost;
        maintenance.MaintenanceDate = request.MaintenanceDate.ToUniversalTime();
        maintenance.Type = request.Type;
        maintenance.Status = request.Status;
        maintenance.Odometer = request.Odometer;

        if (request.Status == MaintenanceStatus.Concluida)
        {
            vehicle.Status = VehicleStatus.Disponivel;
            await _vehicleRepository.UpdateAsync(vehicle);
        }
        else if (request.Status == MaintenanceStatus.EmProgresso)
        {
            vehicle.Status = VehicleStatus.EmManutencao;
            await _vehicleRepository.UpdateAsync(vehicle);
        }

        await _maintenanceRepository.UpdateAsync(maintenance);
        return true;
    }
}

public record DeleteMaintenanceCommand(Guid Id) : IRequest<bool>;

public class DeleteMaintenanceCommandHandler : IRequestHandler<DeleteMaintenanceCommand, bool>
{
    private readonly IMaintenanceRepository _maintenanceRepository;

    public DeleteMaintenanceCommandHandler(IMaintenanceRepository maintenanceRepository)
    {
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task<bool> Handle(DeleteMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var maintenance = await _maintenanceRepository.GetByIdAsync(request.Id);
        if (maintenance == null) return false;

        await _maintenanceRepository.DeleteAsync(maintenance);
        return true;
    }
}

public record GetMaintenanceByIdQuery(Guid Id) : IRequest<FrotaGo.Domain.Entities.Maintenance?>;

public class GetMaintenanceByIdQueryHandler : IRequestHandler<GetMaintenanceByIdQuery, FrotaGo.Domain.Entities.Maintenance?>
{
    private readonly IMaintenanceRepository _maintenanceRepository;

    public GetMaintenanceByIdQueryHandler(IMaintenanceRepository maintenanceRepository)
    {
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task<FrotaGo.Domain.Entities.Maintenance?> Handle(GetMaintenanceByIdQuery request, CancellationToken cancellationToken)
    {
        return await _maintenanceRepository.GetByIdAsync(request.Id);
    }
}
