using System;

// Regular class (reference equality).
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

// Record type (value equality).
public record OrderLineRecord(string ProductId, int Quantity);

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Value equality ===");

        // 1. Class equality: only the same reference is equal.
        var c1 = new OrderLineClass("P001", 5);
        var c2 = new OrderLineClass("P001", 5);
        Console.WriteLine($"class (c1 == c2): {c1 == c2}"); // False
        Console.WriteLine($"class Equals: {c1.Equals(c2)}"); // False

        // 2. Record equality: equal values compare equal.
        var r1 = new OrderLineRecord("P001", 5);
        var r2 = new OrderLineRecord("P001", 5);
        Console.WriteLine($"record (r1 == r2): {r1 == r2}"); // True
        Console.WriteLine($"record Equals: {r1.Equals(r2)}"); // True
    }
}
