using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FrotaGo.Mobile.Core.DTOs;
using FrotaGo.Mobile.Core.Enums;
using FrotaGo.Mobile.Core.Interfaces;

namespace FrotaGo.Mobile.Core.ViewModels;

public partial class TrackingViewModel : BaseViewModel
{
    private readonly ITrackingService _trackingService;
    private readonly IGpsService _gpsService;

    [ObservableProperty]
    private double _currentLatitude;

    [ObservableProperty]
    private double _currentLongitude;

    [ObservableProperty]
    private double _currentSpeedKmh;

    [ObservableProperty]
    private TrackingStatus _status = TrackingStatus.Idle;

    public TrackingViewModel(ITrackingService trackingService, IGpsService gpsService)
    {
        _trackingService = trackingService;
        _gpsService = gpsService;
        Title = "Rastreio GPS em Tempo Real";
    }

    [RelayCommand]
    public async Task StartTrackingAsync()
    {
        if (_gpsService.IsTracking) return;

        Status = TrackingStatus.Active;
        await _gpsService.StartBackgroundTrackingAsync(OnLocationUpdateReceivedAsync);
    }

    [RelayCommand]
    public async Task StopTrackingAsync()
    {
        await _gpsService.StopBackgroundTrackingAsync();
        Status = TrackingStatus.Stopped;
    }

    private async Task OnLocationUpdateReceivedAsync(GpsLocationDto location)
    {
        CurrentLatitude = location.Latitude;
        CurrentLongitude = location.Longitude;
        CurrentSpeedKmh = location.SpeedKmh;

        if (_trackingService.ActiveSession != null)
        {
            await _trackingService.SendTelemetryAsync(location);
        }
    }
}
