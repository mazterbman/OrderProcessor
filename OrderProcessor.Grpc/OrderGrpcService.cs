using Grpc.Core;
using OrderProcessor.Application;
using ProtoOrderStatus = OrderProcessor.Grpc.OrderStatus;

namespace OrderProcessor.Grpc;

public class OrderGrpcService : OrderService.OrderServiceBase
{
    private readonly IOrderRepository _orderRepository;

    public OrderGrpcService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public override async Task<GetOrderByIdResponse> GetOrder(GetOrderByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "order_id не является корректным Guid"));
        }
        
        var order = await _orderRepository.GetByIdAsync(orderId, context.CancellationToken);
        
        if (order == null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Заказа с таким order_id нет"));
        }
        
        return new GetOrderByIdResponse
        {
            OrderId = orderId.ToString(),
            Status = MapStatus(order.Status)
        };
    }
    
    private static OrderStatus MapStatus(Domain.OrderStatus orderStatus)
    {
        // switch по доменному enum → proto enum
        return orderStatus switch
        {
            Domain.OrderStatus.Pending => ProtoOrderStatus.Pending,
            Domain.OrderStatus.Completed => ProtoOrderStatus.Completed,
            Domain.OrderStatus.Failed => ProtoOrderStatus.Failed,
            Domain.OrderStatus.Processing => ProtoOrderStatus.Processing,
            _ => ProtoOrderStatus.Unspecified
        };
    }
}