using ContentService.Application.DTOs.ReplyDtos.Message;
using ContentService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ContentService.Infrastructure.MessageBroker.ReplyConsumers;

public class ReplyCreatedConsumer(IServiceScopeFactory serviceScopeFactory) : IConsumer<ReplyCreateEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Consume(ConsumeContext<ReplyCreateEvent> context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        
        var commentRepo = scope.ServiceProvider.GetRequiredService<ICommentRepo>();

        await commentRepo.UpdateFieldsAsync(c => c.CommentId == context.Message.CommentId,
            c => c.SetProperty(cc => cc.RepliesCount, cc => cc.RepliesCount + 1));
        
        Console.WriteLine($"[RabbitMQ] Processed ReplyCreatedEvent for Comment {context.Message.CommentId}");
    }
}