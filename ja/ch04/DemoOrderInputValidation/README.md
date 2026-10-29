# DemoOrderInputValidation

C# 14、.NET 10、FluentValidation 12.2.0 を使い、入力規則を独立した
validator に定義し、Minimal API で `ValidateAsync` を明示的に呼ぶ例です。
DTO はデータの保持だけを担当し、Data annotation や検証メソッドは持ちません。

```powershell
dotnet run --project DemoOrderInputValidation.csproj
```

アプリが表示する待ち受けアドレスに、`POST /orders/validate` を送信してください。
リクエスト本文の例です。

```json
{
  "customerId": "CUST-42",
  "lines": [{ "productId": "P001", "quantity": 2 }]
}
```

顧客番号が空白ではなく 50 文字以内で、明細コレクションが空でなければ 200 を返します。
これらの規則に違反すると、フィールドのエラーを含む 400 validation problem response を返します。

この例は顧客番号と明細コレクションだけを確認し、各商品の番号や数量は検証しません。
注文の作成や保存も行いません。実際のシステムでは入力検証を補い、application service が
後続処理を調整し、domain model がビジネスルールを守るようにしてください。
