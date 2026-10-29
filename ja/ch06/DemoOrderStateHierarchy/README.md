# DemoOrderStateHierarchy

record の継承で注文の各状態を表します。C# 14 / .NET 10 に対応し、
プレビュー版 SDK は不要です。下書き、確定済み、支払い済みの注文に必要なデータと、
元の状態を受け取って新しい状態を返す遷移メソッドを示します。

```powershell
dotnet run --project DemoOrderStateHierarchy.csproj
```

遷移後も注文の識別情報と時刻が保持されることを確認します。また、`ReturnedOrder` を
追加した場合、既存のパターンマッチングが未対応の状態を実行時に拒否する必要があることも示します。
これは閉じた union ではなく、状態を追加してもコンパイラーによる網羅性の警告は出ません。

`OrderWorkflow.Pay(draft, paidAt)` のコメントを外すと、メソッドが `SubmittedOrder` だけを
受け付けるため、コンパイルエラーになります。公開コンストラクターは説明を簡単にするためのもので、
実際のビジネスルールや作成方法の制限は別途設計する必要があります。
