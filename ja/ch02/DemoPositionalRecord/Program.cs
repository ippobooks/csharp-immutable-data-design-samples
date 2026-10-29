using System;

// 位置指定 record を定義する。
public record OrderLine(
    string ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== 位置指定 record ===");

        // 1. 初期化。
        var line = new OrderLine(
            "P001", "C# のイミュータブルデータ設計", 2, 580m);
        Console.WriteLine($"元のオブジェクト: {line}");

        // 2. 分解。
        var (productId, productName, quantity, unitPrice) = line;
        Console.WriteLine(
            $"分解結果: {productName} ({productId}) " +
            $"x {quantity} = {unitPrice * quantity:C}");
    }
}
