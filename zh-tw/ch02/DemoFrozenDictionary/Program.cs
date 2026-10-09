using System.Collections.Frozen;

// 在初始化階段建立，執行期間只進行查詢。
var categoryNames = new Dictionary<string, string>
{
    ["BOOK"] = "書籍",
    ["SOFTWARE"] = "軟體"
}.ToFrozenDictionary();

Console.WriteLine(categoryNames["BOOK"]); // 輸出：書籍
