using DemoMessage.Consumers.Consumer;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});
// Add services to the container.

var service = builder.Services;
service.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
service.AddEndpointsApiExplorer();
service.AddSwaggerGen();

service.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    
    x.AddConsumers(typeof(Program).Assembly);
    
    x.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(new Uri(builder.Configuration["MessageBroker:Host"]!), h =>
        {
            h.Username(builder.Configuration["MessageBroker:Username"]!);
            h.Password(builder.Configuration["MessageBroker:Password"]!);
        });
        
        configurator.ReceiveEndpoint("test", e =>
        {
            e.ConfigureConsumer<DemoMessageConsumer>(context);
        });
    });
});

service.AddOpenTelemetry()
    .ConfigureResource(resource => 
        resource.AddService(
            serviceName: "Consumer", 
            serviceVersion: "1.0.0.0"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation() // Take care of ASP.NET Core internal request pipeline
            .AddHttpClientInstrumentation() // Sending http request to 3rd party api
            .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName); // MassTransit

        tracing.AddOtlpExporter();
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();