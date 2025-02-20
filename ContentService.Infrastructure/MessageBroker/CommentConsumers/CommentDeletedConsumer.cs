using ContentService.Application.DTOs.CommentDtos.Message;
using ContentService.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace ContentService.Infrastructure.MessageBroker.CommentConsumers;

public class CommentDeletedConsumer(IServiceScopeFactory serviceScopeFactory) : IConsumer<CommentDeleteEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task Consume(ConsumeContext<CommentDeleteEvent> context)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        
        var blogRepo = scope.ServiceProvider.GetRequiredService<IBlogRepo>();

        await blogRepo.UpdateFieldsAsync(b => b.BlogId == context.Message.BlogId,
            b => b.SetProperty(bb => bb.CommentsCount, bb => bb.CommentsCount - 1));
        
        Console.WriteLine($"[RabbitMQ] Processed CommentDeleted for Blog {context.Message.BlogId}");
    }
}