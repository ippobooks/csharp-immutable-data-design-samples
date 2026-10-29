using System;
using System.Collections.Generic;

Console.WriteLine("=== イミュータブルな状態と変換 ===");

var lines = new List<OrderLine>
{
    new("C# の書籍", 500, 2)
};

var now = DateTimeOffset.UtcNow;
var order = OrderWorkflow.Create("CUST-42", lines, now);

Console.WriteLine(
    $"注文作成 - 状態: {order.Status}, 合計: {order.Total}");

// 確定: order を変更せず、新しい submittedOrder を返す。
var submittedOrder = OrderWorkflow.Submit(
    order, now);

Console.WriteLine(
    $"確定後 - 元の注文の状態: {order.Status}");
Console.WriteLine(
    $"確定後 - 新しい注文の状態: {submittedOrder.Status}");

// 支払い:
var paidOrder = OrderWorkflow.Pay(
    submittedOrder, now.AddHours(2));
Console.WriteLine(
    $"支払い後 - 新しい状態: {paidOrder.Status}, " +
    $"支払い時刻: {paidOrder.PaidAt}");

// 確認: 初期化後の次のコードはコンパイルエラーになる。
// paidOrder.Status = OrderStatus.Cancelled;

// 発送後、必要なバージョンを状態の履歴として保持する。
var shippedOrder = OrderWorkflow.Ship(paidOrder, now.AddDays(1));
var auditTrail = new[]
{
    order, submittedOrder, paidOrder, shippedOrder
};

Console.WriteLine("=== 保持した注文状態の履歴 ===");
foreach (var snapshot in auditTrail)
{
    Console.WriteLine(
        $"状態: {snapshot.Status}, 合計: {snapshot.Total}");
}

// バージョンはメモリ内だけに保持し、履歴はデータベースに書き込まない。
// Lines の実体は List だが、この例では注文作成後に変更しない。

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
