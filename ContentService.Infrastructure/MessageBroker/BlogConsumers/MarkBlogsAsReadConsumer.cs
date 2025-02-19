using ContentService.Application.DTOs.BlogDtos.Message;
using ContentService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ContentService.Infrastructure.MessageBroker.BlogConsumers;

public class MarkBlogsAsReadConsumer(IServiceScopeFactory serviceScopeFactory, IConnectionFactory connectionFactory) 
    : RabbitMqConsumer<MarkBlogsAsReadEvent>(serviceScopeFactory, connectionFactory, "mark-blogs-as-read")
{
    protected override async Task ProcessMessageAsync(MarkBlogsAsReadEvent @event, IServiceScopeFactory serviceScope, CancellationToken cancellationToken)
    {
        using var scope = serviceScope.CreateScope();
        var followerOnlyBlogRepo = scope.ServiceProvider.GetRequiredService<IFollowerOnlyBlogRepo>();

        await followerOnlyBlogRepo.MarkBlogsAsReadAsync(@event.UserId, @event.BlogIds);

        Console.WriteLine($"[RabbitMQ] Processed MarkBlogsAsRead for User {@event.UserId}");
    }
}