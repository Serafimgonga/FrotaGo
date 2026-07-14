using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace FrotaGo.Api.Hubs;

public class GpsHub : Hub
{
    public async Task SendLocation(Guid vehicleId, double latitude, double longitude, double speed)
    {
        await Clients.All.SendAsync("LocationUpdated", vehicleId, latitude, longitude, speed);
    }
}
