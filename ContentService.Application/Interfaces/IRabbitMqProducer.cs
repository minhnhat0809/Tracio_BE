namespace ContentService.Application.Interfaces;

public interface IRabbitMqProducer
{
    Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default);
}