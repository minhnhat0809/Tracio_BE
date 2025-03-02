using NotificationService.Application.Dtos.NotificationDtos.ViewDtos;

namespace NotificationService.Application.Interfaces;

public interface IFirebaseNotificationService
{
    Task<string> SendPushNotification(string token, string title, NotificationDto notification);
}