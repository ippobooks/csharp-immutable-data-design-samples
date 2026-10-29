# 注文の状態遷移の単体テスト

1.4 節の xUnit サンプルです。確定と支払いで返される新しいバージョンを検証し、
支払いによって元の確定済み注文が変更されないことを確認します。

`DemoImmutableOrder` を参照し、その `OrderWorkflow` と注文の型を直接テストします。

.NET 10 SDK が必要です。このディレクトリで実行してください。

```shell
dotnet test
```

第1章のディレクトリから実行することもできます。

```shell
dotnet test ch01.slnx
```
