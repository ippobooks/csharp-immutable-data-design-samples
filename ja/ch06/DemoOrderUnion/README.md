# DemoOrderUnion

C# 15 の union 型を使い、型安全な注文のワークフローを示します。
各注文状態を独立した record とし、`Order` union が有効な状態の閉じた集合を定義します。
ワークフローメソッドは特定の遷移元の状態を受け取り、特定の遷移先の状態を返します。
`Order` に対するパターンマッチングは、すべての状態を網羅します。

必要な環境:

- .NET 11 Preview 7 以降
- C# の言語バージョンを `preview` に設定

実行方法:

```powershell
dotnet run --project .\DemoOrderUnion.csproj
```
