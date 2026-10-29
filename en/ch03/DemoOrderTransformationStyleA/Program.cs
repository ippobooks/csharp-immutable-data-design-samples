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
                "Order lines have not been initialized",
                nameof(lines));
        }

        return new Order(id, customerId, lines, createdAt);
    }

    public Order Submit(DateTimeOffset now)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                "Only draft orders can be submitted");
        if (Lines.IsEmpty)
            throw new InvalidOperationException(
                "An order must have at least one line before submission");

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
                "Only submitted orders can be paid");

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
                "Only paid orders can be shipped");

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
                "Only draft, submitted, or paid orders can be cancelled");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(
                "A reason is required to cancel an order",
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
                "Introduction to Immutable Data Design in C#",
                1,
                500m));

        var draft = Order.CreateDraft(
            "ORD-001",
            "CUST-42",
            lines,
            now);

        Console.WriteLine($"Initial state: {draft.Status}");

        var submitted = draft.Submit(now);
        Console.WriteLine($"State after submission: {submitted.Status}");

        var paid = submitted.Pay(now.AddMinutes(5));
        Console.WriteLine($"State after payment: {paid.Status}");

        var shipped = paid.Ship(now.AddHours(2));
        Console.WriteLine($"State after shipping: {shipped.Status}");
    }
}
