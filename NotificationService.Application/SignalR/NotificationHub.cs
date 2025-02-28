using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Application.SignalR;

public class NotificationHub : Hub
{
    private static readonly ConcurrentDictionary<string, string> OnlineUsers = new ConcurrentDictionary<string, string>();
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            OnlineUsers[userId] = Context.ConnectionId;
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            OnlineUsers.TryRemove(userId, out _);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public static bool IsUserOnline(string userId)
    {
        return OnlineUsers.ContainsKey(userId);
    }
}