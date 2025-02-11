using System.Text;
using System.Text.Json;
using ContentService.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace ContentService.Infrastructure.MessageBroker;

public class RabbitMqProducer(IConnectionFactory factory) : IRabbitMqProducer
{
    private readonly Task<IConnection> _connectionTask = factory.CreateConnectionAsync();

    public async Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionTask; // ✅ Persistent connection from DI
            await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken); // ✅ Async channel

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null, cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = new BasicProperties(); // ✅ Explicitly define the properties type

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken
            );
        }
        catch (BrokerUnreachableException ex)
        {
            Console.WriteLine($"[RabbitMQ] Connection error: {ex.Message}");
            throw;
        }
    }
}