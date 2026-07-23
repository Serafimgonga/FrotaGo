using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Features.Lessons;
using FrotaGo.Application.Features.Lessons.Commands.FinishLesson;
using FrotaGo.Application.Features.Lessons.Commands.StartLesson;
using FrotaGo.Application.Features.Students;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using Xunit;

namespace FrotaGo.Application.Tests;

public class PracticalLessonWorkflowTests
{
    private readonly FakeLessonRepository _lessonRepo = new();
    private readonly FakeStudentRepository _studentRepo = new();
    private readonly FakeInstructorRepository _instructorRepo = new();
    private readonly FakeVehicleRepository _vehicleRepo = new();

    public PracticalLessonWorkflowTests()
    {
        // Configurar dados fidedignos para teste
        var student = new Student
        {
            Id = Guid.NewGuid(),
            Name = "João Manuel",
            Category = LicenseCategory.B,
            RequiredLessonsCount = 20,
            CompletedLessonsCount = 0,
            ProgressStatus = StudentProgressStatus.Registered
        };
        _studentRepo.Students.Add(student);

        var instructor = new Instructor
        {
            Id = Guid.NewGuid(),
            Name = "Carlos Manuel",
            IsActive = true
        };
        _instructorRepo.Instructors.Add(instructor);

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Brand = "Toyota",
            Model = "Corolla",
            LicensePlate = "LD-00-11-AA",
            Status = VehicleStatus.Disponivel
        };
        _vehicleRepo.Vehicles.Add(vehicle);
    }

    [Fact]
    public async Task AutoDispatchLesson_ShouldFindAvailableInstructorAndVehicle_AndCreateScheduledLesson()
    {
        var student = _studentRepo.Students.First();
        var handler = new AutoDispatchLessonCommandHandler(_lessonRepo, _instructorRepo, _vehicleRepo, _studentRepo);

        var command = new AutoDispatchLessonCommand(
            student.Id,
            DateTime.UtcNow.AddHours(2),
            60,
            "Aula Prática - Arranque",
            "Atribuído via Despacho Inteligente"
        );

        var lessonId = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, lessonId);
        var createdLesson = _lessonRepo.Lessons.FirstOrDefault(l => l.Id == lessonId);
        Assert.NotNull(createdLesson);
        Assert.Equal(LessonStatus.Agendada, createdLesson.Status);
        Assert.Equal(student.Id, createdLesson.StudentId);
        Assert.Equal(_instructorRepo.Instructors.First().Id, createdLesson.InstructorId);
        Assert.Equal(_vehicleRepo.Vehicles.First().Id, createdLesson.VehicleId);
    }

    [Fact]
    public async Task FinishLesson_ShouldMarkCompleted_SetEvaluation_AndIncrementStudentCompletedLessons()
    {
        var student = _studentRepo.Students.First();
        var instructor = _instructorRepo.Instructors.First();
        var vehicle = _vehicleRepo.Vehicles.First();

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            InstructorId = instructor.Id,
            VehicleId = vehicle.Id,
            ScheduledDate = DateTime.UtcNow,
            DurationMinutes = 60,
            Topic = "Aula 1",
            Status = LessonStatus.EmCurso
        };
        _lessonRepo.Lessons.Add(lesson);

        var handler = new FinishLessonCommandHandler(_lessonRepo, _studentRepo);
        var command = new FinishLessonCommand(
            lesson.Id,
            LessonEvaluation.Excelente,
            "[\"Arranque\", \"Mudança\"]",
            "Excelente controlo de embraiagem"
        );

        var success = await handler.Handle(command, CancellationToken.None);

        Assert.True(success);
        Assert.Equal(LessonStatus.Realizada, lesson.Status);
        Assert.Equal(LessonEvaluation.Excelente, lesson.Evaluation);
        Assert.Equal(1, student.CompletedLessonsCount);
        Assert.True(student.PracticalLessonsStarted);
        Assert.Equal(StudentProgressStatus.PracticalTraining, student.ProgressStatus);
    }

    // Fakes para teste unitário rápido sem necessidade de BD real
    private class FakeLessonRepository : ILessonRepository
    {
        public List<Lesson> Lessons { get; } = new();
        public Task<Lesson?> GetByIdAsync(Guid id) => Task.FromResult(Lessons.FirstOrDefault(l => l.Id == id));
        public Task<IEnumerable<Lesson>> GetAllAsync() => Task.FromResult<IEnumerable<Lesson>>(Lessons);
        public Task AddAsync(Lesson entity) { Lessons.Add(entity); return Task.CompletedTask; }
        public Task UpdateAsync(Lesson entity) => Task.CompletedTask;
        public Task DeleteAsync(Lesson entity) { Lessons.Remove(entity); return Task.CompletedTask; }
    }

    private class FakeStudentRepository : IStudentRepository
    {
        public List<Student> Students { get; } = new();
        public Task<Student?> GetByIdAsync(Guid id) => Task.FromResult(Students.FirstOrDefault(s => s.Id == id));
        public Task<IEnumerable<Student>> GetAllAsync() => Task.FromResult<IEnumerable<Student>>(Students);
        public Task AddAsync(Student entity) { Students.Add(entity); return Task.CompletedTask; }
        public Task UpdateAsync(Student entity) => Task.CompletedTask;
        public Task DeleteAsync(Student entity) { Students.Remove(entity); return Task.CompletedTask; }
    }

    private class FakeInstructorRepository : IInstructorRepository
    {
        public List<Instructor> Instructors { get; } = new();
        public Task<Instructor?> GetByIdAsync(Guid id) => Task.FromResult(Instructors.FirstOrDefault(i => i.Id == id));
        public Task<IEnumerable<Instructor>> GetAllAsync() => Task.FromResult<IEnumerable<Instructor>>(Instructors);
        public Task AddAsync(Instructor entity) { Instructors.Add(entity); return Task.CompletedTask; }
        public Task UpdateAsync(Instructor entity) => Task.CompletedTask;
        public Task DeleteAsync(Instructor entity) { Instructors.Remove(entity); return Task.CompletedTask; }
    }

    private class FakeVehicleRepository : IVehicleRepository
    {
        public List<Vehicle> Vehicles { get; } = new();
        public Task<Vehicle?> GetByIdAsync(Guid id) => Task.FromResult(Vehicles.FirstOrDefault(v => v.Id == id));
        public Task<Vehicle?> GetByLicensePlateAsync(string plate) => Task.FromResult(Vehicles.FirstOrDefault(v => v.LicensePlate == plate));
        public Task<IEnumerable<Vehicle>> GetAllAsync() => Task.FromResult<IEnumerable<Vehicle>>(Vehicles);
        public Task AddAsync(Vehicle entity) { Vehicles.Add(entity); return Task.CompletedTask; }
        public Task UpdateAsync(Vehicle entity) => Task.CompletedTask;
        public Task DeleteAsync(Vehicle entity) { Vehicles.Remove(entity); return Task.CompletedTask; }
    }
}
