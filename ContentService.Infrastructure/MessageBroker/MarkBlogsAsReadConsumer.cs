using System.Text;
using System.Text.Json;
using ContentService.Application.DTOs.BlogDtos.Message;
using ContentService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace ContentService.Infrastructure.MessageBroker;

public class MarkBlogsAsReadConsumer(
    IServiceScopeFactory serviceScopeFactory,
    IConnectionFactory connectionFactory) : BackgroundService
{
    private readonly Task<IConnection> _connectionTask = connectionFactory.CreateConnectionAsync(); // ✅ Use Async Connection\
    
    private IChannel? _channel;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await using var connection = await _connectionTask; // ✅ Wait for async connection
            _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken); // ✅ Create async channel

            await _channel.QueueDeclareAsync(
                queue: "mark-blogs-as-read",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var data = JsonSerializer.Deserialize<MarkBlogsAsReadMessage>(message);
                
                if (data is null)
                {
                    Console.WriteLine("[RabbitMQ] Received null message.");
                    return;
                }

                using var scope = serviceScopeFactory.CreateScope();
                var blogRepo = scope.ServiceProvider.GetRequiredService<IFollowerOnlyBlogRepo>();

                // ✅ Update database with the received data
                await blogRepo.UpdateFieldsAsync(
                    b => data.BlogIds.Contains(b.BlogId) && b.UserId == data.UserId,
                    bf => bf.SetProperty(b => b.IsRead, true));
            };

            await _channel.BasicConsumeAsync(queue: "mark-blogs-as-read", autoAck: true, consumer: consumer, cancellationToken: stoppingToken);
        }
        catch (BrokerUnreachableException ex)
        {
            Console.WriteLine($"[RabbitMQ] Connection error: {ex.Message}");
        }
    }

}