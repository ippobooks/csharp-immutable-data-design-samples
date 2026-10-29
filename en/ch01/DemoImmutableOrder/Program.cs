using System;
using System.Collections.Generic;

Console.WriteLine("=== Immutable state and transformations ===");

var lines = new List<OrderLine>
{
    new("C# book", 500, 2)
};

var now = DateTimeOffset.UtcNow;
var order = OrderWorkflow.Create("CUST-42", lines, now);

Console.WriteLine(
    $"Order created - State: {order.Status}, Total: {order.Total}");

// Submit: return a new submittedOrder without changing order.
var submittedOrder = OrderWorkflow.Submit(
    order, now);

Console.WriteLine(
    $"After submission - Original state: {order.Status}");
Console.WriteLine(
    $"After submission - New state: {submittedOrder.Status}");

// Payment:
var paidOrder = OrderWorkflow.Pay(
    submittedOrder, now.AddHours(2));
Console.WriteLine(
    $"After payment - New state: {paidOrder.Status}, " +
    $"Paid at: {paidOrder.PaidAt}");

// Test: the following code fails to compile after initialization.
// paidOrder.Status = OrderStatus.Cancelled;

// After shipping, keep the required versions as state history.
var shippedOrder = OrderWorkflow.Ship(paidOrder, now.AddDays(1));
var auditTrail = new[]
{
    order, submittedOrder, paidOrder, shippedOrder
};

Console.WriteLine("=== Retained order state history ===");
foreach (var snapshot in auditTrail)
{
    Console.WriteLine(
        $"State: {snapshot.Status}, Total: {snapshot.Total}");
}

// Versions stay in memory; history is not written to a database.
// Lines still wraps a List; this example leaves it unchanged.

public enum OrderStatus
{
    Draft,
    Submitted,
    Paid,
    Shipped,
    Cancelled
}

public record OrderLine(
    string ItemName,
    decimal UnitPrice,
    int Quantity
);

public sealed record Order(
    string Id,
    string CustomerId,
    OrderStatus Status,
    IReadOnlyList<OrderLine> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SubmittedAt = null,
    DateTimeOffset? PaidAt = null,
    DateTimeOffset? ShippedAt = null,
    string? CancelReason = null
)
{
    public decimal Total => Lines.Sum(
        line => line.UnitPrice * line.Quantity);
}

public static class OrderWorkflow
{
    public static Order Create(
        string customerId,
        IReadOnlyList<OrderLine> lines,
        DateTimeOffset now)
    {
        return new Order(
            Id: Guid.NewGuid().ToString(),
            CustomerId: customerId,
            Status: OrderStatus.Draft,
            Lines: lines,
            CreatedAt: now
        );
    }

    public static Order Submit(Order order, DateTimeOffset now)
    {
        return order with
        {
            Status = OrderStatus.Submitted,
            SubmittedAt = now
        };
    }

    public static Order Pay(Order order, DateTimeOffset now)
    {
        return order with
        {
            Status = OrderStatus.Paid,
            PaidAt = now
        };
    }

    public static Order Ship(Order order, DateTimeOffset now)
    {
        return order with
        {
            Status = OrderStatus.Shipped,
            ShippedAt = now
        };
    }

    public static Order Cancel(Order order, string reason)
    {
        return order with
        {
            Status = OrderStatus.Cancelled,
            CancelReason = reason
        };
    }
}
