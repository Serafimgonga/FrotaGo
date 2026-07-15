using System;
using System.Threading.Tasks;

namespace FrotaGo.Application.Interfaces;

public interface ITrackingHubContext
{
    Task BroadcastLocationAsync(Guid vehicleId, double latitude, double longitude, double speed);
    Task BroadcastTrackingStoppedAsync(Guid vehicleId);
    Task BroadcastStatusChangedAsync(Guid sessionId, Guid vehicleId, string status);
}
