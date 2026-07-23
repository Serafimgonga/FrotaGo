using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using MediatR;

namespace FrotaGo.Application.Features.Lessons;

public record AutoDispatchLessonCommand(
    Guid StudentId,
    DateTime ScheduledDate,
    int DurationMinutes,
    string Topic,
    string Observations
) : IRequest<Guid>;

public class AutoDispatchLessonCommandHandler : IRequestHandler<AutoDispatchLessonCommand, Guid>
{
    private readonly ILessonRepository _lessonRepository;
    private readonly IInstructorRepository _instructorRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IStudentRepository _studentRepository;

    public AutoDispatchLessonCommandHandler(
        ILessonRepository lessonRepository,
        IInstructorRepository instructorRepository,
        IVehicleRepository vehicleRepository,
        IStudentRepository studentRepository)
    {
        _lessonRepository = lessonRepository;
        _instructorRepository = instructorRepository;
        _vehicleRepository = vehicleRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Guid> Handle(AutoDispatchLessonCommand request, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(request.StudentId);
        if (student == null)
        {
            throw new Exception("Aluno não encontrado para realizar o despacho automático.");
        }

        var newStart = request.ScheduledDate.ToUniversalTime();
        var newEnd = newStart.AddMinutes(request.DurationMinutes);

        var allLessons = await _lessonRepository.GetAllAsync();
        var activeLessonsInPeriod = allLessons
            .Where(l => l.Status == LessonStatus.Agendada || l.Status == LessonStatus.EmCurso)
            .Where(l => newStart < l.ScheduledDate.AddMinutes(l.DurationMinutes) && newEnd > l.ScheduledDate)
            .ToList();

        var busyInstructorIds = activeLessonsInPeriod.Select(l => l.InstructorId).ToHashSet();
        var busyVehicleIds = activeLessonsInPeriod.Select(l => l.VehicleId).ToHashSet();

        // 1. Encontrar Viatura disponível
        var allVehicles = await _vehicleRepository.GetAllAsync();
        var availableVehicle = allVehicles
            .FirstOrDefault(v => v.Status == VehicleStatus.Disponivel && !busyVehicleIds.Contains(v.Id));

        if (availableVehicle == null)
        {
            throw new Exception("Despacho Automático: Nenhuma viatura disponível para este horário.");
        }

        // 2. Encontrar Instrutor disponível com menor carga de aulas no dia
        var dayStart = newStart.Date;
        var dayEnd = dayStart.AddDays(1);

        var lessonsOnDay = allLessons
            .Where(l => l.ScheduledDate >= dayStart && l.ScheduledDate < dayEnd && l.Status != LessonStatus.Cancelada)
            .ToList();

        var allInstructors = await _instructorRepository.GetAllAsync();
        var candidateInstructors = allInstructors
            .Where(i => i.IsActive && !busyInstructorIds.Contains(i.Id))
            .Select(i => new {
                Instructor = i,
                DailyCount = lessonsOnDay.Count(l => l.InstructorId == i.Id)
            })
            .OrderBy(x => x.DailyCount)
            .ToList();

        var selectedInstructor = candidateInstructors.FirstOrDefault()?.Instructor;

        if (selectedInstructor == null)
        {
            throw new Exception("Despacho Automático: Nenhum instrutor disponível para este horário.");
        }

        // 3. Criar a aula agendada
        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            StudentId = request.StudentId,
            InstructorId = selectedInstructor.Id,
            VehicleId = availableVehicle.Id,
            ScheduledDate = newStart,
            DurationMinutes = request.DurationMinutes,
            Topic = string.IsNullOrWhiteSpace(request.Topic) ? $"Aula Prática - {student.Category}" : request.Topic,
            Status = LessonStatus.Agendada,
            Observations = $"[Despacho Automático FrotaGo] {request.Observations}".Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _lessonRepository.AddAsync(lesson);
        return lesson.Id;
    }
}
