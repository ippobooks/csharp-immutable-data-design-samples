# 付録 A：点字プロジェクトのリファクタリングを読むためのガイド

[繁體中文](../../zh-tw/ch99/README.md) | [English](../../en/ch99/README.md) | [日本語](../../ja/ch99/README.md)

付録 A に関連するソースコード、リファクタリングの記録、ベンチマーク報告への入口をまとめました。プロジェクト全体は公開の [text-to-braille リポジトリ][project]で管理されています。このディレクトリではコードを複製せず、読み方を案内します。

点字の規則を学ぶ必要はありません。注目するのは、エディター、Undo/Redo、既存の保存データ、実行性能を維持しながら、既存システムの設計を少しずつ変える方法です。

## 自分のプロジェクトに活かせること

| 課題 | この事例での対応 | 他のプロジェクトでの用途 |
| --- | --- | --- |
| 一度読み込んだ対応表を何度も検索する | 構築中は可変コレクションを使い、完成後に frozen インデックスを作る | コード表、料金表、ルールの検索 |
| エディターが既存の可変オブジェクトに依存している | 可変モデルを残し、コレクションの変更を専用メソッドに集める | 編集画面、ツリー、文書編集ツール |
| データを段階的に組み立て、従来の処理も残す必要がある | Builder から新しい結果を作る経路と、既存オブジェクトへ反映する経路を用意する | 段階的な移行と新旧処理の共存 |
| コピーやデシリアライズでオブジェクト参照が変わる | 内容の等価性とエディター内で使う実行時の識別を分ける | 選択、位置の追跡、Undo/Redo |
| 既存の文書を引き続き読み書きする必要がある | 公開 API と保存形式を別々に変更できるようにする | JSON、YAML、インポートとエクスポートの互換性 |

<a id="reading-order"></a>

## おすすめの読む順序

1. [変更のまとめ][summary]：変更した型と、可変のまま残したモデルを確認します。
2. [検索処理のリファクタリング解説][lookup-story]：なぜ `DataTable.Select(...)` を検索インデックスに置き換えたのかを読みます。
3. [Builder のリファクタリング解説][builder-story]：新しい結果を作る方法と、既存オブジェクトを更新する方法を比較します。
4. [性能低下の原因分析][regression]：設計がわかりやすくなっても、処理が遅くなる場合があることを確認します。
5. [リファクタリング計画][plan]と[開発記録全体][live-docs]：必要に応じて各段階を詳しく追います。
6. [全体の A/B 報告][overall-report]と[検索処理の A/B 報告][lookup-report]：変更全体の効果と、特定の段階の効果を区別します。

ソースコードのコメント、開発記録、ベンチマーク報告は主に中国語です。3 言語の README は同じ読む順序と設計判断の要約を提供し、元の資料は原文のまま残しています。ベンチマーク内の中国語テキストは変換対象の入力データなので、説明文として翻訳するものではありません。

<a id="source-code"></a>

## ソースコードの入口

| ファイル | 注目する点 |
| --- | --- |
| [XmlBrailleTable.cs][xml-table] | XML の読み込み、インデックスの構築、検索 |
| [BrailleWordBuilder.cs][word-builder] | `Build()`、`ToBrailleWord()`、`ApplyTo(...)` の役割の違い |
| [BrailleLine.cs][line-model]と[BrailleWord.cs][word-model] | 可変モデル、読み取り専用ビュー、変更メソッド、実行時の識別 |
| [BrailleWordBuilderTest.cs][builder-test] | Builder と結果オブジェクトの動作確認 |
| [YamlSerializationTests.cs][yaml-test] | YAML の読み書きの互換性テスト |
| [BrailleConversionBenchmarks.cs][benchmark-code]と[TestData][test-data] | 計測する処理と入力データ |

Builder と一部の結果インターフェイスは内部用の型です。付録のコード片はプロジェクト内部の処理を説明するもので、別のアプリケーションに貼り付けてそのまま呼び出せる API ではありません。`Build()` は新しい結果を作り、`ApplyTo(...)` は渡された既存オブジェクトを変更します。

<a id="run"></a>

## テストとベンチマークの実行

Git と .NET 10 SDK をインストールし、必要な NuGet パッケージを取得できる環境を用意してください。以下のコマンドは公開リポジトリの実際のパスを使います。デスクトップエディターは起動せず、コアライブラリのテストを実行します。

