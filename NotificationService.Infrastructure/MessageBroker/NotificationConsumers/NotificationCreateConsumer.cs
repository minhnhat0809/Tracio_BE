using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Dtos.NotificationDtos.ViewDtos;
using NotificationService.Application.Interfaces;
using NotificationService.Application.SignalR;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using Shared.Dtos.Messages;

namespace NotificationService.Infrastructure.MessageBroker.NotificationConsumers;

public class NotificationCreateConsumer(IServiceScopeFactory serviceScopeFactory, 
                                        IHubContext<NotificationHub> hubContext, 
                                        IMapper mapper, 
                                        ConnectionManager connectionManager,
                                        IFirebaseNotificationService firebaseNotificationService) : IConsumer<NotificationEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IMapper _mapper = mapper;
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;
    private readonly ConnectionManager _connectionManager = connectionManager;
    private readonly IFirebaseNotificationService _firebaseNotificationService = firebaseNotificationService;

    public async Task Consume(ConsumeContext<NotificationEvent> context)
{
    try
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepo>();
        
        var notification = _mapper.Map<Notification>(context.Message);

        var isSucceed = await notificationRepo.CreateAsync(notification);

        if (!isSucceed) 
        {
            Console.WriteLine($"[RabbitMQ] ERROR: Failed to create notification for {context.Message.RecipientId}");
            return; // ❌ Do not throw an exception; just log and exit
        }

        var notificationDto = _mapper.Map<NotificationDto>(notification);

        // ✅ Check if the recipient is online
        if (_connectionManager.IsUserOnline(context.Message.RecipientId))
        {
            var connectionId = _connectionManager.GetConnectionId(context.Message.RecipientId);
            
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveNotification", notificationDto);
            
            Console.WriteLine($"[RabbitMQ] Sent Notification to Online User {context.Message.RecipientId}");
        }
        else
        {
            var deviceFcmRepo = scope.ServiceProvider.GetRequiredService<IDeviceFcmRepo>();
            
            var fcmToken = await deviceFcmRepo.FindAsync(d => d.UserId == context.Message.RecipientId,
                d => d.FcmToken);

            foreach (var token in fcmToken)
            {
                await _firebaseNotificationService.SendPushNotification(
                    token, 
                    $"New {((EntityType)notification.EntityType).ToString()} from {notification.SenderName}", 
                    notificationDto);
            }
            
            Console.WriteLine($"[RabbitMQ] User {context.Message.RecipientId} is offline, storing notification.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Consumer] ERROR processing message: {ex.Message}");
    }
}

}
