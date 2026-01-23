using System.Collections.Concurrent;

public record OnlineUser(string UserId, string UserName);

public class PresenceService
{
    private readonly ConcurrentDictionary<string, string> _connectionToUserId = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _userToConnections = new();
    private readonly ConcurrentDictionary<string, string> _userNames = new();

    public void AddConnection(string userId, string connectionId, string userName)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(connectionId))
            return;

        _userToConnections.GetOrAdd(userId, _ => new HashSet<string>()).Add(connectionId);
        _connectionToUserId.TryAdd(connectionId, userId);
        _userNames[userId] = userName ?? "Anonymous";
    }

    public string? GetUserIdFromConnection(string connectionId)
    {
        _connectionToUserId.TryGetValue(connectionId, out var userId);
        return userId;
    }

    public void RemoveConnection(string connectionId)
    {
        if (_connectionToUserId.TryRemove(connectionId, out var userId))
        {
            if (_userToConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);
                // Only remove user completely if ALL connections are gone
                if (connections.Count == 0)
                {
                    _userToConnections.TryRemove(userId, out _);
                    _userNames.TryRemove(userId, out _);
                }
            }
        }
    }

    public bool IsUserOnline(string userId)
    {
        return _userToConnections.ContainsKey(userId) &&
               _userToConnections[userId].Count > 0;
    }

    public int GetUserConnectionCount(string userId)
    {
        return _userToConnections.TryGetValue(userId, out var connections)
            ? connections.Count
            : 0;
    }

    public IEnumerable<OnlineUser> GetOnlineUsers()
    {
        return _userNames
            .Where(kv => IsUserOnline(kv.Key))
            .Select(kv => new OnlineUser(kv.Key, kv.Value));
    }
}