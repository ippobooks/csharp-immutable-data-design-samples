using System;
using System.Collections.Generic;

Console.WriteLine("=== ミュータブルな状態の副作用 ===");

var order = new Order
{
    Id = "ORD-001",
    CustomerId = "CUST-42",
    Status = OrderStatus.Draft,
    Lines = new List<OrderLine>
    {
        new() { ItemName = "C# の書籍", UnitPrice = 500, Quantity = 2 }
    },
    Total = 1000,
    CreatedAt = DateTimeOffset.UtcNow
};

var service = new OrderService();

Console.WriteLine(
    $"支払い前の状態: {order.Status}, 支払い時刻: {order.PaidAt}");

// Pay は既存の order オブジェクトの状態を直接変更する。
service.Pay(order, DateTimeOffset.UtcNow);

Console.WriteLine(
    $"支払い後の状態: {order.Status}, 支払い時刻: {order.PaidAt}");

// 問題点 2: どの呼び出し元も自由に変更できる。
order.Status = OrderStatus.Cancelled;
order.PaidAt = null;
Console.WriteLine(
    $"外部から変更した後の状態: {order.Status}");

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
