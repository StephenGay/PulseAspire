using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pulse.Models.Communication;
using Pulse.Models.CustomComponents;
using Pulse.Models.PulseContext;
using Pulse.Models.Users;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using static Pulse.Models.AI.Tables.TablesChatStructures;
using static Pulse.Models.Api.ApiEndpoints;

namespace Pulse.ApiService.Hubs;

//[Authorize]
public class MessageHub : Hub
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly PresenceService _presenceService;
    private readonly IDbContextFactory<PulseDbContext> _dbFactory;
    private readonly ILogger<MessageHub> _logger;

    public MessageHub(UserManager<ApplicationUser> userManager,
                       PresenceService presenceService,
                       IDbContextFactory<PulseDbContext> dbFactory,
                        ILogger<MessageHub> logger)
    {
        _userManager = userManager;
        _presenceService = presenceService;
        _dbFactory = dbFactory;
        _logger = logger;
    }

    // Clients will call this to send a message
    public async Task SendMessage(PulseMessage msg)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        db.PulseMessages.Add(msg);
        await db.SaveChangesAsync();
        // Broadcast to ALL connected clients
        await Clients.All.SendAsync("ReceiveMessage", msg);
        _logger.LogDebug("Broadcast message from {User}", msg.SenderUserName);
    }

    // Send private message to a specific recipient
    public async Task SendPrivateMessage(PulseMessage msg)
    {
        if (string.IsNullOrEmpty(msg.RecipientUserName))
        {
            var user = await _userManager.FindByIdAsync(msg.RecipientUserId);
            msg.RecipientUserName = user?.UserName ?? "Unknown Recipient";
        }
        using var db = await _dbFactory.CreateDbContextAsync();
        db.PulseMessages.Add(msg);
        await db.SaveChangesAsync();

        // Get the recipient's connection IDs
        var recipientConnectionIds = _presenceService.GetUserConnectionIds(msg.RecipientUserId);

        if (recipientConnectionIds.Any())
        {
            // Send to the specific recipient via their connection IDs
            await Clients.Clients(recipientConnectionIds.ToList()).SendAsync("ReceiveMessage", msg);
            _logger.LogDebug("Private message from {User} sent to {Recipient} ({Count} connections)", 
                msg.SenderUserName, msg.RecipientUserId, recipientConnectionIds.Count());
        }
        else
        {
            _logger.LogWarning("Recipient {UserId} not online. Message saved but not delivered.", msg.RecipientUserId);
        }

        // Also notify the sender that message was sent
        //await Clients.Caller.SendAsync("ReceiveMessage", msg);
    }

    // Optional: Send to a specific group
    public async Task JoinGroup(string groupName, string uFullName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{uFullName} has joined the group : {groupName}");
    }

    public async Task LeaveGroup(string groupName, string uFullName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{uFullName} has left the group : {groupName}");
    }

    public async Task SendToGroup(string groupName, string user, string message)
    {
        await Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
    }

    public async Task TablesProgressUpdate(TablesProgressUpdate update)
    {
        // Broadcast progress update to all clients (or you could target specific users/groups)
        await Clients.Group(update.SessionId.ToString()).SendAsync("TablesProgressUpdate", update);
        _logger.LogDebug("Broadcasting Tables progress update: {SessionId} - {Status}", update.SessionId, update.Status);
    }

    public async Task SetPresence(int code)
    {
        if (!Enum.IsDefined(typeof(PresenceStatus), code))
        {
            _logger.LogWarning("Invalid presence status code: {Code}", code);
            return;
        }

        var status = (PresenceStatus)code;
        var user = await _userManager.GetUserAsync(Context.User);

        if (user == null)
        {
            _logger.LogWarning("User not found for connection {ConnectionId}", Context.ConnectionId);
            return;
        }

        // Prevent setting to Offline while still connected
        if (status == PresenceStatus.Offline && _presenceService.GetUserConnectionCount(user.Id) > 0)
        {
            _logger.LogWarning("User {UserId} attempted to set offline while connected", user.Id);
            return;
        }

        // Update database
        user.PresenceStatus = status;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            _logger.LogError("Failed to update presence for {UserId}: {Errors}",
                user.Id, string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        _logger.LogInformation("Presence updated: {UserId} -> {Status}", user.Id, status);
        await BroadcastPresenceAsync();
    }

    private async Task BroadcastPresenceAsync()
    {
        var onlineUserIds = _presenceService.GetOnlineUsers()
            .Select(u => u.UserId)
            .ToHashSet();

        using var db = await _dbFactory.CreateDbContextAsync();
        var allUsers = await db.Users
            .Select(u => new { u.Id, u.Email, u.UserName, u.FullName, u.PresenceStatus })
            .ToListAsync();

        var presenceList = allUsers
            .Select(u => new UserPresenceDto(
                UserId: u.Id,
                UserName: u.Email ?? u.UserName ?? "Anonymous",
                FullName: u.FullName ?? u.Email ?? u.UserName ?? "Anonymous",
                IsOnline: onlineUserIds.Contains(u.Id),
                EffectiveStatus: onlineUserIds.Contains(u.Id) 
                    ? (Microsoft.FluentUI.AspNetCore.Components.PresenceStatus)u.PresenceStatus
                    : Microsoft.FluentUI.AspNetCore.Components.PresenceStatus.Offline
            ))
            .OrderBy(dto => dto.FullName)
            .ToList();

        _logger.LogDebug("Broadcasting {Count} users to all clients", presenceList.Count);
        await Clients.All.SendAsync("PresenceListUpdated", presenceList);
    }

    public async Task<List<UserPresenceDto>> GetPresenceListAsync()
    {
        var onlineUserIds = _presenceService.GetOnlineUsers()
            .Select(u => u.UserId)
            .ToHashSet();

        using var db = await _dbFactory.CreateDbContextAsync();
        var allUsers = await db.Users
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.UserName,
                u.FullName,
                u.PresenceStatus
            })
            .ToListAsync();

        var presenceList = allUsers.Select(u => new UserPresenceDto(
            UserId: u.Id,
            UserName: u.Email ?? u.UserName ?? "Anonymous",
            FullName: u.FullName ?? u.Email ?? u.UserName ?? "Anonymous",
            IsOnline: onlineUserIds.Contains(u.Id),
            EffectiveStatus: (Microsoft.FluentUI.AspNetCore.Components.PresenceStatus)(onlineUserIds.Contains(u.Id) ? u.PresenceStatus : PresenceStatus.Offline)
        ))
        .OrderBy(dto => dto.FullName)
        .ToList();

        return presenceList;
    }
    public override async Task OnConnectedAsync()
    {
        try
        {
            var userClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)
                 ?? Context.User?.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userClaim == null || string.IsNullOrEmpty(userClaim.Value))
            {
                // Log and abort connection if no valid user ID claim
                // Optional: _logger.LogWarning("SignalR connection attempted without valid user ID claim");
                //Context.Abort();
                return;
            }

            var userId = userClaim.Value;

            var user = await _userManager.FindByIdAsync(userId);
            var userName = user?.Email ?? user?.UserName ?? "Anonymous";

            _presenceService.AddConnection(userId, Context.ConnectionId, userName);

            // AUTO-SET to Available on connect (if not explicitly set by client)
            if (user != null && user.PresenceStatus == PresenceStatus.Offline)
            {
                user.PresenceStatus = PresenceStatus.Available;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed to auto-set presence to Available for {UserId}", userId);
                }
            }

            // Broadcast updated presence
            await BroadcastPresenceAsync();

            // Load private history for caller
            using var db = await _dbFactory.CreateDbContextAsync();
            var history = await db.PulseMessages
                .Where(m => m.RecipientUserId == userId)
                .OrderBy(m => m.SentAt)
                //.Take(100)
                .ToListAsync();

            await Clients.Caller.SendAsync("LoadHistory", history);

            await base.OnConnectedAsync();
            //var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            ////if (!string.IsNullOrEmpty(userId))
            ////{
            //    var user = await _userManager.GetUserAsync(Context.User);
            //    var userName = user?.Email ?? user?.UserName ?? "Anonymous";

            //    _presenceService.AddConnection(userId, Context.ConnectionId, userName);

            //    // Broadcast updated presence to ALL clients
            //    var onlineUsers = _presenceService.GetOnlineUsers();
            //    await Clients.All.SendAsync("PresenceUpdated", onlineUsers);
            //    await Clients.Caller.SendAsync("Connected", $"Welcome! Your ConnectionId: {Context.ConnectionId}");
            //    _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);
            //}
            //else
            //{
            //    // If anonymous (shouldn't happen for authorized hub), still notify caller.
            //    await Clients.Caller.SendAsync("Connected", $"Welcome! Your ConnectionId: {Context.ConnectionId}");
            //    _logger.LogInformation("Anonymous connection with ConnectionId {ConnectionId}", Context.ConnectionId);
            //}
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OnConnectedAsync");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = _presenceService.GetUserIdFromConnection(Context.ConnectionId);
            _presenceService.RemoveConnection(Context.ConnectionId);

            // If user has no more connections, mark as Offline in database
            if (userId != null && !_presenceService.IsUserOnline(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    user.PresenceStatus = PresenceStatus.Offline;
                    await _userManager.UpdateAsync(user);
                    _logger.LogInformation("User {UserId} marked offline (no connections)", userId);
                }
            }

            await BroadcastPresenceAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OnDisconnectedAsync");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task MarkMessageAsDeliveredAsync(int messageId)
    {
        // Update in your database
        using var db = await _dbFactory.CreateDbContextAsync();
        var message = await db.PulseMessages
            .FirstOrDefaultAsync(m => m.Id == messageId);
            //.FindAsync(messageId);
        if (message != null)
        {
            message.Delivered = true;
            await db.SaveChangesAsync();
        }
    }
}
