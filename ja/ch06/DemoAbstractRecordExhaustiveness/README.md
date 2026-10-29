# DemoAbstractRecordExhaustiveness

`abstract record` を基底型とし、派生 record を `sealed` にしても、C# コンパイラーが
閉じた継承階層として扱わないことを確認します。

実行方法:

```powershell
dotnet build .\DemoAbstractRecordExhaustiveness.csproj
```

期待する結果: ビルドは成功しますが、`Program.cs` の `switch` 式に対して
コンパイラーが `CS8509` 警告を出します。
