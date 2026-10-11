using Acme.Common.Models;
using Acme.Orders.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Acme.Orders.Api.Controllers;

public record CreateOrderRequest(int CustomerId, string Sku, int Quantity);

[ApiController]
[Route("api/orders")]
public class OrdersController(OrderRepository repo, DownstreamClients downstream) : ControllerBase
{
    [HttpGet]
    public IEnumerable<Order> List([FromQuery] int? customerId) =>
        customerId is null ? repo.All() : repo.All().Where(o => o.CustomerId == customerId);

    [HttpGet("{id:int}")]
    public ActionResult<Order> Get(int id) => repo.Get(id) is { } o ? o : NotFound();

    // Order flow: customers-api -> pricing-service -> inventory-service -> notification-service
    [HttpPost]
    public async Task<ActionResult<Order>> Create(CreateOrderRequest req)
    {
        if (req.Quantity <= 0) return BadRequest(new { error = "quantity must be positive" });

        var customer = await downstream.GetCustomerAsync(req.CustomerId);
        if (customer is null) return BadRequest(new { error = $"customer {req.CustomerId} not found" });

        var quote = await downstream.GetQuoteAsync(req.Sku, req.CustomerId, req.Quantity);
        if (quote is null) return StatusCode(502, new { error = $"could not price sku {req.Sku}" });

        var order = new Order
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            Sku = req.Sku,
            Quantity = req.Quantity,
            UnitPrice = quote.UnitPrice,
            DiscountRate = quote.DiscountRate,
            Total = quote.Total,
            CreatedAt = DateTime.UtcNow,
        };

        var orderRef = "ORD-" + Guid.NewGuid().ToString("N")[..8];
        var reserve = await downstream.ReserveStockAsync(req.Sku, req.Quantity, orderRef);
        if (!reserve.Ok)
        {
            // Legacy behaviour: rejected orders are still persisted (finance wanted them for reporting).
            order.Status = OrderStatus.Rejected;
            order.RejectReason = reserve.Error;
            repo.Add(order);
            return Conflict(order);
        }

        repo.Add(order);
        downstream.NotifyOrderCreated(order, customer);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, order);
    }
}
