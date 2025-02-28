using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Application.Dtos.NotificationDtos.Message;
using NotificationService.Application.Interfaces;
using NotificationService.Application.SignalR;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.MessageBroker.NotificationConsumers;

public class NotificationCreateConsumer(IServiceScopeFactory serviceScopeFactory, IHubContext<NotificationHub> hubContext, IMapper mapper) : IConsumer<NotificationEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    
    private readonly IMapper _mapper = mapper;
    
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;
    
    public async Task Consume(ConsumeContext<NotificationEvent> context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepo>();
        
        var notification = _mapper.Map<Notification>(context.Message);

        var isSucceed = await notificationRepo.CreateAsync(notification);

        if (isSucceed) throw new Exception("Fail to create notification");
        
        await _hubContext.Clients.User($"Notification-{context.Message.RecipientId}").SendAsync("ReceiveNotification", notification);
        
        Console.WriteLine($"[RabbitMQ] Processed Notification for User {context.Message.RecipientId}");
    }
}