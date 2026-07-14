using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Features.Documents;
using FrotaGo.Application.Features.Fuel;
using FrotaGo.Application.Features.Instructors;
using FrotaGo.Application.Features.Lessons;
using FrotaGo.Application.Features.Maintenance;
using FrotaGo.Application.Features.Students;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using Xunit;

namespace FrotaGo.Application.Tests;

public class FakeInstructorRepository : IInstructorRepository
{
    public readonly List<Instructor> Instructors = new();
    public Task<Instructor?> GetByIdAsync(Guid id) => Task.FromResult(Instructors.FirstOrDefault(x => x.Id == id));
    public Task<IEnumerable<Instructor>> GetAllAsync() => Task.FromResult<IEnumerable<Instructor>>(Instructors);
    public Task AddAsync(Instructor instructor) { Instructors.Add(instructor); return Task.CompletedTask; }
    public Task UpdateAsync(Instructor instructor) => Task.CompletedTask;
    public Task DeleteAsync(Instructor instructor) { Instructors.Remove(instructor); return Task.CompletedTask; }
}

public class FakeStudentRepository : IStudentRepository
{
    public readonly List<Student> Students = new();
    public Task<Student?> GetByIdAsync(Guid id) => Task.FromResult(Students.FirstOrDefault(x => x.Id == id));
    public Task<IEnumerable<Student>> GetAllAsync() => Task.FromResult<IEnumerable<Student>>(Students);
    public Task AddAsync(Student student) { Students.Add(student); return Task.CompletedTask; }
    public Task UpdateAsync(Student student) => Task.CompletedTask;
    public Task DeleteAsync(Student student) { Students.Remove(student); return Task.CompletedTask; }
}

public class FakeLessonRepository : ILessonRepository
{
    public readonly List<Lesson> Lessons = new();
    public Task<Lesson?> GetByIdAsync(Guid id) => Task.FromResult(Lessons.FirstOrDefault(x => x.Id == id));
    public Task<IEnumerable<Lesson>> GetAllAsync() => Task.FromResult<IEnumerable<Lesson>>(Lessons);
    public Task AddAsync(Lesson lesson) { Lessons.Add(lesson); return Task.CompletedTask; }
    public Task UpdateAsync(Lesson lesson) => Task.CompletedTask;
    public Task DeleteAsync(Lesson lesson) { Lessons.Remove(lesson); return Task.CompletedTask; }
}

public class FakeVehicleRepository : IVehicleRepository
{
    public readonly List<Vehicle> Vehicles = new();
    public Task<Vehicle?> GetByIdAsync(Guid id) => Task.FromResult(Vehicles.FirstOrDefault(x => x.Id == id));
    public Task<Vehicle?> GetByLicensePlateAsync(string licensePlate) => Task.FromResult(Vehicles.FirstOrDefault(x => x.LicensePlate == licensePlate));
    public Task<IEnumerable<Vehicle>> GetAllAsync() => Task.FromResult<IEnumerable<Vehicle>>(Vehicles);
    public Task AddAsync(Vehicle vehicle) { Vehicles.Add(vehicle); return Task.CompletedTask; }
    public Task UpdateAsync(Vehicle vehicle)
    {
        var existing = Vehicles.FirstOrDefault(x => x.Id == vehicle.Id);
        if (existing != null)
        {
            Vehicles.Remove(existing);
            Vehicles.Add(vehicle);
        }
        return Task.CompletedTask;
    }
    public Task DeleteAsync(Vehicle vehicle) { Vehicles.Remove(vehicle); return Task.CompletedTask; }
}

public class FakeMaintenanceRepository : IMaintenanceRepository
{
    public readonly List<Maintenance> Maintenances = new();
    public Task<Maintenance?> GetByIdAsync(Guid id) => Task.FromResult(Maintenances.FirstOrDefault(x => x.Id == id));
    public Task<IEnumerable<Maintenance>> GetAllAsync() => Task.FromResult<IEnumerable<Maintenance>>(Maintenances);
    public Task AddAsync(Maintenance maintenance) { Maintenances.Add(maintenance); return Task.CompletedTask; }
    public Task UpdateAsync(Maintenance maintenance) => Task.CompletedTask;
    public Task DeleteAsync(Maintenance maintenance) { Maintenances.Remove(maintenance); return Task.CompletedTask; }
}

