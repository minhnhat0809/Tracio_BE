using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Application.SignalR;

public class NotificationHub(IHttpContextAccessor httpContextAccessor, ConnectionManager connectionManager) : Hub
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ConnectionManager _connectionManager = connectionManager;

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

            // Store user connection in ConnectionManager
            _connectionManager.AddConnection(userId, Context.ConnectionId);
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
        var userId = _connectionManager.GetUserIdByConnectionId(Context.ConnectionId);

        if (userId != null)
        {
            _connectionManager.RemoveConnection(userId.Value);
            Console.WriteLine($"❌ Firebase user {userId} disconnected.");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
