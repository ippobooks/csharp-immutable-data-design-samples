using System;

public class OrderRequest
{
    public string CustomerId { get; init; } = "";
    public string ProductId { get; init; } = "";
}

public class CreateOrderRequest
{
    public required string CustomerId { get; init; }
    public required string ProductId { get; init; }
    public string? Note { get; init; }
}

public record OrderLine(string ProductId, int Quantity);

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== init, required, and with expressions ===");

        // 1. Use init-only properties.
        var basicReq = new OrderRequest
        {
            CustomerId = "CUST-42",
            ProductId = "P001"
        };
        Console.WriteLine(
            $"Basic request created, product: {basicReq.ProductId}");

        // 2. Object initializers and required.
        var req = new CreateOrderRequest
        {
            CustomerId = "CUST-100",
            ProductId = "PROD-200",
            Note = "Urgent processing"
        };
        Console.WriteLine($"Request created, customer: {req.CustomerId}");

        // 3. A single-level with expression.
        var originalLine = new OrderLine("P001", 5);
        var updatedLine = originalLine with { Quantity = 10 };

        Console.WriteLine($"Original quantity: {originalLine.Quantity}");
        Console.WriteLine($"Updated quantity: {updatedLine.Quantity}");

        // 4. Constructors validate values; required demands assignment.
        var item = new OrderItem("P001", 2);
        Console.WriteLine($"Validated quantity: {item.Quantity}");

        try
        {
            _ = new OrderItem("P001", 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine(
                "The constructor rejected a line with zero quantity.");
        }
    }
}

public sealed record OrderItem
{
    public string ProductId { get; }
    public int Quantity { get; }

    public OrderItem(string productId, int quantity)
    {
        // Validate the format of one field.
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException(
                "ProductId must not be blank", nameof(productId));

        // Validate business rules.
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        ProductId = productId;
        Quantity = quantity;
    }
}
