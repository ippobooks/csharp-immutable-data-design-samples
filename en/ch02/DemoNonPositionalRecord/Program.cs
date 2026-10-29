Console.WriteLine("=== Non-positional records ===");

var line = new ValidatedOrderLine(
    productId: "P001",
    productName: "Immutable Data Design in C#",
    quantity: 2,
    unitPrice: 580m);

Console.WriteLine($"Product: {line.ProductName}");
Console.WriteLine($"Line total: {line.LineTotal:C}");

public sealed record ValidatedOrderLine
{
    public string ProductId { get; private init; }
    public string ProductName { get; private init; }
    public int Quantity { get; private init; }
    public decimal UnitPrice { get; private init; }
    public decimal LineTotal => Quantity * UnitPrice;

    public ValidatedOrderLine(
        string productId,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException(
                "ProductId must not be blank", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity), "Quantity must be greater than 0");

        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(unitPrice), "Unit price must not be negative");

        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
