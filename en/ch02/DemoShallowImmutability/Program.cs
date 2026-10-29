using System;
using System.Collections.Generic;

public record OrderLine(string ProductId, int Quantity);

public record Order(
    string Id,
    List<OrderLine> Lines // List<T> is mutable.
);

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== The shallow immutability trap ===");

        var order = new Order("ORD-001", new List<OrderLine>
        {
            new("P001", 2)
        });

        Console.WriteLine(
            $"Line count before change: {order.Lines.Count}");

        // The Lines reference cannot be replaced.
        // order.Lines = new List<OrderLine>(); // Compile error!

        // The property cannot be reassigned, but its List is mutable.
        order.Lines.Add(new OrderLine("P002", 5));

        Console.WriteLine(
            $"Line count after change: {order.Lines.Count}"); // Now 2!
    }
}
