using ContentService.Application.DTOs.ReplyDtos.Message;
using ContentService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ContentService.Infrastructure.MessageBroker.ReplyConsumers;

public class ReplyDeletedConsumer(IServiceScopeFactory serviceScopeFactory, IConnectionFactory connectionFactory) 
    : RabbitMqConsumer<ReplyDeleteEvent>(serviceScopeFactory, connectionFactory, "content_deleted")
{
    protected override async Task ProcessMessageAsync(ReplyDeleteEvent message, IServiceScopeFactory serviceScope,
        CancellationToken cancellationToken)
    {
        using var scope = serviceScope.CreateScope();
        
        var commentRepo = scope.ServiceProvider.GetRequiredService<ICommentRepo>();

        await commentRepo.UpdateFieldsAsync(c => c.CommentId == message.CommentId,
            c => c.SetProperty(cc => cc.RepliesCount, cc => cc.RepliesCount - 1));
        
        Console.WriteLine($"[RabbitMQ] Processed ReplyDeletedEvent for Comment {message.CommentId}");
    }
}