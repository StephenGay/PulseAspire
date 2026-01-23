//// Services/NotificationHubService.cs
//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Http.Connections;
//using Microsoft.AspNetCore.SignalR.Client;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Pulse.Models.CustomComponents;
//using Pulse.Web.Services;
//using System;
//using System.Threading.Tasks;

//public class MessageHubService : IAsyncDisposable
//{
//    private readonly HubConnection _connection;
//    private readonly IConfiguration _configuration;
//    private readonly ICircuitState _circuitState;
//    private readonly ILogger<MessageHubService> _logger;
   

//    public MessageHubService(
        
//        IConfiguration configuration,
//        ICircuitState circuitState,
//        ILogger<MessageHubService> logger)
//    {
//        _configuration = configuration;
//        _circuitState = circuitState;
        
//        _logger = logger;

//        var apiBaseUriString = _configuration["ServiceUri:PulseApi"] ?? "http://localhost:7466";
//        var apiBaseUri = new Uri(apiBaseUriString);
//        var hubUrl = new Uri(apiBaseUri, "/messagehub").ToString();

//        _connection = new HubConnectionBuilder()
//            .WithUrl(hubUrl, options =>
//            {
//                options.SkipNegotiation = true;
//                options.Transports = HttpTransportType.WebSockets;
//            })
//            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
//            .ConfigureLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Debug))
//            .Build();

//        _connection.On<string, string>("ReceiveMessage", (user, message) =>
//        {
//            try
//            {
//                MessageReceived?.Invoke(this, new MessageEventArgs(user, message));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in ReceiveMessage handler");
//            }
//        });

//        _connection.On<string>("Connected", (connectionId) =>
//        {
//            try
//            {
//                ConnectionStatusChanged?.Invoke(this, new ConnectionEventArgs(true, connectionId));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in Connected handler");
//            }
//        });

//        _connection.Closed += async (ex) =>
//        {
//            _logger.LogWarning(ex, "SignalR connection closed. Letting automatic reconnect attempt to recover.");
//            await Task.CompletedTask;
//        };
//    }

//    public event EventHandler<MessageEventArgs>? MessageReceived;
//    public event EventHandler<ConnectionEventArgs>? ConnectionStatusChanged;

//    public HubConnection Connection => _connection;

//    public async Task StartAsync()
//    {
//        if (_connection.State == HubConnectionState.Disconnected)
//        {
//            try
//            {
//                _logger.LogDebug("Starting SignalR on circuit: {CircuitId}", _circuitState.CurrentCircuitId);
//                await _connection.StartAsync();
//                _logger.LogInformation("SignalR connected on circuit: {CircuitId}", _circuitState.CurrentCircuitId);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "SignalR start failed on circuit: {CircuitId}", _circuitState.CurrentCircuitId);
//                throw;
//            }
//        }
//    }

//    public async Task SendMessageAsync(string user, string message)
//    {
//        await _connection.InvokeAsync("SendMessage", user, message);
//    }

//    public async ValueTask DisposeAsync()
//    {
//        if (_connection is not null)
//        {
//            try
//            {
//                if (_connection.State == HubConnectionState.Connected || _connection.State == HubConnectionState.Reconnecting)
//                {
//                    await _connection.StopAsync();
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogWarning(ex, "Error stopping SignalR connection during dispose");
//            }
//            await _connection.DisposeAsync();
//        }
//    }
//}

//public record MessageEventArgs(string User, string Message);
//public record ConnectionEventArgs(bool IsConnected, string? ConnectionId);

