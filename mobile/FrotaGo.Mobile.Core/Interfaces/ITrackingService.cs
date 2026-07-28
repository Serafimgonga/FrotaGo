using FrotaGo.Mobile.Core.DTOs;
using FrotaGo.Mobile.Core.Models;

namespace FrotaGo.Mobile.Core.Interfaces;

public interface ITrackingService
{
    Task<TrackingSession?> StartTrackingSessionAsync(Guid instructorId, Guid vehicleId, Guid? lessonId);
    Task<bool> SendTelemetryAsync(GpsLocationDto location);
    Task EndTrackingSessionAsync(Guid sessionId);
    TrackingSession? ActiveSession { get; }
}
