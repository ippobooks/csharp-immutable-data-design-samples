Console.WriteLine("=== with 式 ===");

var originalLine = new OrderLine("P001", 5);
var updatedLine = originalLine with { Quantity = 10 };

Console.WriteLine("基本の with:");
Console.WriteLine($"originalLine.Quantity = {originalLine.Quantity}");
Console.WriteLine($"updatedLine.Quantity  = {updatedLine.Quantity}");

var profile = new CustomerProfile(
    "C001",
    "ワン・シャオミン",
    new Address("中山路1号", "台北市", "10001"));

var updatedProfile = profile with
{
    ShippingAddress = profile.ShippingAddress with
    {
        City = "新北市",
        PostalCode = "22001"
    }
};

Console.WriteLine();
Console.WriteLine("入れ子の with:");
PrintProfile("元の profile", profile);
PrintProfile("更新後の profile", updatedProfile);

// WithCity は部分更新の例。実際には住所全体の整合性も検証する。
var changedByMethod = profile.WithCity("桃園市");

Console.WriteLine();
Console.WriteLine("WithCity を使用:");
PrintProfile("元の profile", profile);
PrintProfile("WithCity の後", changedByMethod);

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
