using Microsoft.EntityFrameworkCore;
using OrderProcessor.Application;
using OrderProcessor.Domain;

namespace OrderProcessor.Infrastructure;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _orderDbContext;
    public OrderRepository(OrderDbContext orderDbContext)
    {
        _orderDbContext = orderDbContext;
    }
    
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _orderDbContext.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken: cancellationToken);
    }

    public async Task<List<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await _orderDbContext.Orders.Where(order => order.Status == OrderStatus.Pending).ToListAsync(cancellationToken: cancellationToken);
    }

    public void Add(Order order)
    {
        _orderDbContext.Orders.Add(order);
    }
}