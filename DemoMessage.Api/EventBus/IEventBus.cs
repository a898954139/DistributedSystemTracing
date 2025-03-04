namespace Producer.EventBus;

public interface IEventBus
{
    Task SendAsync<T>(string queueName, T message, CancellationToken cancellationToken = default)
        where T : class;
}