public class FakeFuelRecordRepository : IFuelRecordRepository
{
    public readonly List<FuelRecord> FuelRecords = new();
    public Task<FuelRecord?> GetByIdAsync(Guid id) => Task.FromResult(FuelRecords.FirstOrDefault(x => x.Id == id));
    public Task<IEnumerable<FuelRecord>> GetAllAsync() => Task.FromResult<IEnumerable<FuelRecord>>(FuelRecords);
    public Task AddAsync(FuelRecord record) { FuelRecords.Add(record); return Task.CompletedTask; }
    public Task UpdateAsync(FuelRecord record) => Task.CompletedTask;
    public Task DeleteAsync(FuelRecord record) { FuelRecords.Remove(record); return Task.CompletedTask; }
}

public class FakeVehicleDocumentRepository : IVehicleDocumentRepository
{
    public readonly List<VehicleDocument> VehicleDocuments = new();
    public Task<VehicleDocument?> GetByIdAsync(Guid id) => Task.FromResult(VehicleDocuments.FirstOrDefault(x => x.Id == id));
    public Task<IEnumerable<VehicleDocument>> GetAllAsync() => Task.FromResult<IEnumerable<VehicleDocument>>(VehicleDocuments);
    public Task AddAsync(VehicleDocument document) { VehicleDocuments.Add(document); return Task.CompletedTask; }
    public Task UpdateAsync(VehicleDocument document) => Task.CompletedTask;
    public Task DeleteAsync(VehicleDocument document) { VehicleDocuments.Remove(document); return Task.CompletedTask; }
}

