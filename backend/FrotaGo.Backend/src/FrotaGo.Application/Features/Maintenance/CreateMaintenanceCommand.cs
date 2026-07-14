using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Maintenance;

public record CreateMaintenanceCommand(
    Guid VehicleId,
    string Description,
    decimal Cost,
    DateTime MaintenanceDate,
    MaintenanceType Type,
    MaintenanceStatus Status,
    int Odometer
) : IRequest<Guid>;

public class CreateMaintenanceCommandHandler : IRequestHandler<CreateMaintenanceCommand, Guid>
{
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public CreateMaintenanceCommandHandler(IMaintenanceRepository maintenanceRepository, IVehicleRepository vehicleRepository)
    {
        _maintenanceRepository = maintenanceRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Guid> Handle(CreateMaintenanceCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            throw new Exception("Veículo não encontrado.");
        }

        if (request.Odometer < vehicle.Odometer)
        {
            throw new Exception($"A quilometragem informada ({request.Odometer} km) não pode ser inferior à quilometragem atual do veículo ({vehicle.Odometer} km).");
        }

        vehicle.Odometer = request.Odometer;
        if (request.Status == MaintenanceStatus.EmProgresso || request.Status == MaintenanceStatus.Agendada)
        {
            vehicle.Status = VehicleStatus.EmManutencao;
        }
        else if (request.Status == MaintenanceStatus.Concluida)
        {
            vehicle.Status = VehicleStatus.Disponivel;
        }

        await _vehicleRepository.UpdateAsync(vehicle);

        var maintenance = new FrotaGo.Domain.Entities.Maintenance
        {
            Id = Guid.NewGuid(),
            VehicleId = request.VehicleId,
            Description = request.Description,
            Cost = request.Cost,
            MaintenanceDate = request.MaintenanceDate.ToUniversalTime(),
            Type = request.Type,
            Status = request.Status,
            Odometer = request.Odometer,
            CreatedAt = DateTime.UtcNow
        };

        await _maintenanceRepository.AddAsync(maintenance);
        return maintenance.Id;
    }
}
