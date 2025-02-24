using ContentService.Application.DTOs.ReactionDtos.Message;
using ContentService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ContentService.Infrastructure.MessageBroker.ReactionConsumers;

public class ReactionDeletedConsumer(IServiceScopeFactory serviceScopeFactory) : IConsumer<ReactionDeleteEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Consume(ConsumeContext<ReactionDeleteEvent> context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        
        switch (context.Message.EntityType.ToLower())
        {
            case "comment":
            {
                var commentRepo = scope.ServiceProvider.GetRequiredService<ICommentRepo>();
            
                await commentRepo.UpdateFieldsAsync(c => c.CommentId == context.Message.EntityId, 
                    c => c.SetProperty(cc => cc.LikesCount, cc => cc.LikesCount - 1));
                break;
            }
            case "blog":
            {
                var blogRepo = scope.ServiceProvider.GetRequiredService<IBlogRepo>();
            
                await blogRepo.UpdateFieldsAsync(b => b.BlogId == context.Message.EntityId,
                    b => b.SetProperty(bb => bb.ReactionsCount, bb => bb.ReactionsCount - 1));
                break;
            }
            case "reply":
            {
                var replyRepo = scope.ServiceProvider.GetRequiredService<IReplyRepo>();
            
                await replyRepo.UpdateFieldsAsync(r => r.ReplyId == context.Message.EntityId,
                    r => r.SetProperty(rr => rr.LikesCount, rr => rr.LikesCount - 1));
                break;
            }
        }
        
        Console.WriteLine($"[RabbitMQ] Processed ReactionDeleted for {context.Message.EntityType} {context.Message.EntityId}");
    }
}