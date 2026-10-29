Console.WriteLine("=== IReadOnlyList<T> is only a read-only view ===");

var lines = new List<OrderLine>();
var order = new Order("ORD-001", lines);

Console.WriteLine(
    $"Before external change: order.Lines.Count = {order.Lines.Count}");

// order.Lines.Add(new OrderLine("P001", 5)); // Compile error!

// External code still holds the original List<T> and can modify it.
lines.Add(new OrderLine("P001", 5));

Console.WriteLine(
    $"After external change: order.Lines.Count = {order.Lines.Count}");

public record OrderLine(string ProductId, int Quantity);

public record Order(
    string Id,
    IReadOnlyList<OrderLine> Lines
);
