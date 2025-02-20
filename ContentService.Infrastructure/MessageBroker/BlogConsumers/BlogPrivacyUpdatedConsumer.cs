using ContentService.Application.DTOs.BlogDtos.Message;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ContentService.Infrastructure.MessageBroker.BlogConsumers;

public class BlogPrivacyUpdatedConsumer(IServiceScopeFactory serviceScopeFactory, IConnectionFactory connectionFactory) 
    : RabbitMqConsumer<BlogPrivacyUpdateEvent>(serviceScopeFactory, connectionFactory, "blog_privacy_updated")
{
    protected override async Task ProcessMessageAsync(BlogPrivacyUpdateEvent message, IServiceScopeFactory serviceScope,
        CancellationToken cancellationToken)
    {
        using var scope = serviceScope.CreateScope();
        var followerOnlyBlogRepo = scope.ServiceProvider.GetRequiredService<IFollowerOnlyBlogRepo>();

        switch (message.Action.ToLower())
        {
            case "add":
                
                break;
            case "remove":
                break;
        }

        Console.WriteLine($"[RabbitMQ] Processed BlogPrivacyUpdated for Blog {message.BlogId}");
    }
}