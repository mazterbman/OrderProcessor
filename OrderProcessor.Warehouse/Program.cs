using Confluent.Kafka;
using OrderProcessor.Warehouse;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<WarehouseWorker>();

builder.Services.AddSingleton<IConsumer<string, string>>(sp =>
{
    var config = new ConsumerConfig
    {
        BootstrapServers = builder.Configuration.GetSection("Kafka")["BootstrapServers"],
        GroupId = builder.Configuration.GetSection("Kafka")["GroupId"],
        EnableAutoCommit = false,
        AutoOffsetReset = AutoOffsetReset.Earliest
    };
    return new ConsumerBuilder<string, string>(config).Build();
});

var host = builder.Build();
host.Run();