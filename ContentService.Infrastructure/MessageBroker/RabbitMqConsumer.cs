using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace ContentService.Infrastructure.MessageBroker;

public abstract class RabbitMqConsumer<TMessage>(
    IServiceScopeFactory serviceScopeFactory,
    IConnectionFactory connectionFactory,
    string queueName)
    : BackgroundService
{
    private readonly Task<IConnection> _connectionTask = connectionFactory.CreateConnectionAsync();
    private IChannel? _channel;
    private const int MaxRetryAttempts = 3; // Maximum retries before moving to DLQ
    private const int RetryDelayMs = 5000; // Retry delay in milliseconds (5 seconds)

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await _connectionTask;
                _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                // ✅ Declare DLX (Dead-Letter Exchange)
                await _channel.ExchangeDeclareAsync($"{queueName}_dlx", ExchangeType.Direct, durable: true, autoDelete: false, cancellationToken: stoppingToken);

                // ✅ Declare DLQ (Dead-Letter Queue)
                await _channel.QueueDeclareAsync($"{queueName}_dlq", durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
                
                // ✅ Bind DLQ to DLX
                await _channel.QueueBindAsync($"{queueName}_dlq", $"{queueName}_dlx", $"{queueName}_dlq", cancellationToken: stoppingToken);

                // ✅ Declare Retry Queue (for automatic retries)
                await _channel.QueueDeclareAsync(
                    queue: $"{queueName}_retry",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", $"{queueName}_dlx" }, // Send failed messages to DLX if max retries reached
                        { "x-dead-letter-routing-key", queueName }, // Move back to original queue after delay
                        { "x-message-ttl", RetryDelayMs } // Delay before retrying
                    }!,
                    cancellationToken: stoppingToken);

                // ✅ Declare Main Queue with DLX & Retry
                await _channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: new Dictionary<string, object>
                    {
                        { "x-dead-letter-exchange", $"{queueName}_dlx" }, // Send failed messages to DLX
                        { "x-dead-letter-routing-key", $"{queueName}_retry" }, // Route to retry queue if failure
                    }!,
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
                            await ProcessMessageAsync(data, serviceScopeFactory, stoppingToken);
                        }
                        else
                        {
                            Console.WriteLine($"[RabbitMQ] Received null message on {queueName}");
                        }

                        await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[RabbitMQ] ERROR processing message on {queueName}: {ex.Message}");

                        // Extract retry count from headers
                        var retryCount = 0;
                        if (ea.BasicProperties.Headers != null && ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var header))
                        {
                            retryCount = Convert.ToInt32(header);
                        }

                        if (retryCount >= MaxRetryAttempts)
                        {
                            Console.WriteLine($"[RabbitMQ] Max retries reached ({retryCount}). Moving message to DLQ.");
                            await _channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken); // Move to DLQ
                        }
                        else
                        {
                            Console.WriteLine($"[RabbitMQ] Retrying message {retryCount + 1}/{MaxRetryAttempts}...");

                            var properties = new BasicProperties
                            {
                                Headers = new Dictionary<string, object>()!,
                                Persistent = ea.BasicProperties.Persistent,
                                ContentType = ea.BasicProperties.ContentType,
                                DeliveryMode = ea.BasicProperties.DeliveryMode
                            };
                            properties.Headers["x-retry-count"] = retryCount + 1; // Increment retry count

                            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), stoppingToken); // Exponential backoff

                            await _channel.BasicPublishAsync(
                                exchange: "", 
                                routingKey: queueName, 
                                mandatory: false,
                                basicProperties: properties,
                                body: ea.Body, 
                                cancellationToken: stoppingToken);

                            await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                        }
                    }
                };

                await _channel.BasicConsumeAsync(queue: queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

                Console.WriteLine($"[RabbitMQ] Consumer for {queueName} is running...");
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (BrokerUnreachableException ex)
            {
                Console.WriteLine($"[RabbitMQ] Connection error for {queueName}: {ex.Message}. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ] Consumer error for {queueName}: {ex.Message}. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    protected abstract Task ProcessMessageAsync(TMessage message, IServiceScopeFactory serviceScope, CancellationToken cancellationToken);

    public override void Dispose()
    {
        _channel?.CloseAsync();
        base.Dispose();
    }
}
