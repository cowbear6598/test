using System.Net.Http.Json;
using Acme.Common.Http;
using Acme.Common.Models;

namespace Acme.Orders.Api.Services;

// Go services (test2 repo) respond in snake_case, so these DTOs are deserialized with SnakeCaseOptions.
public record PriceQuote(string Sku, int CustomerId, int Qty, decimal UnitPrice, decimal DiscountRate, decimal Total);

public record ReserveResult(bool Ok, int Remaining, string? Error);

public class DownstreamClients(IConfiguration config)
{
    private string Url(string name) =>
        config[$"ServiceUrls:{name}"] ?? throw new InvalidOperationException($"ServiceUrls:{name} missing");

    public Task<Customer?> GetCustomerAsync(int id) =>
        LegacyHttpHelper.GetJsonAsync<Customer>($"{Url("CustomersApi")}/api/customers/{id}");

    public Task<PriceQuote?> GetQuoteAsync(string sku, int customerId, int qty) =>
        LegacyHttpHelper.GetJsonAsync<PriceQuote>(
            $"{Url("PricingService")}/quote?sku={Uri.EscapeDataString(sku)}&customer_id={customerId}&qty={qty}",
            LegacyHttpHelper.SnakeCaseOptions);

    public async Task<ReserveResult> ReserveStockAsync(string sku, int qty, string orderRef)
    {
        var resp = await LegacyHttpHelper.PostJsonAsync(
            $"{Url("InventoryService")}/reserve",
            new { Sku = sku, Qty = qty, OrderRef = orderRef },
            LegacyHttpHelper.SnakeCaseOptions);
        if (resp is null) return new ReserveResult(false, 0, "inventory_unreachable");

        var result = await resp.Content.ReadFromJsonAsync<ReserveResult>(LegacyHttpHelper.SnakeCaseOptions);
        return result ?? new ReserveResult(false, 0, "invalid_inventory_response");
    }

    public Task ReleaseStockAsync(string sku, int qty) =>
        LegacyHttpHelper.PostJsonAsync($"{Url("InventoryService")}/release",
            new { Sku = sku, Qty = qty }, LegacyHttpHelper.SnakeCaseOptions);

    // Fire-and-forget: if notification-service is down the order still succeeds (and nobody gets an email).
    public void NotifyOrderCreated(Order order, Customer customer)
    {
        _ = LegacyHttpHelper.PostJsonAsync($"{Url("NotificationService")}/notify", new
        {
            channel = "email",
            to = customer.Email,
            subject = $"Order #{order.Id} confirmed",
            body = $"{order.Quantity} x {order.Sku}, total {order.Total:0.00}",
        });
    }
}
