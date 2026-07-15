using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using FrotaGo.Domain.Entities;
using FrotaGo.Domain.Enums;
using FrotaGo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FrotaGo.Infrastructure.BackgroundServices;

public class TrackingHeartbeatWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITrackingHubContext _hubContext;
    private readonly ILogger<TrackingHeartbeatWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);
    private readonly TimeSpan _timeoutThreshold = TimeSpan.FromSeconds(30);

    public TrackingHeartbeatWorker(
        IServiceScopeFactory scopeFactory,
        ITrackingHubContext hubContext,
        ILogger<TrackingHeartbeatWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TrackingHeartbeatWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckActiveSessionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar batimentos cardíacos (heartbeat) de telemetria.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("TrackingHeartbeatWorker parado.");
    }

    private async Task CheckActiveSessionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var cutoffTime = DateTime.UtcNow - _timeoutThreshold;

        // Buscar sessões de tracking ativas
        var activeSessions = await context.TrackingSessions
            .Where(s => s.Status == TrackingStatus.Active)
            .ToListAsync(cancellationToken);

        if (!activeSessions.Any()) return;

        foreach (var session in activeSessions)
        {
            // Obter a última coordenada registada para esta sessão
            var lastLocation = await context.VehicleLocations
                .Where(vl => vl.TrackingSessionId == session.Id)
                .OrderByDescending(vl => vl.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);

            bool isTimedOut = false;

            if (lastLocation != null)
            {
                // Se o timestamp da última coordenada for anterior ao tempo limite
                if (lastLocation.Timestamp < cutoffTime)
                {
                    isTimedOut = true;
                }
            }
            else
            {
                // Sem coordenadas registadas: verificar o tempo de criação da sessão
                if (session.StartedAt < cutoffTime)
                {
                    isTimedOut = true;
                }
            }

            if (isTimedOut)
            {
                _logger.LogWarning("Sessão de tracking {SessionId} (Viatura: {VehicleId}) perdeu sinal.", session.Id, session.VehicleId);
                
                session.Status = TrackingStatus.LostConnection;
                context.TrackingSessions.Update(session);

                // Broadcast via SignalR da alteração de estado para "LostConnection"
                await _hubContext.BroadcastStatusChangedAsync(session.Id, session.VehicleId, "LostConnection");
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
