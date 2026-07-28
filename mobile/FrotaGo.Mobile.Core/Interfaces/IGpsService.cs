using FrotaGo.Mobile.Core.DTOs;

namespace FrotaGo.Mobile.Core.Interfaces;

public interface IGpsService
{
    Task<GpsLocationDto?> GetCurrentLocationAsync();
    Task StartBackgroundTrackingAsync(Func<GpsLocationDto, Task> onLocationUpdated);
    Task StopBackgroundTrackingAsync();
    bool IsTracking { get; }
}
