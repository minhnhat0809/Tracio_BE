namespace ContentService.Application.Interfaces;

public interface IRabbitMqProducer
{
    Task SendAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default);
    
    Task PublishAsync<T>(T message, CancellationToken cancellationToken = default);
}