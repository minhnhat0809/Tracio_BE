using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Dtos.NotificationDtos.Message;
using NotificationService.Application.Interfaces;
using NotificationService.Application.SignalR;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.MessageBroker.NotificationConsumers;

public class NotificationCreateConsumer(IServiceScopeFactory serviceScopeFactory, 
                                        IHubContext<NotificationHub> hubContext, 
                                        IMapper mapper, 
                                        ConnectionManager connectionManager) : IConsumer<NotificationEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly IMapper _mapper = mapper;
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;
    private readonly ConnectionManager _connectionManager = connectionManager;

    public async Task Consume(ConsumeContext<NotificationEvent> context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepo>();
        
        var notification = _mapper.Map<Notification>(context.Message);

        var isSucceed = await notificationRepo.CreateAsync(notification);

        if (!isSucceed) throw new Exception("Fail to create notification");

        // ✅ Check if the recipient is online
        if (_connectionManager.IsUserOnline(context.Message.RecipientId))
        {
            var connectionId = _connectionManager.GetConnectionId(context.Message.RecipientId);
            
            await _hubContext.Clients.Client(connectionId)
                .SendAsync("ReceiveNotification", notification);
            
            Console.WriteLine($"[RabbitMQ] Sent Notification to Online User {context.Message.RecipientId}");
        }
        else
        {
            Console.WriteLine($"[RabbitMQ] User {context.Message.RecipientId} is offline, storing notification.");
        }
    }
}
