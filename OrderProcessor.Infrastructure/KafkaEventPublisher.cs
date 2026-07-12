using System.Text.Json;
using Confluent.Kafka;
using OrderProcessor.Application;
using OrderProcessor.Contracts;

namespace OrderProcessor.Infrastructure;

public class KafkaEventPublisher : IDisposable, IEventPublisher
{
    private readonly IProducer<string, string> _producer;

    public KafkaEventPublisher(IProducer<string, string> producer)
    {
        _producer = producer;
    }
    
    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
    }

    public async Task PublishAsync<T>(T evt, CancellationToken cancellationToken = default) where T : IEvent
    {
        var json = JsonSerializer.Serialize(evt);
        var message = new Message<string, string>
        {
            Key = evt.OrderId,
            Value = json
        };
        //TODO
        // Нужно Предусмотреть try catch с log ошибки 
        await _producer.ProduceAsync("order_created", message, cancellationToken);
    }
}