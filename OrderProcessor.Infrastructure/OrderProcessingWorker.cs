using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderProcessor.Application;
using OrderProcessor.Contracts;
using OrderProcessor.Domain;

namespace OrderProcessor.Infrastructure;

public class OrderProcessingWorker : BackgroundService
{
    private readonly IEventPublisher _publisher;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OrderProcessingWorker> _logger;

    public OrderProcessingWorker(IServiceScopeFactory serviceScopeFactory, ILogger<OrderProcessingWorker> logger, IEventPublisher publisher)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _publisher = publisher;
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

                using var semaphoreProcessing = new SemaphoreSlim(10);
                var ordersCompleted = await ProcessingOrdersAsync(orders, semaphoreProcessing, unitOfWork, stoppingToken);

                using var semaphorePublish = new SemaphoreSlim(10);
                await PublishOrdersAsync(ordersCompleted, semaphorePublish, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Игнорируем
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Ошибка при обработке заказов в фоновом воркере");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task<Order?[]> ProcessingOrdersAsync(List<Order> orders, SemaphoreSlim semaphore,
        IUnitOfWork unitOfWork, CancellationToken stoppingToken)
    {
        var ordersCompleted = await Task.WhenAll(orders.Select(o => ProcessOrderAsync(o, semaphore, stoppingToken)));
        await unitOfWork.SaveChangesAsync(stoppingToken);
        _logger.LogInformation("Сохранение выполнено");
        return ordersCompleted;
    }

    private async Task PublishOrdersAsync(Order?[] ordersCompleted,
        SemaphoreSlim semaphore, CancellationToken stoppingToken)
    {
        await Task.WhenAll(ordersCompleted.Where(o => o is not null).Select(o =>
            PublishOrderAsync(new OrderCreatedEvent(o!.Id.ToString(), DateTimeOffset.UtcNow), semaphore,
                stoppingToken)));
        _logger.LogInformation("Отправил события все в Kafka");
    }

    private async Task<Order?> ProcessOrderAsync(Order order,
        SemaphoreSlim semaphore,
        CancellationToken stoppingToken)
    {
        await semaphore.WaitAsync(stoppingToken);
        try
        {
            order.StartProcessing();
            return order;
        }
        catch(OrderValidationException ex)
        {
            _logger.LogWarning(ex, "Заказ {OrderId} не переведён в обработку", order.Id);
            return null;
        }
        finally
        {
            semaphore.Release();
        }
    }
    
    private async Task PublishOrderAsync(OrderCreatedEvent orderCreatedEvent,
        SemaphoreSlim semaphore,
        CancellationToken stoppingToken)
    {
        await semaphore.WaitAsync(stoppingToken);
        try
        {
            await _publisher.PublishAsync(orderCreatedEvent, stoppingToken);
        }
        catch(KafkaException ex)
        {
            _logger.LogWarning(ex, "Ошибка при работе с Kafka заказ {OrderId}", orderCreatedEvent.OrderId);
        }
        finally
        {
            semaphore.Release();
        }
    }
}