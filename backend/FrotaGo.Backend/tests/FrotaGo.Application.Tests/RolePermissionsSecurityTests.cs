using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Features.Authentication;
using FrotaGo.Application.Features.Users;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using Xunit;

namespace FrotaGo.Application.Tests;

public class FakeUserRepository : IUserRepository
{
    public readonly List<User> Users = new();

    public Task<User?> GetByIdAsync(Guid id) => Task.FromResult(Users.FirstOrDefault(u => u.Id == id));
    public Task<User?> GetByEmailAsync(string email) => Task.FromResult(Users.FirstOrDefault(u => u.Email == email));
    public Task<User?> GetByInvitationTokenAsync(string token) => Task.FromResult(Users.FirstOrDefault(u => u.InvitationToken == token));
    public Task<IEnumerable<User>> GetBySchoolIdAsync(Guid schoolId) => Task.FromResult<IEnumerable<User>>(Users.Where(u => u.SchoolId == schoolId));
    public Task<IEnumerable<User>> GetAllAsync() => Task.FromResult<IEnumerable<User>>(Users);
    public Task AddAsync(User user) { Users.Add(user); return Task.CompletedTask; }
    public Task UpdateAsync(User user)
    {
        var existing = Users.FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            Users.Remove(existing);
            Users.Add(user);
        }
        return Task.CompletedTask;
    }
    public Task DeleteAsync(User user) { Users.Remove(user); return Task.CompletedTask; }
}

public class RolePermissionsSecurityTests
{
    [Fact]
    public void SchoolOwner_ShouldHaveFullPermissionsMatrix()
    {
        // Act
        var permissions = LoginQueryHandler.GetPermissionsForRole(UserRole.SchoolOwner.ToString());

        // Assert
        Assert.Contains("School.Manage", permissions);
        Assert.Contains("Users.Manage", permissions);
        Assert.Contains("Vehicles.Manage", permissions);
        Assert.Contains("Students.Create", permissions);
        Assert.Contains("Students.Edit", permissions);
        Assert.Contains("Students.View", permissions);
        Assert.Contains("Lessons.Create", permissions);
        Assert.Contains("Payments.Create", permissions);
        Assert.Contains("Payments.View", permissions);
        Assert.Contains("Reports.Full", permissions);
        Assert.Contains("Vehicles.ViewGps", permissions);
    }

    [Fact]
    public void InstructorRole_MustNotHaveManagementPermissions()
    {
        // Act
        var permissions = LoginQueryHandler.GetPermissionsForRole(UserRole.Instructor.ToString());

        // Assert - Allowed for Instructor
        Assert.Contains("Lessons.Execute", permissions);
        Assert.Contains("Vehicles.ViewGps", permissions);
        Assert.Contains("Reports.Personal", permissions);

        // Assert - Strictly FORBIDDEN for Instructor
        Assert.DoesNotContain("Vehicles.Manage", permissions);
        Assert.DoesNotContain("School.Manage", permissions);
        Assert.DoesNotContain("Users.Manage", permissions);
        Assert.DoesNotContain("Students.Create", permissions);
        Assert.DoesNotContain("Payments.Create", permissions);
    }

    [Fact]
    public void ReceptionistRole_MustNotHaveVehicleOrUserManagementPermissions()
    {
        // Act
        var permissions = LoginQueryHandler.GetPermissionsForRole(UserRole.Receptionist.ToString());

        // Assert - Allowed for Receptionist
        Assert.Contains("Students.Create", permissions);
        Assert.Contains("Students.Edit", permissions);
        Assert.Contains("Students.View", permissions);
        Assert.Contains("Lessons.Create", permissions);

        // Assert - FORBIDDEN for Receptionist
        Assert.DoesNotContain("Vehicles.Manage", permissions);
        Assert.DoesNotContain("Users.Manage", permissions);
        Assert.DoesNotContain("School.Manage", permissions);
        Assert.DoesNotContain("Payments.Create", permissions);
    }

    [Fact]
    public void FinancialRole_MustOnlyHaveFinancialAndReportingPermissions()
    {
        // Act
        var permissions = LoginQueryHandler.GetPermissionsForRole(UserRole.Financial.ToString());

        // Assert - Allowed for Financial
        Assert.Contains("Payments.Create", permissions);
        Assert.Contains("Payments.View", permissions);
        Assert.Contains("Reports.Financial", permissions);

        // Assert - FORBIDDEN for Financial
        Assert.DoesNotContain("Vehicles.Manage", permissions);
        Assert.DoesNotContain("Users.Manage", permissions);
        Assert.DoesNotContain("Students.Create", permissions);
        Assert.DoesNotContain("Lessons.Create", permissions);
    }

    [Fact]
    public async Task DeactivatingSchoolOwner_MustBeBlockedBySafetyGuardrail()
    {
        // Arrange
        var userRepo = new FakeUserRepository();
        var owner = new User
        {
            Id = Guid.NewGuid(),
            Name = "Proprietario Principal",
            Email = "dono@escola.ao",
            Role = UserRole.SchoolOwner,
            IsActive = true
        };
        await userRepo.AddAsync(owner);

        var handler = new ToggleUserStatusCommandHandler(userRepo);
        var command = new ToggleUserStatusCommand(owner.Id);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("Proprietário", ex.Message);
        
        // Confirm owner is still active
        var recheckedOwner = await userRepo.GetByIdAsync(owner.Id);
        Assert.NotNull(recheckedOwner);
        Assert.True(recheckedOwner.IsActive);
    }

    [Fact]
    public async Task DeactivatingRegularInstructor_ShouldSucceed()
    {
        // Arrange
        var userRepo = new FakeUserRepository();
        var instructor = new User
        {
            Id = Guid.NewGuid(),
            Name = "Instrutor Teste",
            Email = "instrutor@escola.ao",
            Role = UserRole.Instructor,
            IsActive = true
        };
        await userRepo.AddAsync(instructor);

        var handler = new ToggleUserStatusCommandHandler(userRepo);
        var command = new ToggleUserStatusCommand(instructor.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        var updatedInstructor = await userRepo.GetByIdAsync(instructor.Id);
        Assert.NotNull(updatedInstructor);
        Assert.False(updatedInstructor.IsActive);
    }
}