リンクとチェックアウト先は[参照用スナップショット][snapshot] `c513c9f1fc8badb1bb843490fee60e57844e4e44` に固定しています。このガイド用の参照版であり、後述の過去の計測報告にある candidate 版とは異なります。

```sh
git clone --branch immutable-design https://github.com/braillekit/text-to-braille.git
cd text-to-braille
git checkout --detach c513c9f1fc8badb1bb843490fee60e57844e4e44

dotnet test src/EasyBrailleEditApp/BrailleToolkit.Tests/BrailleToolkit.Tests.csproj -c Release

dotnet run --project src/EasyBrailleEditApp/BrailleToolkit.Benchmarks/BrailleToolkit.Benchmarks.csproj -c Release -- --filter "*BrailleConversionBenchmarks*"
```

BenchmarkDotNet の実行には時間がかかります。結果はハードウェア、OS、SDK、実行環境によって変わります。上のコマンドで計測するのは参照用スナップショットだけで、変更前後の A/B 比較を自動で実行するわけではありません。

<a id="benchmarks"></a>

## 書籍に掲載した計測結果の確認

| 比較対象 | Baseline commit | Candidate commit |
| --- | --- | --- |
| [リファクタリング全体の累積結果][overall-report] | `ba9bc71d6b33b00dbe04de957c5acc2dc07e576d` | `279355533346a66352230062c89e30a5637cacf1` |
| [検索処理とインデックスのリファクタリング][lookup-report] | `dcf71ef2d0aba531b6d724628234bec7d08168e1` | `8251b2df5bc25d0b4bb67826e3745cbc64f59b2a` |

過去の比較を再現するには、報告に従って個別の worktree、同一の入力データ、同一のベンチマークコードを使います。全体比較の baseline には、まだベンチマークプロジェクトがありません。報告には、元の製品コードを変えずに計測コードとテストデータだけを追加した手順が記されています。任意の旧版と現在のブランチを比較しても、同じ実験にはなりません。

結果には検索処理、Builder、データの受け渡し方の変更がまとめて含まれます。record、struct、不変設計なら必ず高速になることを示すものではありません。

[project]: https://github.com/braillekit/text-to-braille
[live-docs]: https://github.com/braillekit/text-to-braille/tree/immutable-design/docs-dev/refactoring/immutable
[snapshot]: https://github.com/braillekit/text-to-braille/tree/c513c9f1fc8badb1bb843490fee60e57844e4e44/
[summary]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/docs-dev/refactoring/immutable/immutable-design-change-summary.md
[lookup-story]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/docs-dev/refactoring/immutable/blog-post-1-performance-data-table.md
[builder-story]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/docs-dev/refactoring/immutable/blog-post-2-builder-boundary.md
[plan]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/docs-dev/refactoring/immutable/immutable-refactoring-plan.md
[regression]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/docs-dev/refactoring/immutable/phase4-cause-analysis.md
[overall-report]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/docs-dev/refactoring/immutable/benchmark-result/2026-04-06-branch-start-vs-phase4d-clean-worktree-ab.md
[lookup-report]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/docs-dev/refactoring/immutable/benchmark-result/2026-04-05-phase3-clean-worktree-ab.md
[xml-table]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/src/EasyBrailleEditApp/BrailleToolkit/Data/XmlBrailleTable.cs
[word-builder]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/src/EasyBrailleEditApp/BrailleToolkit/BrailleWordBuilder.cs
[line-model]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/src/EasyBrailleEditApp/BrailleToolkit/BrailleLine.cs
[word-model]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/src/EasyBrailleEditApp/BrailleToolkit/BrailleWord.cs
[builder-test]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/src/EasyBrailleEditApp/BrailleToolkit.Tests/BrailleWordBuilderTest.cs
[yaml-test]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/src/EasyBrailleEditApp/BrailleToolkit.Tests/YamlSerializationTests.cs
[benchmark-code]: https://github.com/braillekit/text-to-braille/blob/c513c9f1fc8badb1bb843490fee60e57844e4e44/src/EasyBrailleEditApp/BrailleToolkit.Benchmarks/BrailleConversionBenchmarks.cs
[test-data]: https://github.com/braillekit/text-to-braille/tree/c513c9f1fc8badb1bb843490fee60e57844e4e44/src/EasyBrailleEditApp/BrailleToolkit.Benchmarks/TestData
