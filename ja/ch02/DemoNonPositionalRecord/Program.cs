Console.WriteLine("=== 非位置指定 record ===");

var line = new ValidatedOrderLine(
    productId: "P001",
    productName: "C# のイミュータブルデータ設計",
    quantity: 2,
    unitPrice: 580m);

Console.WriteLine($"商品: {line.ProductName}");
Console.WriteLine($"小計: {line.LineTotal:C}");

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
                "ProductId は空白にできません", nameof(productId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "数量は 0 より大きくなければなりません");

        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(
                nameof(unitPrice), "単価は負にできません");

        ProductId = productId;
        ProductName = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}
