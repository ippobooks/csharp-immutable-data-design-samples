var lines = new[] { new OrderLine("P001", 5) };

// 內容相同，但兩個 Order 使用不同的陣列物件。
var a = new Order("ORD-001", lines);
var b = new Order("ORD-001", lines.ToArray());

Console.WriteLine($"record 預設相等性: {a == b}"); // False
Console.WriteLine(
    $"逐一比較集合內容: {a.Lines.SequenceEqual(b.Lines)}"); // True

// 明確比較編號與明細，不會改變 record 原本的相等性。
bool sameData = a.Id == b.Id && a.Lines.SequenceEqual(b.Lines);
Console.WriteLine(sameData); // True
Console.WriteLine(a == b);  // 仍然是 False

public sealed record Order(
    string Id,
    IReadOnlyList<OrderLine> Lines
);

public sealed record OrderLine(
    string ProductId,
    int Quantity
);
