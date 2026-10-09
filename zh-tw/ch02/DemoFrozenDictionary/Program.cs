using System.Collections.Frozen;

// 初始化時準備資料，再建立供後續查詢使用的 frozen dictionary。
var categoryNames = new Dictionary<string, string>
{
    ["BOOK"] = "書籍",
    ["SOFTWARE"] = "軟體"
}.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

Console.WriteLine(categoryNames["book"]); // 輸出：書籍
