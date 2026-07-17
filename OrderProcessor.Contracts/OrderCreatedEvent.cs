namespace OrderProcessor.Contracts;

public record OrderCreatedEvent(string OrderId, DateTimeOffset CreatedAt) : IEvent;

// Пустой интерфейс-маркер
public interface IEvent
{
    string OrderId { get; }
}