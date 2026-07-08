using OrderProcessor.Domain;

namespace OrderProcessor.Application;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public OrderService(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Guid> CreateOrderAsync(CreateOrderRequest orderRequest, CancellationToken cancellationToken = default)
    {
        var order = new Order(orderRequest.Items.Select(order => new OrderItem(order.ProductId, order.Count, order.Price)).ToList());
        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}