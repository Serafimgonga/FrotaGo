using FrotaGo.Mobile.Core.DTOs;
using FrotaGo.Mobile.Core.Enums;
using FrotaGo.Mobile.Core.Helpers;
using FrotaGo.Mobile.Core.Interfaces;
using FrotaGo.Mobile.Core.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace FrotaGo.Mobile.Core.Services;

public class TrackingService : ITrackingService
{
    private readonly IApiClient _apiClient;
    private HubConnection? _hubConnection;

    public TrackingSession? ActiveSession { get; private set; }

    public TrackingService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<TrackingSession?> StartTrackingSessionAsync(Guid instructorId, Guid vehicleId, Guid? lessonId)
    {
        var request = new
        {
            InstructorId = instructorId,
            VehicleId = vehicleId,
            PracticalLessonId = lessonId
        };

        var session = await _apiClient.PostAsync<object, TrackingSession>(ApiEndpoints.Mobile.StartTracking, request);
        if (session != null)
        {
            ActiveSession = session;
            ActiveSession.Status = TrackingStatus.Active;
            await ConnectSignalRAsync();
        }

        return session;
    }

    public async Task<bool> SendTelemetryAsync(GpsLocationDto location)
    {
        if (ActiveSession == null) return false;

        location.SessionId = ActiveSession.SessionId;
        location.VehicleId = ActiveSession.VehicleId;

        if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("SendLocation", location);
            return true;
        }

        return await _apiClient.PostAsync(ApiEndpoints.Mobile.SendTelemetry, location);
    }

    public async Task EndTrackingSessionAsync(Guid sessionId)
    {
        if (ActiveSession != null && ActiveSession.SessionId == sessionId)
        {
            await _apiClient.PostAsync($"{ApiEndpoints.Mobile.EndTracking}/{sessionId}", new { });
            ActiveSession.Status = TrackingStatus.Stopped;
            ActiveSession = null;

            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
    }

    private async Task ConnectSignalRAsync()
    {
        try
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(ApiEndpoints.SignalRHubUrl)
                .WithAutomaticReconnect()
                .Build();

            await _hubConnection.StartAsync();
        }
        catch
        {
            // SignalR fallback to HTTP REST Telemetry
        }
    }
}
