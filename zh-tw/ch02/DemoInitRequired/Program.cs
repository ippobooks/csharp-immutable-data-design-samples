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
        Console.WriteLine("=== 示範 init, required 與 with 運算式 ===");

        // 1. 單純使用 init-only 屬性
        var basicReq = new OrderRequest
        {
            CustomerId = "CUST-42",
            ProductId = "P001"
        };
        Console.WriteLine($"一般請求建立成功，商品: {basicReq.ProductId}");

        // 2. 物件初始設定式與 required
        var req = new CreateOrderRequest
        {
            CustomerId = "CUST-100",
            ProductId = "PROD-200",
            Note = "急件處理"
        };
        Console.WriteLine($"建立請求成功，客戶: {req.CustomerId}");

        // 3. 單層 with 運算式
        var originalLine = new OrderLine("P001", 5);
        var updatedLine = originalLine with { Quantity = 10 };

        Console.WriteLine($"原始項目數量: {originalLine.Quantity}");
        Console.WriteLine($"更新後項目數量: {updatedLine.Quantity}");

        // 4. 建構式驗證值的內容；required 只要求提供必要成員。
        var item = new OrderItem("P001", 2);
        Console.WriteLine($"通過驗證的項目數量: {item.Quantity}");

        try
        {
            _ = new OrderItem("P001", 0);
        }
        catch (ArgumentOutOfRangeException)
        {
            Console.WriteLine("建構式拒絕了數量為 0 的項目。");
        }
    }
}

public sealed record OrderItem
{
    public string ProductId { get; }
    public int Quantity { get; }

    public OrderItem(string productId, int quantity)
    {
        // 驗證單一欄位的格式。
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException(
                "ProductId 不能為空", nameof(productId));

        // 驗證業務規則。
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity));

        ProductId = productId;
        Quantity = quantity;
    }
}
