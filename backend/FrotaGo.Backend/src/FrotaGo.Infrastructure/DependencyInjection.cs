using FrotaGo.Application.Interfaces;
using FrotaGo.Infrastructure.Authentication;
using FrotaGo.Infrastructure.Persistence;
using FrotaGo.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrotaGo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IInstructorRepository, InstructorRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        services.AddScoped<IFuelRecordRepository, FuelRecordRepository>();
        services.AddScoped<IVehicleDocumentRepository, VehicleDocumentRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddHostedService<FrotaGo.Infrastructure.BackgroundServices.TrackingHeartbeatWorker>();

        return services;
    }
}
