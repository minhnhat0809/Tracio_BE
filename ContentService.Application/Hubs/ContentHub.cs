using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace ContentService.Application.Hubs;

public class ContentHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await Clients.All.SendAsync("OnConnected");
    }

    private static readonly ConcurrentDictionary<string, HashSet<string>> UserGroups = new();
    
    // Join general blog updates
    public async Task JoinBlogUpdates()
    {
        try
        {
            Console.WriteLine($"🔵 Client {Context.ConnectionId} is joining BlogUpdates");

            await Groups.AddToGroupAsync(Context.ConnectionId, "BlogUpdates");

            Console.WriteLine($"✅ Client {Context.ConnectionId} successfully joined BlogUpdates");

            await Clients.Caller.SendAsync("JoinedBlogUpdate", "12345");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in JoinBlogUpdates: {ex.Message}");
        }
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
        Console.WriteLine($"Client {Context.ConnectionId} is joining Blog-{blogId}");
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Blog-{blogId}");
        
        await Clients.Caller.SendAsync("JoinedGroup", $"Blog-{blogId}");
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