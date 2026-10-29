using System;
using System.Collections.Immutable;

namespace DemoOrderTransformationStyleB;

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

public sealed record Order(
    string Id,
    string CustomerId,
    OrderStatus Status,
    ImmutableArray<OrderLine> Lines,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SubmittedAt = null,
    DateTimeOffset? PaidAt = null,
    DateTimeOffset? ShippedAt = null,
    string? CancelReason = null)
{
    public decimal Total => Lines.Sum(
        line => line.UnitPrice * line.Quantity);
}

public sealed record OrderTransitionResult
{
    public bool IsSuccess { get; }
    public Order? Order { get; }
    public string? ErrorMessage { get; }

    private OrderTransitionResult(
        bool isSuccess,
        Order? order,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        Order = order;
        ErrorMessage = errorMessage;
    }

    public static OrderTransitionResult Success(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        return new(true, order, null);
    }

    public static OrderTransitionResult Fail(string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        return new(false, null, message);
    }
}

public static class OrderWorkflow
{
    public static OrderTransitionResult Submit(
        Order order, DateTimeOffset now)
    {
        if (order.Status != OrderStatus.Draft)
        {
            return OrderTransitionResult.Fail(
                "確定できるのは下書きだけです。" +
                $"現在の状態: {order.Status}");
        }

        if (order.Lines.IsDefault)
        {
            return OrderTransitionResult.Fail(
                "注文明細が初期化されていません");
        }

        if (order.Lines.IsEmpty)
        {
            return OrderTransitionResult.Fail(
                "確定するには少なくとも一つの注文明細が必要です");
        }

        var submitted = order with
        {
            Status = OrderStatus.Submitted,
            SubmittedAt = now
        };

        return OrderTransitionResult.Success(submitted);
    }

    public static OrderTransitionResult Pay(
        Order order, DateTimeOffset now)
    {
        if (order.Status != OrderStatus.Submitted)
        {
            return OrderTransitionResult.Fail(
                "支払えるのは確定済み注文だけです。" +
                $"現在の状態: {order.Status}");
        }

        var paid = order with
        {
            Status = OrderStatus.Paid,
            PaidAt = now
        };

        return OrderTransitionResult.Success(paid);
    }

    public static OrderTransitionResult Ship(
        Order order, DateTimeOffset now)
    {
        if (order.Status != OrderStatus.Paid)
        {
            return OrderTransitionResult.Fail(
                "発送できるのは支払い済み注文だけです。" +
                $"現在の状態: {order.Status}");
        }

        var shipped = order with
        {
            Status = OrderStatus.Shipped,
            ShippedAt = now
        };

        return OrderTransitionResult.Success(shipped);
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

        var draft = new Order(
            "ORD-001",
            "CUST-42",
            OrderStatus.Draft,
            lines,
            now);

        Console.WriteLine($"初期状態: {draft.Status}");

        var submitResult = OrderWorkflow.Submit(draft, now);
        if (!submitResult.IsSuccess)
        {
            Console.WriteLine($"確定に失敗: {submitResult.ErrorMessage}");
            return;
        }

        var submitted = submitResult.Order!;
        Console.WriteLine($"確定後の状態: {submitted.Status}");

        var payResult = OrderWorkflow.Pay(submitted, now.AddMinutes(5));
        if (!payResult.IsSuccess)
        {
            Console.WriteLine($"支払いに失敗: {payResult.ErrorMessage}");
            return;
        }

        var paid = payResult.Order!;
        Console.WriteLine($"支払い後の状態: {paid.Status}");
    }
}
