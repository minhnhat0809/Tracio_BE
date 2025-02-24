using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace ContentService.Application.Hubs;

public class ContentHub : Hub
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> UserGroups = new();
    
    // Join general blog updates
    public async Task JoinBlogUpdates()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "BlogUpdates");
    }
    
    // User leaves "BlogUpdates" when entering a specific blog
    public async Task LeaveBlogUpdates()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "BlogUpdates");
        Console.WriteLine($"User {Context.ConnectionId} left BlogUpdates");
    }

    // Join a specific blog group
    public async Task JoinBlog(string blogId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Blog-{blogId}");
    }
    
    // Leave groups when navigating away
    public async Task LeaveBlog(string blogId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Blog-{blogId}");
    }

    // Join a specific comment thread
    public async Task JoinComment(string commentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Comment-{commentId}");
    }

    public async Task LeaveComment(string commentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Comment-{commentId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var group in UserGroups.GetValueOrDefault(Context.ConnectionId, new HashSet<string>()))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        }
        UserGroups.Remove(Context.ConnectionId, out _);
        await base.OnDisconnectedAsync(exception);
    }
}