using Microsoft.AspNetCore.SignalR;

namespace VidaPositiva.Api.Hubs;

public class BrokerHub : Hub
{
    public async Task Subscribe(string connectionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, connectionId);
    }

    public async Task Unsubscribe(string connectionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, connectionId);
    }
}