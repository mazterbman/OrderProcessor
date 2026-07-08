using OrderProcessor.Application;

namespace OrderProcessor.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrderDbContext _orderDbContext;

    public UnitOfWork(OrderDbContext orderDbContext)
    {
        _orderDbContext = orderDbContext;
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _orderDbContext.SaveChangesAsync(cancellationToken);
    }
}