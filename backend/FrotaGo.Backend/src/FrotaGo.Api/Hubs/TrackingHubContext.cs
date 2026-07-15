using System;
using System.Threading.Tasks;
using FrotaGo.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FrotaGo.Api.Hubs;

public class TrackingHubContext : ITrackingHubContext
{
    private readonly IHubContext<GpsHub> _hubContext;

    public TrackingHubContext(IHubContext<GpsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task BroadcastLocationAsync(Guid vehicleId, double latitude, double longitude, double speed)
    {
        await _hubContext.Clients.All.SendAsync("LocationUpdated", vehicleId, latitude, longitude, speed);
    }

    public async Task BroadcastTrackingStoppedAsync(Guid vehicleId)
    {
        await _hubContext.Clients.All.SendAsync("TrackingStopped", vehicleId);
    }

    public async Task BroadcastStatusChangedAsync(Guid sessionId, Guid vehicleId, string status)
    {
        await _hubContext.Clients.All.SendAsync("TrackingStatusChanged", sessionId, vehicleId, status);
    }
}
