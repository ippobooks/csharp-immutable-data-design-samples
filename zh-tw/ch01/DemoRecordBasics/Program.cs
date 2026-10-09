// 建立第 1.3 節的簡化訂單。
var order = new Order(
    Id: "ORD-001",
    CustomerId: "CUST-42",
    Status: OrderStatus.Draft,
    Total: 1200m,
    CreatedAt: DateTimeOffset.UtcNow
);

// 初始化完成後，以下指派會造成編譯錯誤。
// order.Status = OrderStatus.Paid;

// 用 with 建立付款後的新版本，原本的 order 維持原值。
var paidOrder = order with
{
    Status = OrderStatus.Paid,
    PaidAt = DateTimeOffset.UtcNow
};

Console.WriteLine($"原訂單狀態：{order.Status}");
Console.WriteLine($"新訂單狀態：{paidOrder.Status}");
Console.WriteLine($"原訂單總額：{order.Total}");
Console.WriteLine($"新訂單總額：{paidOrder.Total}");
Console.WriteLine(
    $"是否為同一個物件：{ReferenceEquals(order, paidOrder)}");

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
