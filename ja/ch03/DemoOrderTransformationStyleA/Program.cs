using System;
using System.Collections.Immutable;

namespace DemoOrderTransformationStyleA;

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
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? SubmittedAt { get; private init; }
    public DateTimeOffset? PaidAt { get; private init; }
    public DateTimeOffset? ShippedAt { get; private init; }
    public string? CancelReason { get; private init; }

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

    public Order Ship(DateTimeOffset now)
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException(
                "発送できるのは支払い済みの注文だけです");

        return this with
        {
            Status = OrderStatus.Shipped,
            ShippedAt = now
        };
    }

    public Order Cancel(string reason)
    {
        if (Status is not (
            OrderStatus.Draft or
            OrderStatus.Submitted or
            OrderStatus.Paid))
            throw new InvalidOperationException(
                "キャンセルできるのは下書き・確定済み・" +
                "支払い済みの注文だけです");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(
                "注文のキャンセルには理由が必要です",
                nameof(reason));

        return this with
        {
            Status = OrderStatus.Cancelled,
            CancelReason = reason
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

        Console.WriteLine($"初期状態: {draft.Status}");

        var submitted = draft.Submit(now);
        Console.WriteLine($"確定後の状態: {submitted.Status}");

        var paid = submitted.Pay(now.AddMinutes(5));
        Console.WriteLine($"支払い後の状態: {paid.Status}");

        var shipped = paid.Ship(now.AddHours(2));
        Console.WriteLine($"発送後の状態: {shipped.Status}");
    }
}
