using System.Collections.Frozen;

// Prepare data at initialization, then freeze it for later lookups.
var categoryNames = new Dictionary<string, string>
{
    ["BOOK"] = "Books",
    ["SOFTWARE"] = "Software"
}.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

Console.WriteLine(categoryNames["book"]); // Output: Books
