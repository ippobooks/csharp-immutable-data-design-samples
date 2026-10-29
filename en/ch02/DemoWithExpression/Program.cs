Console.WriteLine("=== with expressions ===");

var originalLine = new OrderLine("P001", 5);
var updatedLine = originalLine with { Quantity = 10 };

Console.WriteLine("Basic with:");
Console.WriteLine($"originalLine.Quantity = {originalLine.Quantity}");
Console.WriteLine($"updatedLine.Quantity  = {updatedLine.Quantity}");

var profile = new CustomerProfile(
    "C001",
    "Xiaoming Wang",
    new Address("1 Zhongshan Road", "Taipei City", "10001"));

var updatedProfile = profile with
{
    ShippingAddress = profile.ShippingAddress with
    {
        City = "New Taipei City",
        PostalCode = "22001"
    }
};

Console.WriteLine();
Console.WriteLine("Nested with:");
PrintProfile("Original profile", profile);
PrintProfile("Updated profile", updatedProfile);

// WithCity shows a partial update; also validate address consistency.
var changedByMethod = profile.WithCity("Taoyuan City");

Console.WriteLine();
Console.WriteLine("Using WithCity:");
PrintProfile("Original profile", profile);
PrintProfile("After WithCity", changedByMethod);

static void PrintProfile(string label, CustomerProfile profile)
{
    Console.WriteLine(
        $"{label}: {profile.Name}, " +
        $"{profile.ShippingAddress.City} " +
        $"{profile.ShippingAddress.PostalCode}");
}

public record OrderLine(string ProductId, int Quantity);

public record Address(
    string Street,
    string City,
    string PostalCode
);

public record CustomerProfile(
    string Id,
    string Name,
    Address ShippingAddress)
{
    public CustomerProfile WithCity(string city)
    {
        return this with
        {
            ShippingAddress = ShippingAddress with { City = city }
        };
    }
}
