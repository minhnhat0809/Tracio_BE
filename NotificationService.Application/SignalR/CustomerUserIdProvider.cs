using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Application.SignalR;

public class CustomerUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst("custom_id")?.Value;
    }
}