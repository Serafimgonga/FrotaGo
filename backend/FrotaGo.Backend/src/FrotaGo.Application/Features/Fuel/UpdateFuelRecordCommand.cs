using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using MediatR;

namespace FrotaGo.Application.Features.Fuel;

public record UpdateFuelRecordCommand(
    Guid Id,
    Guid VehicleId,
    decimal Litres,
    decimal CostPerLitre,
    decimal TotalCost,
    int Odometer,
    DateTime Date,
    string Location
) : IRequest<bool>;

public class UpdateFuelRecordCommandHandler : IRequestHandler<UpdateFuelRecordCommand, bool>
{
    private readonly IFuelRecordRepository _fuelRecordRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public UpdateFuelRecordCommandHandler(IFuelRecordRepository fuelRecordRepository, IVehicleRepository vehicleRepository)
    {
        _fuelRecordRepository = fuelRecordRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<bool> Handle(UpdateFuelRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await _fuelRecordRepository.GetByIdAsync(request.Id);
        if (record == null) return false;

        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle == null) throw new Exception("Veículo não encontrado.");

        record.VehicleId = request.VehicleId;
        record.Litres = request.Litres;
        record.CostPerLitre = request.CostPerLitre;
        record.TotalCost = request.TotalCost;
        record.Odometer = request.Odometer;
        record.Date = request.Date.ToUniversalTime();
        record.Location = request.Location;

        if (request.Odometer > vehicle.Odometer)
        {
            vehicle.Odometer = request.Odometer;
            await _vehicleRepository.UpdateAsync(vehicle);
        }

        await _fuelRecordRepository.UpdateAsync(record);
        return true;
    }
}

public record DeleteFuelRecordCommand(Guid Id) : IRequest<bool>;

public class DeleteFuelRecordCommandHandler : IRequestHandler<DeleteFuelRecordCommand, bool>
{
    private readonly IFuelRecordRepository _fuelRecordRepository;

    public DeleteFuelRecordCommandHandler(IFuelRecordRepository fuelRecordRepository)
    {
        _fuelRecordRepository = fuelRecordRepository;
    }

    public async Task<bool> Handle(DeleteFuelRecordCommand request, CancellationToken cancellationToken)
    {
        var record = await _fuelRecordRepository.GetByIdAsync(request.Id);
        if (record == null) return false;

        await _fuelRecordRepository.DeleteAsync(record);
        return true;
    }
}

public record GetFuelRecordByIdQuery(Guid Id) : IRequest<FuelRecord?>;

public class GetFuelRecordByIdQueryHandler : IRequestHandler<GetFuelRecordByIdQuery, FuelRecord?>
{
    private readonly IFuelRecordRepository _fuelRecordRepository;

    public GetFuelRecordByIdQueryHandler(IFuelRecordRepository fuelRecordRepository)
    {
        _fuelRecordRepository = fuelRecordRepository;
    }

    public async Task<FuelRecord?> Handle(GetFuelRecordByIdQuery request, CancellationToken cancellationToken)
    {
        return await _fuelRecordRepository.GetByIdAsync(request.Id);
    }
}
