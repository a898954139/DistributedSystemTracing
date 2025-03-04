using Contract;
using Producer.EventBus;

namespace Producer.Publisher;

public class Worker(IEventBus bus, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var text = new DemoMessage
            {
                Value = $"The current time is {DateTime.UtcNow}"
            };
            
            await bus.SendAsync("test", text, stoppingToken);
            
            logger.LogInformation("Published message: {@Text}", text);
            await Task.Delay(1000, stoppingToken);
        }
    }
}