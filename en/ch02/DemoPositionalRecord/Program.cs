using System;

// Define a positional record.
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
        Console.WriteLine("=== Positional records ===");

        // 1. Initialization.
        var line = new OrderLine(
            "P001", "Immutable Data Design in C#", 2, 580m);
        Console.WriteLine($"Original object: {line}");

        // 2. Deconstruction.
        var (productId, productName, quantity, unitPrice) = line;
        Console.WriteLine(
            $"Deconstructed: {productName} ({productId}) " +
            $"x {quantity} = {unitPrice * quantity:C}");
    }
}
