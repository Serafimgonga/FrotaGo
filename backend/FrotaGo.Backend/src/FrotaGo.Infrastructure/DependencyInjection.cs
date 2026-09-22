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
        var rawConnection = configuration.GetConnectionString("DefaultConnection");
        var connectionString = NormalizePostgresConnectionString(rawConnection);

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, HttpContextTenantProvider>();

        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IInstructorRepository, InstructorRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
        services.AddScoped<IFuelRecordRepository, FuelRecordRepository>();
        services.AddScoped<IVehicleDocumentRepository, VehicleDocumentRepository>();
        services.AddScoped<IAccidentRepository, AccidentRepository>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddHostedService<FrotaGo.Infrastructure.BackgroundServices.TrackingHeartbeatWorker>();

        return services;
    }

    private static string NormalizePostgresConnectionString(string? rawConnection)
    {
        if (string.IsNullOrWhiteSpace(rawConnection))
            return string.Empty;

        var trimmed = rawConnection.Trim().Trim('"', '\'');

        if (trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var uri = new Uri(trimmed);
                var userInfo = uri.UserInfo.Split(':', 2);
                var username = Uri.UnescapeDataString(userInfo[0]);
                var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
                var database = uri.AbsolutePath.TrimStart('/');
                if (string.IsNullOrEmpty(database)) database = "postgres";
                var port = uri.Port > 0 ? uri.Port : 5432;

                var builder = new Npgsql.NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = port,
                    Database = database,
                    Username = username,
                    Password = password,
                    SslMode = Npgsql.SslMode.Require,
                    TrustServerCertificate = true
                };

                return builder.ConnectionString;
            }
            catch
            {
                return trimmed;
            }
        }

        return trimmed;
    }
}
