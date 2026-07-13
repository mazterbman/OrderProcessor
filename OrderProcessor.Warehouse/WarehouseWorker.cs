using System.Text.Json;
using Confluent.Kafka;
using OrderProcessor.Contracts;

namespace OrderProcessor.Warehouse;

public class WarehouseWorker : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<WarehouseWorker> _logger;

    public WarehouseWorker(ILogger<WarehouseWorker> logger, IConsumer<string, string> consumer)
    {
        _logger = logger;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _consumer.Subscribe("order_created");
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!await ExecuteConsumerResult(stoppingToken))
                    break;
            }
        }
        finally
        {
            _consumer.Close();
        }
    }

    private async Task<bool> ExecuteConsumerResult(CancellationToken stoppingToken)
    {
        ConsumeResult<string, string>? result = null;
        try
        {
            // TODO Метод намеренно синхронный, поэтому завернул в Task.Run
            result = await Task.Run(() => _consumer.Consume(stoppingToken), stoppingToken);
            var createdEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(result.Message.Value);
            if (createdEvent == null)
            {
                _logger.LogError("Ошибка createdEvent был null");
                _consumer.Commit(result);
                return true;
            }

            _logger.LogInformation("Заказ {Id} получен складом, начинаем сборку", createdEvent.OrderId);
            _consumer.Commit(result);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Ошибка во время десириализации JSON");
            _consumer.Commit(result);
        }
        catch (OperationCanceledException)
        {
            // Игнорируем
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка во время работы WarehouseWorker");
        }

        return false;
    }
}