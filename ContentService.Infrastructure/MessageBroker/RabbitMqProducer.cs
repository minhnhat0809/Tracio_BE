using System.Text.Json;
using ContentService.Application.Interfaces;
using MassTransit;

namespace ContentService.Infrastructure.MessageBroker;

public class RabbitMqProducer(IPublishEndpoint publishEndpoint) : IRabbitMqProducer
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task SendAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            Console.WriteLine($"[MassTransit] Warning: Attempted to publish a null message with routing key '{routingKey}'. Ignoring.");
            return;
        }

        try
        {
            Console.WriteLine($"[MassTransit] Publishing message to exchange with routing key '{routingKey}': {JsonSerializer.Serialize(message)}");

            await _publishEndpoint.Publish(message, context =>
            {
                context.SetRoutingKey(routingKey);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MassTransit] ERROR publishing message with routing key '{routingKey}': {ex.Message}");
            throw;
        }
    }

    public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            Console.WriteLine($"[MassTransit] Warning: Attempted to publish a null message. Ignoring.");
            return;
        }

        try
        {
            Console.WriteLine($"[MassTransit] Publishing message to fanout exchange: {JsonSerializer.Serialize(message)}");

            await _publishEndpoint.Publish(message, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MassTransit] ERROR publishing message: {ex.Message}");
            throw;
        }
    }
}