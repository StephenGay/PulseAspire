// Services/NotificationHubService.cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Pulse.Web.Services;
using System;

public class MessageHubService : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private readonly IConfiguration _configuration;
    private readonly ICircuitState _circuitState;  // Updated to ICircuitState
    private readonly ILogger<MessageHubService> _logger;  // Add if you want service-specific logging

    public MessageHubService(
        NavigationManager navigationManager,
        IConfiguration configuration,
        ICircuitState circuitState,
        ILogger<MessageHubService> logger)  // Inject logger for this service
    {
        _configuration = configuration;
        _circuitState = circuitState;
        _logger = logger;

        // ... existing constructor logic for hubUrl and _connection ...
        var apiBaseUriString = _configuration["ServiceUri:PulseApi"] ?? "http://localhost:7466";
        var apiBaseUri = new Uri(apiBaseUriString);
        var hubUrl = new Uri(apiBaseUri, "/messagehub").ToString();

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.SkipNegotiation = true;  // Bypass for proxies like YARP
                options.Transports = HttpTransportType.WebSockets;
            })
            .WithAutomaticReconnect()
            .ConfigureLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Debug))  // For detailed client logs
            .Build();

        // Register server-to-client methods
        _connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            MessageReceived?.Invoke(this, new MessageEventArgs(user, message));
        });

        _connection.On<string>("Connected", (connectionId) =>
        {
            ConnectionStatusChanged?.Invoke(this, new ConnectionEventArgs(true, connectionId));
        });
    }

    public event EventHandler<MessageEventArgs>? MessageReceived;
    public event EventHandler<ConnectionEventArgs>? ConnectionStatusChanged;

    public HubConnection Connection => _connection;

    public async Task StartAsync()
    {
        if (_connection.State == HubConnectionState.Disconnected)
        {
            try
            {
                _logger.LogDebug("Starting SignalR on circuit: {CircuitId}", _circuitState.CurrentCircuitId);
                await _connection.StartAsync();
                _logger.LogInformation("SignalR connected on circuit: {CircuitId}", _circuitState.CurrentCircuitId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SignalR start failed on circuit: {CircuitId}", _circuitState.CurrentCircuitId);
                throw;
            }
        }
    }

    public async Task SendMessageAsync(string user, string message)
    {
        await _connection.InvokeAsync("SendMessage", user, message);
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}

public record MessageEventArgs(string User, string Message);
public record ConnectionEventArgs(bool IsConnected, string? ConnectionId);

