using System.Text.Json;
using ContentService.Application.Interfaces;
using MassTransit;

namespace ContentService.Infrastructure.MessageBroker;

public class RabbitMqProducer(IPublishEndpoint publishEndpoint) : IRabbitMqProducer
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    
    public async Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            Console.WriteLine($"[MassTransit] Warning: Attempted to publish a null message to {queueName}. Ignoring.");
            return; 
        }
        
        try
        {
            Console.WriteLine($"[MassTransit] Publishing message to {queueName}: {JsonSerializer.Serialize(message)}");

            await _publishEndpoint.Publish(message, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MassTransit] ERROR publishing message to {queueName}: {ex.Message}");
            throw;
        }
    }
}
