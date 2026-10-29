using System;
using System.Collections.Generic;

public record OrderLine(string ProductId, int Quantity);

public record Order(
    string Id,
    List<OrderLine> Lines // List<T> は変更可能。
);

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== 浅いイミュータビリティの落とし穴 ===");

        var order = new Order("ORD-001", new List<OrderLine>
        {
            new("P001", 2)
        });

        Console.WriteLine($"変更前の明細数: {order.Lines.Count}");

        // Lines の参照自体は置き換えられない。
        // order.Lines = new List<OrderLine>(); // コンパイルエラー!

        // プロパティは再代入できないが、内部の List は変更できる。
        order.Lines.Add(new OrderLine("P002", 5));

        Console.WriteLine(
            $"変更後の明細数: {order.Lines.Count}"); // 2 になる!
    }
}
