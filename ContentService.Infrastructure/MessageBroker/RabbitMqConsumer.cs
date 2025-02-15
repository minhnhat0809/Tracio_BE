using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace ContentService.Infrastructure.MessageBroker;

public abstract class RabbitMqConsumer<TMessage>(
    IServiceScope serviceScopeFactory,
    IConnectionFactory connectionFactory,
    string queueName,
    IChannel? channel)
    : BackgroundService
{
    private readonly Task<IConnection> _connectionTask = connectionFactory.CreateConnectionAsync(); // ✅ Use Async Connection\
    private IChannel? _channel = channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var connection = await _connectionTask; // ✅ Wait for async connection
            _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken); // ✅ Create async channel

            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var data = JsonSerializer.Deserialize<TMessage>(message);
                    if (data != null)
                    {
                        await ProcessMessageAsync(data, serviceScopeFactory, stoppingToken); // ✅ Process the message
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RabbitMQ] Error processing message: {ex.Message}");
                }
            };

            await _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer, cancellationToken: stoppingToken);
        }
        catch (BrokerUnreachableException ex)
        {
            Console.WriteLine($"[RabbitMQ] Connection error: {ex.Message}");
        }
    }

    protected abstract Task ProcessMessageAsync(TMessage message, IServiceScope serviceScope, CancellationToken cancellationToken);

    public override void Dispose()
    {
        _channel?.CloseAsync();
        base.Dispose();
    }
}
