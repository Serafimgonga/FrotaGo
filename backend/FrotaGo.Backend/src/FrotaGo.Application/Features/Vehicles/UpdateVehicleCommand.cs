using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Vehicles;

public record UpdateVehicleCommand(
    Guid Id,
    string LicensePlate,
    string Brand,
    string Model,
    string Chassis,
    int Year,
    int Odometer,
    FuelType Fuel,
    TransmissionType Transmission,
    VehicleStatus Status
) : IRequest<bool>;

public class UpdateVehicleCommandHandler : IRequestHandler<UpdateVehicleCommand, bool>
{
    private readonly IVehicleRepository _vehicleRepository;

    public UpdateVehicleCommandHandler(IVehicleRepository vehicleRepository)
    {
        _vehicleRepository = vehicleRepository;
    }

    public async Task<bool> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.Id);
        if (vehicle == null) return false;

        var existingPlate = await _vehicleRepository.GetByLicensePlateAsync(request.LicensePlate);
        if (existingPlate != null && existingPlate.Id != request.Id)
        {
            throw new Exception("Outro veículo já utiliza esta matrícula.");
        }

        vehicle.LicensePlate = request.LicensePlate;
        vehicle.Brand = request.Brand;
        vehicle.Model = request.Model;
        vehicle.Chassis = request.Chassis;
        vehicle.Year = request.Year;
        vehicle.Odometer = request.Odometer;
        vehicle.Fuel = request.Fuel;
        vehicle.Transmission = request.Transmission;
        vehicle.Status = request.Status;

        await _vehicleRepository.UpdateAsync(vehicle);
        return true;
    }
}
