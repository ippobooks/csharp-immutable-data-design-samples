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
        Console.WriteLine("=== init、required、with 式 ===");

        // 1. init 専用プロパティを使う。
        var basicReq = new OrderRequest
        {
            CustomerId = "CUST-42",
            ProductId = "P001"
        };
        Console.WriteLine(
            $"基本のリクエストを作成、商品: {basicReq.ProductId}");

        // 2. オブジェクト初期化子と required。
        var req = new CreateOrderRequest
        {
            CustomerId = "CUST-100",
            ProductId = "PROD-200",
            Note = "至急対応"
        };
        Console.WriteLine($"リクエストを作成、顧客: {req.CustomerId}");

        // 3. 単一階層の with 式。
        var originalLine = new OrderLine("P001", 5);
        var updatedLine = originalLine with { Quantity = 10 };

        Console.WriteLine($"元の数量: {originalLine.Quantity}");
        Console.WriteLine($"更新後の数量: {updatedLine.Quantity}");

        // 4. コンストラクターで値を検証。required は代入だけを求める。
        var item = new OrderItem("P001", 2);
        Console.WriteLine($"検証済みの数量: {item.Quantity}");

        try
        {
            _ = new OrderItem("P001", 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine(
                "コンストラクターが数量 0 の明細を拒否しました。");
        }
    }
}

public sealed record OrderItem
{
    public string ProductId { get; }
    public int Quantity { get; }

    public OrderItem(string productId, int quantity)
    {
        // 単一フィールドの形式を検証する。
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException(
                "ProductId は空白にできません", nameof(productId));

        // ビジネスルールを検証する。
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        ProductId = productId;
        Quantity = quantity;
    }
}
