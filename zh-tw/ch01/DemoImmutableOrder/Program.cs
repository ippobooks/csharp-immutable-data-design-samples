using System;
using System.Collections.Generic;

Console.WriteLine("=== 示範不可變狀態 (Immutable State) 與轉換 ===");

var lines = new List<OrderLine>
{
    new("C# 書籍", 500, 2)
};

var now = DateTimeOffset.UtcNow;
var order = OrderWorkflow.Create("CUST-42", lines, now);

Console.WriteLine(
    $"建立訂單 - 狀態: {order.Status}, 總額: {order.Total}");

// 提交訂單：原 order 不變，回傳新的 submittedOrder 物件
var submittedOrder = OrderWorkflow.Submit(
    order, now);

Console.WriteLine(
    $"提交訂單後 - 原訂單狀態: {order.Status}");
Console.WriteLine(
    $"提交訂單後 - 新訂單狀態: {submittedOrder.Status}");

// 付款：
var paidOrder = OrderWorkflow.Pay(
    submittedOrder, now.AddHours(2));
Console.WriteLine(
    $"付款後 - 新訂單狀態: {paidOrder.Status}, 付款時間: {paidOrder.PaidAt}");

// 測試：初始化完成後，以下程式碼會編譯錯誤：
// paidOrder.Status = OrderStatus.Cancelled;

// 出貨後，將需要的版本保留為狀態歷史。
var shippedOrder = OrderWorkflow.Ship(paidOrder, now.AddDays(1));
var auditTrail = new[]
{
    order, submittedOrder, paidOrder, shippedOrder
};

Console.WriteLine("=== 保留的訂單狀態歷史 ===");
foreach (var snapshot in auditTrail)
{
    Console.WriteLine(
        $"狀態：{snapshot.Status}，總額：{snapshot.Total}");
}

// 這裡只在記憶體中保留版本，沒有將歷史寫入資料庫。
// Lines 背後仍是 List；本範例建立訂單後不再修改它。

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
