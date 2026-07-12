namespace OrderProcessor.Domain;

public class Order
{
    private readonly List<OrderItem> _orderItems;
    
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderItem> Items => _orderItems;

    public Order(List<OrderItem> items)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        _orderItems = new List<OrderItem>(items);
    }
    
    private Order(){}

    public void AddItem(Guid productId, int count, decimal price)
        => AddItem(new OrderItem(productId, count, price));

    public void AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            throw new OrderValidationException("Невозможно изменить заказ, что уже не в статусе черновика.");
        
        var existing = _orderItems.FirstOrDefault(i => i.ProductId == item.ProductId);
        if (existing == null || !decimal.Equals(existing.Price, item.Price))
        {
            _orderItems.Add(item);
            return;
        }

        _orderItems.Remove(existing);
        _orderItems.Add(new OrderItem(existing.ProductId, existing.Count + item.Count, existing.Price));
    }
    
    public void StartProcessing()
    {
        if (Status != OrderStatus.Pending)
            throw new OrderValidationException($"Нельзя начать обработку заказа в статусе {Status}.");
        
        Status = OrderStatus.Processing;
    }

    public void Complete() => Status = OrderStatus.Completed;
    public void Fail() => Status = OrderStatus.Failed;
}