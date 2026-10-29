using System;
using System.Collections.Generic;
using System.Collections.Immutable;

public record OrderLine(string ProductId, int Quantity);

public sealed class SafeOrder
{
    public string Id { get; }
    public IReadOnlyList<OrderLine> Lines { get; }

    public SafeOrder(string id, IEnumerable<OrderLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        Id = id;
        // Copy, then wrap to prevent casts back to a mutable array.
        Lines = Array.AsReadOnly(lines.ToArray());
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== Read-only and immutable collections ===");

        // 1. Defensive copying.
        var sourceList = new List<OrderLine> { new("P001", 1) };
        var order = new SafeOrder("ORD-001", sourceList);
        sourceList.Add(new("P002", 5)); // External change
        Console.WriteLine(
            $"Line count in SafeOrder: {order.Lines.Count}"); // 1

        // 2. Immutable updates return a new collection version.
        var productIds = ImmutableList.Create("P001", "P002");
        var updatedProductIds = productIds.Add("P003");
        Console.WriteLine(
            $"Original ImmutableList count: {productIds.Count}");
        Console.WriteLine(
            $"Updated ImmutableList count: {updatedProductIds.Count}");

        // 3. The ImmutableArray default trap.
        ImmutableArray<OrderLine> items = default;
        Console.WriteLine($"items.IsDefault = {items.IsDefault}"); // True
        try
        {
            _ = items.Length;
        }
        catch (NullReferenceException)
        {
            Console.WriteLine(
                "Caught NullReferenceException!" +
                "An uninitialized ImmutableArray has " +
                "a null backing array.");
        }

        var safeItems = ImmutableArray<OrderLine>.Empty;
        Console.WriteLine(safeItems.IsDefault); // False
        Console.WriteLine(safeItems.Length);    // 0
    }
}
