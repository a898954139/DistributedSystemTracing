using Contract;
using MassTransit;

namespace Producer.Publisher;

public class Worker(IBus bus, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var text = new DemoMessage
            {
                Value = $"The current time is {DateTime.UtcNow}"
            };
            
            await bus.Publish(text, stoppingToken);
            
            logger.LogInformation("Published message: {@Text}", text);
            await Task.Delay(10_000, stoppingToken);
        }
    }
}