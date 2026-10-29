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
        // コピーしてからラップし、配列へのキャストによる変更を防ぐ。
        Lines = Array.AsReadOnly(lines.ToArray());
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine(
            "=== 読み取り専用とイミュータブルなコレクション ===");

        // 1. 防御的コピー。
        var sourceList = new List<OrderLine> { new("P001", 1) };
        var order = new SafeOrder("ORD-001", sourceList);
        sourceList.Add(new("P002", 5)); // 外部から変更
        Console.WriteLine(
            $"SafeOrder の明細数: {order.Lines.Count}"); // 1

        // 2. イミュータブルな更新はコレクションの新バージョンを返す。
        var productIds = ImmutableList.Create("P001", "P002");
        var updatedProductIds = productIds.Add("P003");
        Console.WriteLine(
            $"元の ImmutableList の要素数: {productIds.Count}");
        Console.WriteLine(
            $"更新後の ImmutableList 要素数: {updatedProductIds.Count}");

        // 3. ImmutableArray の default の落とし穴。
        ImmutableArray<OrderLine> items = default;
        Console.WriteLine($"items.IsDefault = {items.IsDefault}"); // True
        try
        {
            _ = items.Length;
        }
        catch (NullReferenceException)
        {
            Console.WriteLine(
                "NullReferenceException を捕捉しました。" +
                "未初期化の ImmutableArray の内部配列は null です。");
        }

        var safeItems = ImmutableArray<OrderLine>.Empty;
        Console.WriteLine(safeItems.IsDefault); // False
        Console.WriteLine(safeItems.Length);    // 0
    }
}
