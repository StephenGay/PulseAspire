// Services/NotificationHubService.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pulse.Models.Communication;
using Pulse.Models.CustomComponents;
using Pulse.Web.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

public class MessageHubService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;
    private readonly ILogger<MessageHubService> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed = false;

    // Events for components to subscribe
    public event Action<PulseMessage>? OnReceiveMessage;
    public event Action<PulseMessage>? OnReceiveFlapperChunk;
    public event Action<List<PulseMessage>>? OnLoadHistory;
    public event Action<List<UserPresenceDto>>? OnPresenceListUpdated;

    public MessageHubService(
        IConfiguration configuration,
        AuthService authService,
        ILogger<MessageHubService> logger)
    {
        _authService = authService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnsureConnectedAsync()
    {
        if (_disposed)
        {
            _logger?.LogWarning("Cannot connect: MessageHubService has been disposed.");
            return;
        }

        if (!_authService.IsAuthenticated || string.IsNullOrEmpty(_authService.JwtToken))
        {
            _logger?.LogWarning("Skipping SignalR connection: User not authenticated or JWT missing.");
            await DisposeConnectionAsync();
            return;
        }

        await _connectionLock.WaitAsync();
        try
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                return;
            }

            if (_hubConnection == null)
            {
                var apiBaseUriString = _configuration["PulseApi:Endpoint"]
                    ?? throw new InvalidOperationException("Missing configuration key 'PulseApi:Endpoint'");
                
                var hubUri = new Uri(new Uri(apiBaseUriString), "/messagehub");
                
                // CRITICAL: Pass token as query parameter for WebSocket connections
                var tokenizedHubUrl = $"{hubUri}?access_token={Uri.EscapeDataString(_authService.JwtToken)}";

                _logger?.LogInformation("Initializing SignalR connection to {HubUrl}", hubUri);

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(tokenizedHubUrl, options =>
                    {
                        // Still set AccessTokenProvider for any HTTP-based transports (fallback)
                        options.AccessTokenProvider = async () => _authService.JwtToken ?? string.Empty;
                        options.SkipNegotiation = false; // Allow fallback to HTTP long-polling if WebSocket fails
                        options.Transports = HttpTransportType.WebSockets | HttpTransportType.LongPolling;
                    })
                    .WithAutomaticReconnect(new[] 
                    { 
                        TimeSpan.Zero, 
                        TimeSpan.FromSeconds(2), 
                        TimeSpan.FromSeconds(10), 
                        TimeSpan.FromSeconds(30) 
                    })
                    .ConfigureLogging(logging => logging.AddConsole().SetMinimumLevel(LogLevel.Debug))
                    .Build();

                // Register message handlers
                _hubConnection.On<PulseMessage>("ReceiveMessage", msg =>
                {
                    try
                    {
                        OnReceiveMessage?.Invoke(msg);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error handling ReceiveMessage");
                    }
                });

                _hubConnection.On<PulseMessage>("ReceiveFlapperChunk", msg =>
                {
                    try
                    {
                        OnReceiveFlapperChunk?.Invoke(msg);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error handling ReceiveFlapperChunk");
                    }
                });

                _hubConnection.On<List<PulseMessage>>("LoadHistory", history =>
                {
                    try
                    {
                        OnLoadHistory?.Invoke(history);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error handling LoadHistory");
                    }
                });

                _hubConnection.On<List<UserPresenceDto>>("PresenceListUpdated", list =>
                {
                    try
                    {
                        OnPresenceListUpdated?.Invoke(list);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error handling PresenceListUpdated");
                    }
                });

                // Handle disconnection without infinite loop
                _hubConnection.Closed += OnConnectionClosed;
            }

            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                try
                {
                    _logger?.LogInformation("Starting SignalR connection...");
                    await _hubConnection.StartAsync();
                    _logger?.LogInformation("SignalR connected successfully.");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to start SignalR connection");
                    throw;
                }
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task<List<UserPresenceDto>> GetPresenceListAsync()
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            _logger?.LogWarning("Cannot get presence list: hub not connected");
            return new();
        }

        try
        {
            var presenceList = await _hubConnection.InvokeAsync<List<UserPresenceDto>>("GetPresenceListAsync");
            return presenceList ?? new();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error fetching presence list");
            return new();
        }
    }
    private async Task OnConnectionClosed(Exception? ex)
    {
        _logger?.LogWarning(ex, "SignalR connection closed. Attempting automatic reconnect...");
        
        // Don't reconnect immediately; let automatic reconnect retry policy handle it
        // Only manual reconnect if user is still authenticated after a delay
        await Task.Delay(3000);
        
        if (_authService.IsAuthenticated && !_disposed)
        {
            try
            {
                await EnsureConnectedAsync();
            }
            catch (Exception reconnectEx)
            {
                _logger?.LogError(reconnectEx, "Automatic reconnection failed");
            }
        }
    }

    private async Task DisposeConnectionAsync()
    {
        if (_hubConnection != null)
        {
            try
            {
                if (_hubConnection.State == HubConnectionState.Connected || 
                    _hubConnection.State == HubConnectionState.Reconnecting)
                {
                    await _hubConnection.StopAsync();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error stopping SignalR connection");
            }
            finally
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
    }

    public async Task SendMessageAsync(PulseMessage msg)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            _logger?.LogWarning("Cannot send message: Hub not connected.");
            await EnsureConnectedAsync();
        }

        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.InvokeAsync("SendMessage", msg);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error sending message via SignalR");
                throw;
            }
        }
    }

    public async Task SendPrivateMessageAsync(PulseMessage msg)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            _logger?.LogWarning("Cannot send message: Hub not connected.");
            await EnsureConnectedAsync();
        }

        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.InvokeAsync("SendPrivateMessage", msg);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error sending message via SignalR");
                throw;
            }
        }
    }
    public async Task SetPresenceAsync(int code)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            await EnsureConnectedAsync();
        }

        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.InvokeAsync("SetPresence", code);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error setting presence via SignalR");
                throw;
            }
        }
    }

    public async Task MarkMessageAsDeliveredAsync(int messageId)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            _logger?.LogWarning("Cannot mark message as delivered: Hub not connected.");
            await EnsureConnectedAsync();
        }

        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.InvokeAsync("MarkMessageAsDeliveredAsync", messageId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error marking message as delivered via SignalR");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await DisposeConnectionAsync();
        _connectionLock?.Dispose();
    }
}

