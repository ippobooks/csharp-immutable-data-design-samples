using System.Collections.Frozen;

// 初期化時にデータを用意し、後続の検索用に frozen dictionary を作る。
var categoryNames = new Dictionary<string, string>
{
    ["BOOK"] = "書籍",
    ["SOFTWARE"] = "ソフトウェア"
}.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

Console.WriteLine(categoryNames["book"]); // 出力: 書籍
