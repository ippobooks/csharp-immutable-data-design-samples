using System;
using System.Collections.Generic;

Console.WriteLine("=== Side effects of mutable state ===");

var order = new Order
{
    Id = "ORD-001",
    CustomerId = "CUST-42",
    Status = OrderStatus.Draft,
    Lines = new List<OrderLine>
    {
        new() { ItemName = "C# book", UnitPrice = 500, Quantity = 2 }
    },
    Total = 1000,
    CreatedAt = DateTimeOffset.UtcNow
};

var service = new OrderService();

Console.WriteLine(
    $"Before payment - State: {order.Status}, Paid at: {order.PaidAt}");

// Pay modifies the state of the existing order object.
service.Pay(order, DateTimeOffset.UtcNow);

Console.WriteLine(
    $"After payment - State: {order.Status}, Paid at: {order.PaidAt}");

// Risk 2: any caller can modify the object freely.
order.Status = OrderStatus.Cancelled;
order.PaidAt = null;
Console.WriteLine(
    $"State after an external change: {order.Status}");

public enum OrderStatus
{
    Draft,
    Submitted,
    Paid,
    Shipped,
    Cancelled
}

public class OrderLine
{
    public string ItemName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class Order
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public List<OrderLine> Lines { get; set; } = [];
    public decimal Total { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public DateTimeOffset? ShippedAt { get; set; }
    public string? CancelReason { get; set; }
}

public class OrderService
{
    public void Submit(Order order, DateTimeOffset now)
    {
        order.Status = OrderStatus.Submitted;
        order.SubmittedAt = now;
    }

    public void Pay(Order order, DateTimeOffset now)
    {
        order.Status = OrderStatus.Paid;
        order.PaidAt = now;
    }

    public void Ship(Order order, DateTimeOffset now)
    {
        order.Status = OrderStatus.Shipped;
        order.ShippedAt = now;
    }

    public void Cancel(Order order, string reason)
    {
        order.Status = OrderStatus.Cancelled;
        order.CancelReason = reason;
        order.SubmittedAt = null;
        order.PaidAt = null;
    }
}
