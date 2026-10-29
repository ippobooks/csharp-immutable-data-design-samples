// 1.3 節の簡略化した注文を作る。
var order = new Order(
    Id: "ORD-001",
    CustomerId: "CUST-42",
    Status: OrderStatus.Draft,
    Total: 1200m,
    CreatedAt: DateTimeOffset.UtcNow
);

// 初期化後の次の代入はコンパイルエラーになる。
// order.Status = OrderStatus.Paid;

// with で支払い後の新バージョンを作り、元の order は変更しない。
var paidOrder = order with
{
    Status = OrderStatus.Paid,
    PaidAt = DateTimeOffset.UtcNow
};

Console.WriteLine($"元の注文の状態: {order.Status}");
Console.WriteLine($"新しい注文の状態: {paidOrder.Status}");
Console.WriteLine($"元の注文の合計: {order.Total}");
Console.WriteLine($"新しい注文の合計: {paidOrder.Total}");
Console.WriteLine(
    $"同じオブジェクトか: {ReferenceEquals(order, paidOrder)}");

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
