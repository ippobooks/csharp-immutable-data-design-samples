var lines = new[] { new OrderLine("P001", 5) };

// The two orders have the same contents but use different arrays.
var a = new Order("ORD-001", lines);
var b = new Order("ORD-001", lines.ToArray());

Console.WriteLine($"Default record equality: {a == b}"); // False
Console.WriteLine(
    $"Element comparison: {a.Lines.SequenceEqual(b.Lines)}"); // True

// Comparing ID and lines explicitly does not change record equality.
bool sameData = a.Id == b.Id && a.Lines.SequenceEqual(b.Lines);
Console.WriteLine(sameData); // True
Console.WriteLine(a == b);  // Still False

public sealed record Order(
    string Id,
    IReadOnlyList<OrderLine> Lines
);

public sealed record OrderLine(
    string ProductId,
    int Quantity
);
