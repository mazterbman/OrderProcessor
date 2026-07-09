using Microsoft.AspNetCore.Mvc;
using OrderProcessor.Application;

namespace OrderProcessor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController :ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var id = await _orderService.CreateOrderAsync(request, cancellationToken);
        return Ok(id);
    }
}