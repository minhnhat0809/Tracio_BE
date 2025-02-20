using ContentService.Application.DTOs.BlogDtos.Message;
using ContentService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ContentService.Infrastructure.MessageBroker.BlogConsumers;

public class MarkBlogsAsReadConsumer(IServiceScopeFactory serviceScopeFactory) : IConsumer<MarkBlogsAsReadEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Consume(ConsumeContext<MarkBlogsAsReadEvent> context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var followerOnlyBlogRepo = scope.ServiceProvider.GetRequiredService<IFollowerOnlyBlogRepo>();
        

        await followerOnlyBlogRepo.MarkBlogsAsReadAsync(context.Message.UserId, context.Message.BlogIds);

        Console.WriteLine($"[RabbitMQ] Processed MarkBlogsAsRead for User {context.Message.UserId}");
    }
}