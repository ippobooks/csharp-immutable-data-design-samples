// readonly record struct 適合小型、不可變的值。
var price = new Money(1200m, "TWD");
var discounted = price with { Amount = 960m };

Console.WriteLine(price);      // Money { Amount = 1200, Currency = TWD }
Console.WriteLine(discounted); // Money { Amount = 960, Currency = TWD }

object boxedPrice = price; // 裝箱：包裝物件保存 price 的值複本
Console.WriteLine(boxedPrice); // Money { Amount = 1200, Currency = TWD }

// 一般 record struct 的位置式屬性預設可修改。
var point = new MutablePoint(1.0, 2.0);
point.X = 5.0;
Console.WriteLine(point); // MutablePoint { X = 5, Y = 2 }

// 陣列元素存取可以直接修改該位置的值。
var array = new MutableCounter[] { new() };
array[0].Increment();
Console.WriteLine(array[0].Value); // 輸出：1

// 索引子按值回傳 struct；直接使用 List<T> 也只會改到複本。
IList<MutableCounter> list = new List<MutableCounter> { new() };
list[0].Increment();
Console.WriteLine(list[0].Value); // 輸出：0

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
