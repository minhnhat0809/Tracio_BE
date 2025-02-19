using ContentService.Application.DTOs.ReplyDtos.Message;
using ContentService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ContentService.Infrastructure.MessageBroker.ReplyConsumers;

public class ReplyCreatedConsumer(IServiceScopeFactory serviceScopeFactory, IConnectionFactory connectionFactory) 
    : RabbitMqConsumer<ReplyCreateEvent>(serviceScopeFactory, connectionFactory, "reply_created")
{
    protected override async Task ProcessMessageAsync(ReplyCreateEvent message, IServiceScopeFactory serviceScope,
        CancellationToken cancellationToken)
    {
        using var scope = serviceScope.CreateScope();
        
        var commentRepo = scope.ServiceProvider.GetRequiredService<ICommentRepo>();

        await commentRepo.UpdateFieldsAsync(c => c.CommentId == message.CommentId,
            c => c.SetProperty(cc => cc.RepliesCount, cc => cc.RepliesCount + 1));
        
        Console.WriteLine($"[RabbitMQ] Processed ReplyCreatedEvent for Comment {message.CommentId}");
    }
}