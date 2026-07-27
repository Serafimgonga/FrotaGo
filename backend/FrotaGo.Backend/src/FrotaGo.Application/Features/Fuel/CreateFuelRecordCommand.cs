using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using MediatR;

namespace FrotaGo.Application.Features.Fuel;

public record CreateFuelRecordCommand(
    Guid VehicleId,
    decimal Litres,
    decimal CostPerLitre,
    decimal TotalCost,
    int Odometer,
    DateTime Date,
    string Location
) : IRequest<Guid>;

public class CreateFuelRecordCommandHandler : IRequestHandler<CreateFuelRecordCommand, Guid>
{
    private readonly IFuelRecordRepository _fuelRecordRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ITenantProvider _tenantProvider;

    public CreateFuelRecordCommandHandler(IFuelRecordRepository fuelRecordRepository, IVehicleRepository vehicleRepository, ITenantProvider tenantProvider)
    {
        _fuelRecordRepository = fuelRecordRepository;
        _vehicleRepository = vehicleRepository;
        _tenantProvider = tenantProvider;
    }

    public async Task<Guid> Handle(CreateFuelRecordCommand request, CancellationToken cancellationToken)
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
        await _vehicleRepository.UpdateAsync(vehicle);

        var schoolId = _tenantProvider.SchoolId ?? vehicle.SchoolId;

        var fuelRecord = new FuelRecord
        {
            Id = Guid.NewGuid(),
            SchoolId = schoolId,
            VehicleId = request.VehicleId,
            Litres = request.Litres,
            CostPerLitre = request.CostPerLitre,
            TotalCost = request.TotalCost,
            Odometer = request.Odometer,
            Date = request.Date.ToUniversalTime(),
            Location = request.Location,
            CreatedAt = DateTime.UtcNow
        };

        await _fuelRecordRepository.AddAsync(fuelRecord);
        return fuelRecord.Id;
    }
}
