namespace Acme.Common.Models;

public static class OrderStatus
{
    public const string Created = "Created";
    public const string Rejected = "Rejected";
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Sku { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = OrderStatus.Created;
    public string? RejectReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
