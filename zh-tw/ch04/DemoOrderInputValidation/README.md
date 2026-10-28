# DemoOrderInputValidation

使用 C# 14、.NET 10、以及 FluentValidation 12.2.0，展示把輸入規則
放在獨立 validator，並在 Minimal API 中明確呼叫 `ValidateAsync`。
DTO 僅承載資料，沒有 Data annotation 或驗證方法。

```powershell
dotnet run --project DemoOrderInputValidation.csproj
```

向程式顯示的監聽位址送出 `POST /orders/validate`，例如：

```json
{
  "customerId": "CUST-42",
  "lines": [{ "productId": "P001", "quantity": 2 }]
}
```

客戶編號非空且不超過 50 字元、明細集合非空時，回傳 200；
違反上述規則時，回傳包含欄位錯誤的 400 validation problem response。

本範例只示範客戶編號與明細集合的檢查，沒有逐項驗證商品編號與數量，
也不會建立或儲存訂單。實際系統應補齊輸入檢查，並由 application
service 協調後續工作，讓 domain model 保護業務規則。
