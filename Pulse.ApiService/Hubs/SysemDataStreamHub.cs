//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using Pulse.Models.PulseContext;

//namespace Pulse.ApiService.Hubs;

//public class SystemDataStreamHub : Hub<ISystemDataStreamClient>
//{
//    private readonly ILogger<SystemDataStreamHub> _logger;
//    private readonly IDbContextFactory<PulseDbContext> _dbFactory;
//    public SystemDataStreamHub(ILogger<SystemDataStreamHub> logger, IDbContextFactory<PulseDbContext> dbFactory)
//    {
//        _logger = logger;
//        _dbFactory = dbFactory;
//    }

//    public override async Task OnConnectedAsync()
//    {
//        //await GroupManager.JoinStandardGroups(Context, Groups, _logger);
//        await GroupManager.ConnectToUserSystemStreams(Context, _dbFactory, _logger);
//        await base.OnConnectedAsync();
//    }

//    public override Task OnDisconnectedAsync(Exception? exception)
//    {
//        _logger.LogInformation("SystemDataStream client disconnected: {ConnectionId}", Context.ConnectionId);
//        return base.OnDisconnectedAsync(exception);
//    }

//    // Clients can subscribe to specific production lines / machines
//    public async Task SubscribeToSystemStream(int streamId,string divisionId)
//    {
//        await Groups.AddToGroupAsync(Context.ConnectionId, $"{streamId}_{divisionId}");
//    }
        

//    public async Task UnsubscribeFromSystemStream(int streamId,string divisionId)
//        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{streamId}_{divisionId}");
//}

//public interface ISystemDataStreamClient : IDisposable
//{
//    Task ReceiveProductionUpdate(object update);      // e.g. machine status, counters
//    Task ReceiveInventoryChange(object change);
//    Task ReceiveSensorData(object data);
//    Task ReceiveChartUpdate(string chartId, object data);
//}