// Create the simplified order from Section 1.3.
var order = new Order(
    Id: "ORD-001",
    CustomerId: "CUST-42",
    Status: OrderStatus.Draft,
    Total: 1200m,
    CreatedAt: DateTimeOffset.UtcNow
);

// This assignment fails to compile after initialization.
// order.Status = OrderStatus.Paid;

// Use with to create a paid version, leaving order unchanged.
var paidOrder = order with
{
    Status = OrderStatus.Paid,
    PaidAt = DateTimeOffset.UtcNow
};

Console.WriteLine($"Original order state: {order.Status}");
Console.WriteLine($"New order state: {paidOrder.Status}");
Console.WriteLine($"Original order total: {order.Total}");
Console.WriteLine($"New order total: {paidOrder.Total}");
Console.WriteLine(
    $"Same object: {ReferenceEquals(order, paidOrder)}");

public enum OrderStatus
{
    Draft,
    Submitted,
    Paid,
    Shipped,
    Cancelled
}

public record Order(
    string Id,
    string CustomerId,
    OrderStatus Status,
    decimal Total,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt = null
);
