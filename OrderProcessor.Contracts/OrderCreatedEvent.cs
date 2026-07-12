namespace OrderProcessor.Contracts;

public record OrderCreatedEvent(string OrderId, DateTimeOffset CreatedAt) : IEvent;

// Пустой маркер интерфейс
public interface IEvent
{
    string OrderId { get; }
}