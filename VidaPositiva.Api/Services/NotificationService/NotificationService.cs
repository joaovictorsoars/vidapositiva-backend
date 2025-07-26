using Microsoft.AspNetCore.SignalR;
using VidaPositiva.Api.Hubs;

namespace VidaPositiva.Api.Services.NotificationService;

public class NotificationService(IHubContext<BrokerHub> hubContext) : INotificationService
{
    public async Task NotifyProgressAsync(string connectionId, object obj)
    {
        await hubContext.Clients.Group(connectionId).SendAsync("ReceiveProgress", obj);
    }
}