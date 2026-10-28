# DemoOrderStateHierarchy

用 record 繼承表示不同訂單狀態，適用於 C# 14 / .NET 10，
不需要 preview SDK。範例展示草稿訂單、已提交訂單、以及已付款訂單
各自需要的資料，以及接收舊狀態、回傳新狀態的轉換方法。

```powershell
dotnet run --project DemoOrderStateHierarchy.csproj
```

程式確認轉換保留訂單識別與時間，並展示新增 `ReturnedOrder` 後，
原有模式比對必須在執行期拒絕未支援狀態。它不是封閉 union，
也沒有新增狀態時的編譯器窮盡性提醒。

取消註解 `OrderWorkflow.Pay(draft, paidAt)` 會產生編譯錯誤，因為
方法只接受 `SubmittedOrder`。公開建構式仍是教學簡化，實際業務規則
與建構入口應另行安排。
