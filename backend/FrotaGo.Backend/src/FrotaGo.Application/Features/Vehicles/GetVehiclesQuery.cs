using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Vehicles;

public record GetVehiclesQuery : IRequest<IEnumerable<Vehicle>>;

public class GetVehiclesQueryHandler : IRequestHandler<GetVehiclesQuery, IEnumerable<Vehicle>>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly ILessonRepository _lessonRepository;

    public GetVehiclesQueryHandler(IVehicleRepository vehicleRepository, ILessonRepository lessonRepository)
    {
        _vehicleRepository = vehicleRepository;
        _lessonRepository = lessonRepository;
    }

    public async Task<IEnumerable<Vehicle>> Handle(GetVehiclesQuery request, CancellationToken cancellationToken)
    {
        var vehicles = await _vehicleRepository.GetAllAsync();
        var lessons = await _lessonRepository.GetAllAsync();
        var now = DateTime.UtcNow;

        var occupiedVehicleIds = lessons
            .Where(l => l.Status == LessonStatus.Agendada &&
                        l.ScheduledDate <= now &&
                        now <= l.ScheduledDate.AddMinutes(l.DurationMinutes))
            .Select(l => l.VehicleId)
            .ToHashSet();

        foreach (var vehicle in vehicles)
        {
            if (vehicle.Status == VehicleStatus.Disponivel && occupiedVehicleIds.Contains(vehicle.Id))
            {
                vehicle.Status = VehicleStatus.EmAula;
            }
        }

        return vehicles;
    }
}
