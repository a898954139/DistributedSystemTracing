using Elastic.Channels;
using Elastic.CommonSchema.Serilog;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using MassTransit;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Producer.EventBus;
using Producer.Publisher;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Seq appsettings logging configure
builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Elasticsearch(
            [new Uri(builder.Configuration["ElasticConfiguration:Uri"])],
            options =>
            {
                options.DataStream = new DataStreamName("logs", "demo");
                options.BootstrapMethod = BootstrapMethod.Failure;
                options.ConfigureChannel = channelOptions =>
                {
                    channelOptions.BufferOptions = new BufferOptions
                    {
                        ExportMaxConcurrency = 10
                    };
                };
            });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var service = builder.Services;
service.AddEndpointsApiExplorer();
service.AddSwaggerGen();

service.AddTransient<IEventBus, EventBus>();
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
    });
});

service.AddOpenTelemetry()
    .ConfigureResource(resource => 
        resource.AddService(
            serviceName: "Publisher", 
            serviceVersion: "1.0.0.0"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation() // Take care of ASP.NET Core internal request pipeline
            .AddHttpClientInstrumentation() // Sending http request to 3rd party api
            .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName); // MassTransit

        tracing.AddOtlpExporter();
    });


service.AddHostedService<Worker>();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();