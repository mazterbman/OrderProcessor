namespace OrderProcessor.Application;

public interface IOrderService
{
    Task<Guid> CreateOrderAsync(CreateOrderRequest orderRequest, CancellationToken cancellationToken = default);
}