using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Lessons;

public record AvailableResourcesDto(
    List<Instructor> AvailableInstructors,
    List<Vehicle> AvailableVehicles
);

public record GetAvailableResourcesQuery(
    DateTime ScheduledDate,
    int DurationMinutes,
    LicenseCategory? Category = null
) : IRequest<AvailableResourcesDto>;

public class GetAvailableResourcesQueryHandler : IRequestHandler<GetAvailableResourcesQuery, AvailableResourcesDto>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public GetAvailableResourcesQueryHandler(
        ILessonRepository lessonRepository,
        IInstructorRepository instructorRepository,
        IVehicleRepository vehicleRepository)
    {
        _lessonRepository = lessonRepository;
        _instructorRepository = instructorRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<AvailableResourcesDto> Handle(GetAvailableResourcesQuery request, CancellationToken cancellationToken)
    {
        var newStart = request.ScheduledDate.ToUniversalTime();
        var newEnd = newStart.AddMinutes(request.DurationMinutes);

        var allLessons = await _lessonRepository.GetAllAsync();
        var activeLessonsInPeriod = allLessons
            .Where(l => l.Status == LessonStatus.Agendada || l.Status == LessonStatus.EmCurso)
            .Where(l => newStart < l.ScheduledDate.AddMinutes(l.DurationMinutes) && newEnd > l.ScheduledDate)
            .ToList();

        var busyInstructorIds = activeLessonsInPeriod.Select(l => l.InstructorId).ToHashSet();
        var busyVehicleIds = activeLessonsInPeriod.Select(l => l.VehicleId).ToHashSet();

        var allInstructors = await _instructorRepository.GetAllAsync();
        var availableInstructors = allInstructors
            .Where(i => i.IsActive && !busyInstructorIds.Contains(i.Id))
            .ToList();

        var allVehicles = await _vehicleRepository.GetAllAsync();
        var availableVehicles = allVehicles
            .Where(v => (v.Status == VehicleStatus.Disponivel || v.Status == VehicleStatus.EmAula) && !busyVehicleIds.Contains(v.Id))
            .ToList();

        return new AvailableResourcesDto(availableInstructors, availableVehicles);
    }
}
