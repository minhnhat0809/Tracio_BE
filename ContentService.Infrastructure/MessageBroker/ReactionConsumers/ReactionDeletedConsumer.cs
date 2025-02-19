using ContentService.Application.DTOs.ReactionDtos.Message;
using ContentService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ContentService.Infrastructure.MessageBroker.ReactionConsumers;

public class ReactionDeletedConsumer(IServiceScopeFactory serviceScopeFactory, IConnectionFactory connectionFactory) 
    : RabbitMqConsumer<ReactionDeleteEvent>(serviceScopeFactory, connectionFactory, "content_deleted")
{
    protected override async Task ProcessMessageAsync(ReactionDeleteEvent message, IServiceScopeFactory serviceScope,
        CancellationToken cancellationToken)
    {
        using var scope = serviceScope.CreateScope();
        
        switch (message.EntityType.ToLower())
        {
            case "comment":
            {
                var commentRepo = scope.ServiceProvider.GetRequiredService<ICommentRepo>();
            
                await commentRepo.UpdateFieldsAsync(c => c.CommentId == message.EntityId, 
                    c => c.SetProperty(cc => cc.LikesCount, cc => cc.LikesCount - 1));
                break;
            }
            case "blog":
            {
                var blogRepo = scope.ServiceProvider.GetRequiredService<IBlogRepo>();
            
                await blogRepo.UpdateFieldsAsync(b => b.BlogId == message.EntityId,
                    b => b.SetProperty(bb => bb.ReactionsCount, bb => bb.ReactionsCount - 1));
                break;
            }
            case "reply":
            {
                var replyRepo = scope.ServiceProvider.GetRequiredService<IReplyRepo>();
            
                await replyRepo.UpdateFieldsAsync(r => r.ReplyId == message.EntityId,
                    r => r.SetProperty(rr => rr.LikesCount, rr => rr.LikesCount - 1));
                break;
            }
        }
        
        Console.WriteLine($"[RabbitMQ] Processed ReactionDeleted for {message.EntityType} {message.EntityId}");
    }
}