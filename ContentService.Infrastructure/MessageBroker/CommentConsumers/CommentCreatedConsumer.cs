using ContentService.Application.DTOs.CommentDtos.Message;
using ContentService.Application.Hubs;
using ContentService.Application.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ContentService.Infrastructure.MessageBroker.CommentConsumers;

public class CommentCreatedConsumer(IServiceScopeFactory serviceScopeFactory) : IConsumer<CommentCreateEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Consume(ConsumeContext<CommentCreateEvent> context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var blogRepo = scope.ServiceProvider.GetRequiredService<IBlogRepo>();

        await blogRepo.UpdateFieldsAsync(b => b.BlogId == context.Message.BlogId,
            b => b.SetProperty(bb => bb.CommentsCount, bb => bb.CommentsCount + 1));
        
        Console.WriteLine($"[RabbitMQ] Processed CommentCreated for Blog {context.Message.BlogId}");
    }
}