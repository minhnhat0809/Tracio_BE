using FirebaseAdmin.Messaging;
using NotificationService.Application.Dtos.NotificationDtos.ViewDtos;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Services;

public class FirebaseNotificationServiceService : IFirebaseNotificationService
{
    public async Task<string> SendPushNotification(string token, string title, NotificationDto notification)
    {
        var message = new Message()
        {
            Token = token, 
            Notification = new Notification
            {
                Title = title,
                Body = notification.Message
            },
            Data = new Dictionary<string, string>()
            {
                { "SenderName", notification.SenderName },
                { "SenderAvatar", notification.SenderAvatar },
                { "EntityId", notification.EntityId.ToString() },
                { "EntityType", notification.EntityType.ToString() },
                { "CreatedAt", notification.CreatedAt.ToLongDateString() }
            }
        };

        // Send the notification
        var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        return response;
    }
}