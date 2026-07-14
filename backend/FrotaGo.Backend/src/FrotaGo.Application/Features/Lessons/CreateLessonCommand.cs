using System;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Lessons;

public record CreateLessonCommand(
    Guid StudentId,
    Guid InstructorId,
    Guid VehicleId,
    DateTime ScheduledDate,
    int DurationMinutes,
    string Topic,
    string Observations
) : IRequest<Guid>;

public class CreateLessonCommandHandler : IRequestHandler<CreateLessonCommand, Guid>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IVehicleRepository _vehicleRepository;

    public CreateLessonCommandHandler(ILessonRepository lessonRepository, IVehicleRepository vehicleRepository)
    {
        _lessonRepository = lessonRepository;
        _vehicleRepository = vehicleRepository;
    }

    public async Task<Guid> Handle(CreateLessonCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
        if (vehicle != null && (vehicle.Status == VehicleStatus.EmManutencao || vehicle.Status == VehicleStatus.ForaDeServico))
        {
            throw new Exception("Este veículo não está disponível para aulas práticas de condução.");
        }

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            StudentId = request.StudentId,
            InstructorId = request.InstructorId,
            VehicleId = request.VehicleId,
            ScheduledDate = request.ScheduledDate.ToUniversalTime(),
            DurationMinutes = request.DurationMinutes,
            Topic = request.Topic,
            Status = LessonStatus.Agendada,
            Observations = request.Observations,
            CreatedAt = DateTime.UtcNow
        };

        await _lessonRepository.AddAsync(lesson);
        return lesson.Id;
    }
}
