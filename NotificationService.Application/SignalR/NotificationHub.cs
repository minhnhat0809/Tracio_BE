using System.Collections.Concurrent;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Application.SignalR;

public class NotificationHub(IHttpContextAccessor httpContextAccessor) : Hub
{
    private static readonly ConcurrentDictionary<int, string> Connections = new();
    
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    // Initialize Firebase App
    static NotificationHub()
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile("firebase-service-account.json") // Path to Firebase credentials
        });
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var token = httpContext?.Request.Query["token"].ToString();

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine($"❌ Connection rejected: Missing token.");
            Context.Abort();
            return;
        }

        try
        {
            // Verify Firebase Token
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

            // Extract custom_id from claims
            var customIdClaim = decodedToken.Claims.FirstOrDefault(c => c.Key == "custom_id").Value;
            
            if (customIdClaim == null || !int.TryParse(customIdClaim.ToString(), out int userId))
            {
                Console.WriteLine("❌ Connection rejected: Invalid or missing custom_id.");
                Context.Abort();
                return;
            }

            // Store user connection
            Connections[userId] = Context.ConnectionId;
            Console.WriteLine($"✅ Firebase user {userId} connected (Connection ID: {Context.ConnectionId})");

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Invalid Firebase Token: {ex.Message}");
            Context.Abort();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Connections.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

        if (userId != 0)
        {
            Connections.TryRemove(userId, out _);
            Console.WriteLine($"❌ Firebase user {userId} disconnected.");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Check if user is online
    public bool IsUserOnline(int userId)
    {
        return Connections.ContainsKey(userId);
    }
}