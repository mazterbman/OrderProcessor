using OrderProcessor.Domain;

namespace OrderProcessor.Application;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);
    void Add(Order order);
}