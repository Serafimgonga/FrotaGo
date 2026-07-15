using System.Text.Json;
using FrotaGo.Mobile.Models;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Networking;
using Microsoft.Maui.ApplicationModel.Permissions;
using Microsoft.Maui.Storage;
using System.Security.Cryptography;
using System.Text;

namespace FrotaGo.Mobile.Services;

public class TrackingService
{
    private readonly ApiService _api;
    private CancellationTokenSource? _cts;
    private TrackingSession? _session;
    private readonly List<LocationPoint> _cache = new();
    private readonly EncryptionService _encryption;
    private readonly string _cacheFile;

    public TrackingService(ApiService api)
    {
        _api = api;
        _encryption = new EncryptionService();
        _cacheFile = Path.Combine(FileSystem.AppDataDirectory, "tracking_cache.enc");
        _ = LoadCacheAsync();
    }

    private async Task LoadCacheAsync()
    {
        try
        {
            if (!File.Exists(_cacheFile)) return;
            var data = await File.ReadAllBytesAsync(_cacheFile);
            var jsonBytes = await _encryption.DecryptAsync(data);
            var json = Encoding.UTF8.GetString(jsonBytes);
            var list = JsonSerializer.Deserialize<List<LocationPoint>>(json);
            if (list != null) _cache.AddRange(list);
        }
        catch { }
    }

    private async Task SaveCacheAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_cache);
            var bytes = Encoding.UTF8.GetBytes(json);
            var enc = await _encryption.EncryptAsync(bytes);
            await File.WriteAllBytesAsync(_cacheFile, enc);
        }
        catch { }
    }

    public async Task<bool> StartAsync(int lessonId, int vehicleId, int instructorId)
    {
        // request location permission
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted) return false;

        // attempt to start tracking session on backend
        _session = await _api.StartTrackingSessionAsync(lessonId, vehicleId, instructorId);
        if (_session == null) return false;

        _session.Status = TrackingStatus.Active;

        _cts = new CancellationTokenSource();
        _ = Task.Run(() => LoopSendAsync(_cts.Token));
        return true;
    }

    public async Task StopAsync()
    {
        try
        {
            // attempt to notify backend
            if (_session != null && Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                await _api.StopTrackingAsync(_session.Id);
            }
            _cts?.Cancel();
            _session = null;
            // persist cache
            await SaveCacheAsync();
        }
        catch { }
    }

    public string? CurrentSessionId => _session?.Id;

    private async Task LoopSendAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _session != null)
        {
            try
            {
                // check connectivity
                bool online = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

                // try get location
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request, ct);
                if (location != null)
                {
                    if (online)
                    {
                        var ok = await _api.SendLocationAsync(_session.Id, location.Latitude, location.Longitude, location.Speed);
                        if (!ok)
                        {
                            _cache.Add(new LocationPoint(_session.Id, location.Latitude, location.Longitude, location.Speed));
                            await SaveCacheAsync();
                        }
                    }
                    else
                    {
                        _cache.Add(new LocationPoint(_session.Id, location.Latitude, location.Longitude, location.Speed));
                        await SaveCacheAsync();
                    }
                }

                // try to flush cache if online
                if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet && _cache.Count > 0)
                {
                    var toSend = _cache.ToList();
                    foreach (var p in toSend)
                    {
                        var ok = await _api.SendLocationAsync(p.TrackingSessionId, p.Latitude, p.Longitude, p.Speed);
                        if (ok) _cache.Remove(p);
                    }
                    await SaveCacheAsync();
                }
            }
            catch { }

            try { await Task.Delay(TimeSpan.FromSeconds(5), ct); } catch { }
        }
    }

    private record LocationPoint(string TrackingSessionId, double Latitude, double Longitude, double? Speed);
}
