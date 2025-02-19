using ContentService.Application.DTOs.CommentDtos.Message;
using ContentService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ContentService.Infrastructure.MessageBroker.CommentConsumers;

public class CommentDeletedConsumer(IServiceScopeFactory serviceScopeFactory, IConnectionFactory connectionFactory) 
    : RabbitMqConsumer<CommentDeleteEvent>(serviceScopeFactory, connectionFactory, "content_deleted")
{
    protected override async Task ProcessMessageAsync(CommentDeleteEvent message, IServiceScopeFactory serviceScope,
        CancellationToken cancellationToken)
    {
        using var scope = serviceScope.CreateScope();
        var commentRepo = scope.ServiceProvider.GetRequiredService<ICommentRepo>();
        
        var blogRepo = scope.ServiceProvider.GetRequiredService<IBlogRepo>();

        var blogId = await commentRepo.GetByIdAsync(c => c.CommentId == message.BlogId, c => c.BlogId);

        await blogRepo.UpdateFieldsAsync(b => b.BlogId == blogId,
            b => b.SetProperty(bb => bb.CommentsCount, bb => bb.CommentsCount - 1));
        
        Console.WriteLine($"[RabbitMQ] Processed CommentDeleted for Blog {message.BlogId}");
    }
}