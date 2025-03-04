using MassTransit;
using Producer.Publisher;

namespace Producer.EventBus;

public sealed class EventBus(IServiceProvider serviceProvider) : IEventBus
{
    public async Task SendAsync<T>(string queueName, T message, CancellationToken cancellationToken = default) where T : class
    {
        using var scope = serviceProvider.CreateScope();
        var sendEndpointProvider = scope.ServiceProvider.GetRequiredService<ISendEndpointProvider>();
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{queueName}"));
        await sendEndpoint.Send(message, cancellationToken);   
    }
}