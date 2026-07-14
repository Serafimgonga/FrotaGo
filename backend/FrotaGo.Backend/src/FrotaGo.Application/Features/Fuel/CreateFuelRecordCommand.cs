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

    public CreateFuelRecordCommandHandler(IFuelRecordRepository fuelRecordRepository, IVehicleRepository vehicleRepository)
    {
        _fuelRecordRepository = fuelRecordRepository;
        _vehicleRepository = vehicleRepository;
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

        var fuelRecord = new FuelRecord
        {
            Id = Guid.NewGuid(),
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
