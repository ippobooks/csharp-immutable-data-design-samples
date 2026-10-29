// readonly record struct は小さなイミュータブルな値に適する。
var price = new Money(1200m, "TWD");
var discounted = price with { Amount = 960m };

Console.WriteLine(price);      // Money { Amount = 1200, Currency = TWD }
Console.WriteLine(discounted); // Money { Amount = 960, Currency = TWD }

// ボックス化: ラッパーは price の値のコピーを保持する。
object boxedPrice = price;
Console.WriteLine(boxedPrice); // Money { Amount = 1200, Currency = TWD }

// 通常の record struct の位置指定プロパティは既定で変更可能。
var point = new MutablePoint(1.0, 2.0);
point.X = 5.0;
Console.WriteLine(point); // MutablePoint { X = 5, Y = 2 }

// 配列要素へのアクセスは、その位置の値を直接変更できる。
var array = new MutableCounter[] { new() };
array[0].Increment();
Console.WriteLine(array[0].Value); // 出力: 1

// インデクサーは struct を値で返し、List<T> でも変更はコピーだけ。
IList<MutableCounter> list = new List<MutableCounter> { new() };
list[0].Increment();
Console.WriteLine(list[0].Value); // 出力: 0

public readonly record struct Money(
    decimal Amount,
    string Currency
);

public record struct MutablePoint(double X, double Y);

public struct MutableCounter
{
    public int Value;

    public void Increment()
    {
        Value++;
    }
}
