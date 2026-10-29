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
                "Order lines have not been initialized",
                nameof(lines));
        }

        return new Order(id, customerId, lines, createdAt);
    }

    // Calculate the final total.
    public decimal FinalTotal => (Total * DiscountRate) + ShippingFee;

    public Order ApplyDiscount(decimal rate)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                "Only draft orders can have a discount applied");
        if (rate is < 0m or > 1m)
            throw new ArgumentOutOfRangeException(nameof(rate));

        return this with { DiscountRate = rate };
    }

    public Order ApplyShippingFee(decimal fee)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException(
                "Only draft orders can have shipping fees calculated");
        if (fee < 0m)
            throw new ArgumentOutOfRangeException(nameof(fee));

        return this with { ShippingFee = fee };
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

        // Transformation pipeline: method chaining.
        var processedOrder = draft
            .ApplyDiscount(0.9m)       // 10% discount
            .ApplyShippingFee(60m)     // Shipping fee: 60
            .Submit(now)               // Submit
            .Pay(now.AddMinutes(5));   // Pay

        Console.WriteLine($"Original order total: {draft.FinalTotal}");
        Console.WriteLine(
            $"Processed order state: {processedOrder.Status}");
        Console.WriteLine(
            "Discount rate after processing: " +
            $"{processedOrder.DiscountRate}");
        Console.WriteLine(
            "Shipping fee after processing: " +
            $"{processedOrder.ShippingFee}");
        Console.WriteLine(
            $"Final total after processing: {processedOrder.FinalTotal}");
    }
}
