Console.WriteLine("=== IReadOnlyList<T> は読み取り専用ビュー ===");

var lines = new List<OrderLine>();
var order = new Order("ORD-001", lines);

Console.WriteLine($"外部変更前: order.Lines.Count = {order.Lines.Count}");

// order.Lines.Add(new OrderLine("P001", 5)); // コンパイルエラー!

// 外部に元の List<T> への参照が残るため、内容を変更できる。
lines.Add(new OrderLine("P001", 5));

Console.WriteLine($"外部変更後: order.Lines.Count = {order.Lines.Count}");

public record OrderLine(string ProductId, int Quantity);

public record Order(
    string Id,
    IReadOnlyList<OrderLine> Lines
);
