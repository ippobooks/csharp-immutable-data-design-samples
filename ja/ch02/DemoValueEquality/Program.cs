using System;

// 通常のクラス（参照の等価性）。
public class OrderLineClass
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }

    public OrderLineClass(string productId, int quantity)
    {
        ProductId = productId;
        Quantity = quantity;
    }
}

// record 型（値の等価性）。
public record OrderLineRecord(string ProductId, int Quantity);

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== 値の等価性 ===");

        // 1. class の比較: 同じ参照だけが等しい。
        var c1 = new OrderLineClass("P001", 5);
        var c2 = new OrderLineClass("P001", 5);
        Console.WriteLine($"class (c1 == c2): {c1 == c2}"); // False
        Console.WriteLine($"class Equals: {c1.Equals(c2)}"); // False

        // 2. record の比較: 値が同じなら等しい。
        var r1 = new OrderLineRecord("P001", 5);
        var r2 = new OrderLineRecord("P001", 5);
        Console.WriteLine($"record (r1 == r2): {r1 == r2}"); // True
        Console.WriteLine($"record Equals: {r1.Equals(r2)}"); // True
    }
}
