using System.Text;
using System.Text.Json;
using ContentService.Application.DTOs.BlogDtos.Message;
using ContentService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ContentService.Infrastructure.MessageBroker;

public class MarkBlogsAsReadConsumer(
    IServiceScopeFactory serviceScopeFactory,
    IConnectionFactory connectionFactory) : BackgroundService
{
    private readonly Task<IConnection> _connectionTask = connectionFactory.CreateConnectionAsync(); // ✅ Use Async Connection\
    
    private IChannel? _channel;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await _connectionTask;
                _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await _channel.QueueDeclareAsync(
                    queue: "mark-blogs-as-read",
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
                    var data = JsonSerializer.Deserialize<MarkBlogsAsReadMessage>(message);

                    if (data is null)
                    {
                        Console.WriteLine("[RabbitMQ] Received null message.");
                        await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                        return;
                    }

                    using var scope = serviceScopeFactory.CreateScope();
                    
                    var followerOnlyBlogRepo = scope.ServiceProvider.GetRequiredService<IFollowerOnlyBlogRepo>();

                    // update IsRead 
                    await followerOnlyBlogRepo.MarkBlogsAsReadAsync(data.UserId, data.BlogIds);
                    
                    Console.WriteLine("[RabbitMQ] Received");

                    await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                };

                await _channel.BasicConsumeAsync(queue: "mark-blogs-as-read", autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

                Console.WriteLine("[RabbitMQ] Consumer is running...");
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ] Consumer error: {ex.Message}. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken); // Retry after 5s
            }
        }
    }


}