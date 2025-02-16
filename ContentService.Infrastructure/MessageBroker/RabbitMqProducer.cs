using System.Text;
using System.Text.Json;
using ContentService.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace ContentService.Infrastructure.MessageBroker;

public class RabbitMqProducer(IConnectionFactory factory) : IRabbitMqProducer, IAsyncDisposable
{
    private readonly Task<IConnection> _connectionTask = factory.CreateConnectionAsync(); // ✅ Use async connection initialization
    private IChannel? _channel;

    public async Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken = default)
    {
        try
        {
            var connection = await _connectionTask; // ✅ Await the connection
            _channel ??= await connection.CreateChannelAsync(cancellationToken: cancellationToken); // ✅ Ensure channel is initialized

            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null, cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = new BasicProperties
            {
                Persistent = true 
            };

            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);
        }
        catch (BrokerUnreachableException ex)
        {
            Console.WriteLine($"[RabbitMQ] Connection error: {ex.Message}");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }

        if (await _connectionTask is { } connection)
        {
            await connection.CloseAsync();
            await connection.DisposeAsync();
        }
    }
}
