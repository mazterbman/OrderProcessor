namespace OrderProcessor.Application;

public record CreateOrderRequest(List<OrderItemRequest> Items);
public record OrderItemRequest(Guid ProductId, int Count, decimal Price);