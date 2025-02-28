using System.Collections.Concurrent;

namespace NotificationService.Application.SignalR;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<int, string> _connections = new();

    public void AddConnection(int userId, string connectionId)
    {
        _connections[userId] = connectionId;
    }

    public void RemoveConnection(int userId)
    {
        _connections.TryRemove(userId, out _);
    }

    public bool IsUserOnline(int userId)
    {
        return _connections.ContainsKey(userId);
    }

    public string? GetConnectionId(int userId)
    {
        return _connections.GetValueOrDefault(userId);
    }
    
    public int? GetUserIdByConnectionId(string connectionId)
    {
        return _connections.FirstOrDefault(x => x.Value == connectionId).Key;
    }
}