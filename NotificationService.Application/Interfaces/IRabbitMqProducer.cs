namespace NotificationService.Application.Interfaces;

public interface IRabbitMqProducer
{
    Task SendAsync<T>(T message, string queueName, CancellationToken cancellationToken = default);
}