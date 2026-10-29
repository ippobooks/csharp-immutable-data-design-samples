// readonly record struct suits small, immutable values.
var price = new Money(1200m, "TWD");
var discounted = price with { Amount = 960m };

Console.WriteLine(price);      // Money { Amount = 1200, Currency = TWD }
Console.WriteLine(discounted); // Money { Amount = 960, Currency = TWD }

object boxedPrice = price; // Boxing: the wrapper stores a copy of price.
Console.WriteLine(boxedPrice); // Money { Amount = 1200, Currency = TWD }

// Positional properties of a record struct are mutable by default.
var point = new MutablePoint(1.0, 2.0);
point.X = 5.0;
Console.WriteLine(point); // MutablePoint { X = 5, Y = 2 }

// Array element access can modify the value in place.
var array = new MutableCounter[] { new() };
array[0].Increment();
Console.WriteLine(array[0].Value); // Output: 1

// Indexers return struct copies, even when using List<T> directly.
IList<MutableCounter> list = new List<MutableCounter> { new() };
list[0].Increment();
Console.WriteLine(list[0].Value); // Output: 0

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
