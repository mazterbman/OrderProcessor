using Microsoft.EntityFrameworkCore;
using OrderProcessor.Domain;
using OrderProcessor.Infrastructure;
using Testcontainers.PostgreSql;

namespace OrderProcessor.TestDb;

public class OrderRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder().Build();
    private DbContextOptions<OrderDbContext> _options = null!;
    private OrderDbContext _dbContext = null!; 
        
    [Fact]
    public async Task SaveOrder_ThenOrderExistInDb()
    {
        var order = new Order([
            new OrderItem(Guid.NewGuid(), 1, 100),
            new OrderItem(Guid.NewGuid(), 1, 200),
            new OrderItem(Guid.NewGuid(), 1, 300),
            new OrderItem(Guid.NewGuid(), 1, 400)
        ]);
        var repository = new OrderRepository(_dbContext);
        var unitOfWork = new UnitOfWork(_dbContext);
        
        repository.Add(order);
        await unitOfWork.SaveChangesAsync();
        
        await using var context = new OrderDbContext(_options);
        var loaded = await context.Orders.FindAsync(order.Id);
        Assert.NotNull(loaded);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var connectionString = _container.GetConnectionString();
        _options = new DbContextOptionsBuilder<OrderDbContext>().UseNpgsql(connectionString).Options;
        _dbContext = new OrderDbContext(_options);
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _container.StopAsync();
        await _container.DisposeAsync();
    }
}