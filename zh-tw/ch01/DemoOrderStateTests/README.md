# 訂單狀態轉換的單元測試

對應第 1 章第 1.4 節的 xUnit 範例，驗證提交與付款後的新版本，並確認付款不會修改原本已提交的訂單。

此專案參考 `DemoImmutableOrder`，直接測試該範例的 `OrderWorkflow` 與訂單型別。

需要 .NET 10 SDK。在本目錄執行：

```shell
dotnet test
```

也可以在第 1 章目錄執行：

```shell
dotnet test ch01.slnx
```
