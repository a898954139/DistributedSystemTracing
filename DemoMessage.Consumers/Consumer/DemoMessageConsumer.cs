using MassTransit;

namespace DemoMessage.Consumers.Consumer;

public class DemoMessageConsumer(ILogger<DemoMessageConsumer> logger): IConsumer<Contract.DemoMessage>
{
    public Task Consume(ConsumeContext<Contract.DemoMessage> context)
    {
        logger.LogInformation(
            "{Consumer}: {Message}",
            GetType().Name,
            context.Message.Value);
        
        return Task.CompletedTask;
    }
}