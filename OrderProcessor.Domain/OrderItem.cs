namespace OrderProcessor.Domain;

public class OrderItem
{
    private int _count;
    private decimal _price;
    private Guid _guid;
    
    
    public Guid ProductId
    {
        get => _guid;
        private set
        {
            if (value == Guid.Empty)
                throw new OrderValidationException($"Неврзможно создать позицию с таким Guid = {value}");

            _guid = value;
        }
    }

    public int Count
    {
        get => _count;
        private set
        {
            if (value <= 0)
                throw new OrderValidationException("Невозможно создать позицию с кол-вом <= 0");

            _count = value;
        }
    }

    public decimal Price
    {
        get => _price;
        private set
        {
            if (value < 0)
                throw new OrderValidationException("Невозможно создать позицию с ценой <= -1");

            _price = value;
        }
    }
    
    public OrderItem(Guid productId, int count, decimal price)
    {
        ProductId = productId;
        Count = count;
        Price = price;
    }
}