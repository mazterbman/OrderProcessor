using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderProcessor.Application;
using OrderProcessor.Domain;

namespace OrderProcessor.Infrastructure;

public class OrderProcessingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OrderProcessingWorker> _logger;

    public OrderProcessingWorker(IServiceScopeFactory serviceScopeFactory, ILogger<OrderProcessingWorker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                var orders = await repository.GetPendingOrdersAsync(stoppingToken);
                _logger.LogInformation("Воркер проснулся, найдено Pending-заказов: {Count}", orders.Count);

                var semaphore = new SemaphoreSlim(10);
                await Task.WhenAll(orders.Select(o => ProcessOrderAsync(o, semaphore, stoppingToken)));
                await unitOfWork.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Сохранение выполнено");
            }
            catch (OperationCanceledException)
            {
                // Игнорируем
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Ошибка при обработке заказов в фоновом воркере");
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessOrderAsync(Order order,
        SemaphoreSlim semaphore,
        CancellationToken stoppingToken)
    {
        await semaphore.WaitAsync(stoppingToken);
        try
        {
            order.StartProcessing();
        }
        finally
        {
            semaphore.Release();
        }
    }
}