using FirebaseAdmin.Messaging;

namespace NotificationService.Application.Services;

public class FirebaseNotificationService
{
    public async Task<string> SendPushNotification(string token, string title, string body)
    {
        var message = new Message()
        {
            Token = token, // The recipient's FCM device token
            Notification = new Notification()
            {
                Title = title,
                Body = body
            }
        };

        // Send the notification
        var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        return response;
    }
}