public class CrudFeaturesTests
{
    [Fact]
    public async Task CreateInstructor_ShouldAddInstructorToRepository()
    {
        // Arrange
        var repo = new FakeInstructorRepository();
        var handler = new CreateInstructorCommandHandler(repo);
        var command = new CreateInstructorCommand("Joao Instructor", "joao@frotago.com", "912345678", "LD-12345");

        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, resultId);
        var instructor = await repo.GetByIdAsync(resultId);
        Assert.NotNull(instructor);
        Assert.Equal("Joao Instructor", instructor.Name);
        Assert.True(instructor.IsActive);
    }

    [Fact]
    public async Task CreateStudent_ShouldAddStudentToRepository()
    {
        // Arrange
        var repo = new FakeStudentRepository();
        var handler = new CreateStudentCommandHandler(repo);
        var command = new CreateStudentCommand("Maria Student", "maria@frotago.com", "911111111", "00123456AB789", LicenseCategory.B);

        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, resultId);
        var student = await repo.GetByIdAsync(resultId);
        Assert.NotNull(student);
        Assert.Equal("Maria Student", student.Name);
        Assert.Equal(LicenseCategory.B, student.Category);
    }

    [Fact]
    public async Task CreateLesson_WithAvailableVehicle_ShouldScheduleSuccessfully()
    {
        // Arrange
        var lessonRepo = new FakeLessonRepository();
        var vehicleRepo = new FakeVehicleRepository();
        
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle { Id = vehicleId, LicensePlate = "LD-00-11-AA", Odometer = 1000, Status = VehicleStatus.Disponivel };
        await vehicleRepo.AddAsync(vehicle);

        var handler = new CreateLessonCommandHandler(lessonRepo, vehicleRepo);
        var command = new CreateLessonCommand(Guid.NewGuid(), Guid.NewGuid(), vehicleId, DateTime.UtcNow, 60, "Controle de Embraiagem", "Primeira aula");

        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, resultId);
        var lesson = await lessonRepo.GetByIdAsync(resultId);
        Assert.NotNull(lesson);
        Assert.Equal(LessonStatus.Agendada, lesson.Status);
    }

    [Fact]
    public async Task CreateLesson_WithVehicleInMaintenance_ShouldThrowException()
    {
        // Arrange
        var lessonRepo = new FakeLessonRepository();
        var vehicleRepo = new FakeVehicleRepository();
        
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle { Id = vehicleId, LicensePlate = "LD-00-11-AA", Odometer = 1000, Status = VehicleStatus.EmManutencao };
        await vehicleRepo.AddAsync(vehicle);

        var handler = new CreateLessonCommandHandler(lessonRepo, vehicleRepo);
        var command = new CreateLessonCommand(Guid.NewGuid(), Guid.NewGuid(), vehicleId, DateTime.UtcNow, 60, "Controle de Embraiagem", "Primeira aula");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("não está disponível para aulas", ex.Message);
    }

    [Fact]
    public async Task CreateMaintenance_WithValidOdometer_ShouldUpdateVehicleOdometerAndStatus()
    {
        // Arrange
        var maintRepo = new FakeMaintenanceRepository();
        var vehicleRepo = new FakeVehicleRepository();
        
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle { Id = vehicleId, LicensePlate = "LD-00-11-AA", Odometer = 50000, Status = VehicleStatus.Disponivel };
        await vehicleRepo.AddAsync(vehicle);

        var handler = new CreateMaintenanceCommandHandler(maintRepo, vehicleRepo);
        var command = new CreateMaintenanceCommand(vehicleId, "Revisao Geral", 150000, DateTime.UtcNow, MaintenanceType.Preventiva, MaintenanceStatus.EmProgresso, 50500);

        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, resultId);
        var updatedVehicle = await vehicleRepo.GetByIdAsync(vehicleId);
        Assert.NotNull(updatedVehicle);
        Assert.Equal(50500, updatedVehicle.Odometer);
        Assert.Equal(VehicleStatus.EmManutencao, updatedVehicle.Status);
    }

    [Fact]
    public async Task CreateMaintenance_WithLowerOdometer_ShouldThrowException()
    {
        // Arrange
        var maintRepo = new FakeMaintenanceRepository();
        var vehicleRepo = new FakeVehicleRepository();
        
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle { Id = vehicleId, LicensePlate = "LD-00-11-AA", Odometer = 50000, Status = VehicleStatus.Disponivel };
        await vehicleRepo.AddAsync(vehicle);

        var handler = new CreateMaintenanceCommandHandler(maintRepo, vehicleRepo);
        var command = new CreateMaintenanceCommand(vehicleId, "Revisao Geral", 150000, DateTime.UtcNow, MaintenanceType.Preventiva, MaintenanceStatus.EmProgresso, 49999);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("não pode ser inferior", ex.Message);
    }

    [Fact]
    public async Task CreateFuelRecord_WithValidOdometer_ShouldUpdateVehicleOdometer()
    {
        // Arrange
        var fuelRepo = new FakeFuelRecordRepository();
        var vehicleRepo = new FakeVehicleRepository();
        
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle { Id = vehicleId, LicensePlate = "LD-00-11-AA", Odometer = 12000, Status = VehicleStatus.Disponivel };
        await vehicleRepo.AddAsync(vehicle);

        var handler = new CreateFuelRecordCommandHandler(fuelRepo, vehicleRepo);
        var command = new CreateFuelRecordCommand(vehicleId, 45, 300, 13500, 12200, DateTime.UtcNow, "Pumangol");

        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, resultId);
        var updatedVehicle = await vehicleRepo.GetByIdAsync(vehicleId);
        Assert.NotNull(updatedVehicle);
        Assert.Equal(12200, updatedVehicle.Odometer);
    }

    [Fact]
    public async Task CreateFuelRecord_WithLowerOdometer_ShouldThrowException()
    {
        // Arrange
        var fuelRepo = new FakeFuelRecordRepository();
        var vehicleRepo = new FakeVehicleRepository();
        
        var vehicleId = Guid.NewGuid();
        var vehicle = new Vehicle { Id = vehicleId, LicensePlate = "LD-00-11-AA", Odometer = 12000, Status = VehicleStatus.Disponivel };
        await vehicleRepo.AddAsync(vehicle);

        var handler = new CreateFuelRecordCommandHandler(fuelRepo, vehicleRepo);
        var command = new CreateFuelRecordCommand(vehicleId, 45, 300, 13500, 11999, DateTime.UtcNow, "Pumangol");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("não pode ser inferior", ex.Message);
    }

    [Fact]
    public async Task CreateDocument_ShouldAddDocumentToRepository()
    {
        // Arrange
        var repo = new FakeVehicleDocumentRepository();
        var handler = new CreateDocumentCommandHandler(repo);
        var command = new CreateDocumentCommand(Guid.NewGuid(), DocumentType.Seguro, "SEG-12345", DateTime.UtcNow.AddDays(365), DateTime.UtcNow, null);

        // Act
        var resultId = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, resultId);
        var doc = await repo.GetByIdAsync(resultId);
        Assert.NotNull(doc);
        Assert.Equal("SEG-12345", doc.DocumentNumber);
        Assert.Equal(DocumentType.Seguro, doc.Type);
    }
}
