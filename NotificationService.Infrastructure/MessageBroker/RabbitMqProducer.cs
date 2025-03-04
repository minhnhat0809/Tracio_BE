using System.Text.Json;
using MassTransit;
using NotificationService.Application.Interfaces;

namespace NotificationService.Infrastructure.MessageBroker;

public class RabbitMqProducer(ISendEndpointProvider sendEndpointProvider) : IRabbitMqProducer
{
    private readonly ISendEndpointProvider _sendEndpointProvider = sendEndpointProvider;

    public async Task SendAsync<T>(T message, string queueName, CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            Console.WriteLine($"[MassTransit] Warning: Attempted to send a null message to {queueName}. Ignoring.");
            return;
        }

        try
        {
            Console.WriteLine($"[MassTransit] Sending message to queue {queueName}: {JsonSerializer.Serialize(message)}");

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueName}"));
            await sendEndpoint.Send(message, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MassTransit] ERROR sending message to {queueName}: {ex.Message}");
            throw;
        }
    }
}