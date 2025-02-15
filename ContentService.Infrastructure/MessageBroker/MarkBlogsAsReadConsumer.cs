using ContentService.Application.DTOs.BlogDtos.Message;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace ContentService.Infrastructure.MessageBroker;

public class MarkBlogsAsReadConsumer(
    IServiceScope serviceScopeFactory, 
    IConnectionFactory connectionFactory, 
    string queueName, 
    IChannel? channel) : RabbitMqConsumer<MarkBlogsAsReadMessage>(serviceScopeFactory, connectionFactory, queueName, channel)
{
    protected override async Task ProcessMessageAsync(MarkBlogsAsReadMessage message, IServiceScope serviceScope,
        CancellationToken cancellationToken)
    {
        try
        {
            var blogRepo = serviceScope.ServiceProvider.GetRequiredService<IFollowerOnlyBlogRepo>();
            
            await blogRepo.UpdateRangeAsync(b => message.BlogIds.Contains(b.BlogId) && b.UserId == message.UserId,
                bf => bf.SetProperty(b => b.IsRead, true));
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }   
    }
}