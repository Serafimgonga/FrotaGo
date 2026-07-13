using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Vehicles;

public record CreateVehicleCommand(
    string LicensePlate,
    string Brand,
    string Model,
    string Chassis,
    int Year,
    int Odometer,
    FuelType Fuel,
    TransmissionType Transmission
) : IRequest<Guid>;

public class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Guid>
{
    private readonly IVehicleRepository _vehicleRepository;

    public CreateVehicleCommandHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var existing = await _vehicleRepository.GetByLicensePlateAsync(request.LicensePlate);
        if (existing != null)
        {
            throw new Exception("Veículo com esta matrícula já existe.");
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            LicensePlate = request.LicensePlate,
            Brand = request.Brand,
            Model = request.Model,
            Chassis = request.Chassis,
            Year = request.Year,
            Odometer = request.Odometer,
            Fuel = request.Fuel,
            Transmission = request.Transmission,
            Status = VehicleStatus.Disponivel
        };

        await _vehicleRepository.AddAsync(vehicle);

        return vehicle.Id;
    }
}
