using System;
using System.Collections.Immutable;

namespace DemoOrderTransformationPipeline;

public enum OrderStatus
{
    Draft,
    Submitted,
    Paid,
    Shipped,
    Cancelled
}

public sealed record OrderLine(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice)
{
    public decimal Amount => Quantity * UnitPrice;
}

public sealed record Order
{
    public string Id { get; private init; }
    public string CustomerId { get; private init; }
    public OrderStatus Status { get; private init; }
    public ImmutableArray<OrderLine> Lines { get; private init; }
    public decimal Total { get; private init; }
    public decimal DiscountRate { get; private init; }
    public decimal ShippingFee { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? SubmittedAt { get; private init; }
    public DateTimeOffset? PaidAt { get; private init; }

    private Order(
        string id,
        string customerId,
        ImmutableArray<OrderLine> lines,
        DateTimeOffset createdAt)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        Lines = lines;
        Total = lines.Sum(line => line.Amount);
        DiscountRate = 1m;
        ShippingFee = 0m;
        CreatedAt = createdAt;
    }

    public static Order CreateDraft(
        string id,
        string customerId,
        ImmutableArray<OrderLine> lines,
        DateTimeOffset createdAt)
    {
        if (lines.IsDefault)
        {
            throw new ArgumentException(
                "注文明細が初期化されていません",
                nameof(lines));
        }

        return new Order(id, customerId, lines, createdAt);
    }

    // 最終金額を計算する。
    public decimal FinalTotal => (Total * DiscountRate) + ShippingFee;

    public Order ApplyDiscount(decimal rate)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                "割引を適用できるのは下書きの注文だけです");
        if (rate is < 0m or > 1m)
            throw new ArgumentOutOfRangeException(nameof(rate));

        return this with { DiscountRate = rate };
    }

    public Order ApplyShippingFee(decimal fee)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                "送料を計算できるのは下書きの注文だけです");
        if (fee < 0m)
            throw new ArgumentOutOfRangeException(nameof(fee));

        return this with { ShippingFee = fee };
    }

    public Order Submit(DateTimeOffset now)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                "確定できるのは下書きの注文だけです");
        if (Lines.IsEmpty)
            throw new InvalidOperationException(
                "確定するには少なくとも一つの注文明細が必要です");

        return this with
        {
            Status = OrderStatus.Submitted,
            SubmittedAt = now
        };
    }

    public Order Pay(DateTimeOffset now)
    {
        if (Status != OrderStatus.Submitted)
            throw new InvalidOperationException(
                "支払えるのは確定済みの注文だけです");

        return this with
        {
            Status = OrderStatus.Paid,
            PaidAt = now
        };
    }
}

public class Program
{
    public static void Main()
    {
        var timeProvider = TimeProvider.System;
        var now = timeProvider.GetUtcNow();

        var lines = ImmutableArray.Create(
            new OrderLine(
                "P001",
                "C# のイミュータブルデータ設計入門",
                1,
                500m));

        var draft = Order.CreateDraft(
            "ORD-001",
            "CUST-42",
            lines,
            now);

        // 変換パイプライン: メソッドチェーン。
        var processedOrder = draft
            .ApplyDiscount(0.9m)       // 10% 割引
            .ApplyShippingFee(60m)     // 送料: 60
            .Submit(now)               // 確定
            .Pay(now.AddMinutes(5));   // 支払い

        Console.WriteLine($"元の注文の合計金額: {draft.FinalTotal}");
        Console.WriteLine($"処理後の注文状態: {processedOrder.Status}");
        Console.WriteLine(
            $"処理後の割引率: {processedOrder.DiscountRate}");
        Console.WriteLine($"処理後の送料: {processedOrder.ShippingFee}");
        Console.WriteLine(
            $"処理後の最終金額: {processedOrder.FinalTotal}");
    }
}
