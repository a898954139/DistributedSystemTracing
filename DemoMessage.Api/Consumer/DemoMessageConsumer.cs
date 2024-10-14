using Contract;
using MassTransit;

namespace Producer.Consumer;

public class DemoMessageConsumer(ILogger<DemoMessageConsumer> logger): IConsumer<DemoMessage>
{
    public Task Consume(ConsumeContext<DemoMessage> context)
    {
        logger.LogInformation(
            "{Consumer}: {Message}",
            GetType().Name,
            context.Message.Value);
        
        return Task.CompletedTask;
    }
}