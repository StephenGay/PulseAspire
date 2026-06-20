//using Microsoft.AspNetCore.SignalR;
//using Pulse.Models.Communication;
//using Pulse.Models.Communication;

//namespace Pulse.ApiService.Hubs;

//public class NotificationHub : Hub<INotificationClient>
//{
//    private readonly ILogger<NotificationHub> _logger;

//    public NotificationHub(ILogger<NotificationHub> logger)
//    {
//        _logger = logger;
//    }

//    public override async Task OnConnectedAsync()
//    {
//        await GroupManager.JoinStandardGroups(Context, Groups, _logger);
//        await base.OnConnectedAsync();
//    }

//    public override Task OnDisconnectedAsync(Exception? exception)
//    {
//        _logger.LogInformation("Notification client disconnected: {ConnectionId}", Context.ConnectionId);
//        return base.OnDisconnectedAsync(exception);
//    }
//}

//public interface INotificationClient
//{
//    Task ReceiveNotification(SystemNotification notification);
//}