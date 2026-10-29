var lines = new[] { new OrderLine("P001", 5) };

// 内容は同じだが、二つの Order は異なる配列を使う。
var a = new Order("ORD-001", lines);
var b = new Order("ORD-001", lines.ToArray());

Console.WriteLine($"record の既定の等価性: {a == b}"); // False
Console.WriteLine(
    $"要素ごとの比較: {a.Lines.SequenceEqual(b.Lines)}"); // True

// 番号と明細を明示的に比較しても、record の等価性は変わらない。
bool sameData = a.Id == b.Id && a.Lines.SequenceEqual(b.Lines);
Console.WriteLine(sameData); // True
Console.WriteLine(a == b);  // 引き続き False

public sealed record Order(
    string Id,
    IReadOnlyList<OrderLine> Lines
);

public sealed record OrderLine(
    string ProductId,
    int Quantity
);
