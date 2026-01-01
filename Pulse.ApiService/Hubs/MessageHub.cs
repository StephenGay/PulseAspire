using Microsoft.AspNetCore.SignalR;

namespace Pulse.ApiService.Hubs;

public class MessageHub : Hub
{
    // Clients will call this to send a message
    public async Task SendMessage(string user, string message)
    {
        // Broadcast to ALL connected clients
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    // Optional: Send only to the caller
    public async Task SendPrivateMessage(string message)
    {
        await Clients.Caller.SendAsync("ReceiveMessage", "You", message);
    }

    // Optional: Send to a specific group
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} joined {groupName}");
    }

    public async Task SendToGroup(string groupName, string user, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", $"Welcome! Your ConnectionId: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
