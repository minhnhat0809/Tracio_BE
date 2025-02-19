using ContentService.Application.DTOs.CommentDtos.Message;
using ContentService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ContentService.Infrastructure.MessageBroker.CommentConsumers;

public class CommentCreatedConsumer(IServiceScopeFactory serviceScopeFactory, IConnectionFactory connectionFactory) 
    : RabbitMqConsumer<CommentCreateEvent>(serviceScopeFactory, connectionFactory, "comment_created")
{
    protected override async Task ProcessMessageAsync(CommentCreateEvent message, IServiceScopeFactory serviceScope,
        CancellationToken cancellationToken)
    {
        using var scope = serviceScope.CreateScope();
        var blogRepo = scope.ServiceProvider.GetRequiredService<IBlogRepo>();

        await blogRepo.UpdateFieldsAsync(b => b.BlogId == message.BlogId,
            b => b.SetProperty(bb => bb.CommentsCount, bb => bb.CommentsCount + 1));
        
        Console.WriteLine($"[RabbitMQ] Processed CommentCreated for Blog {message.BlogId}");
    }
}