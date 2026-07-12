using OrderProcessor.Contracts;

namespace OrderProcessor.Application;

public interface IEventPublisher
{
    Task PublishAsync<T>(T evt, CancellationToken cancellationToken = default) where T : IEvent;
}

