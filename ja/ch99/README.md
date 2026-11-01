# 付録 A：点字プロジェクトのリファクタリングを読むためのガイド

[繁體中文](../../zh-tw/ch99/README.md) | [English](../../en/ch99/README.md) | [日本語](../../ja/ch99/README.md)

このガイドでは、付録 A に関連するソースコード、リファクタリングの記録、ベンチマーク報告へのリンクをまとめています。プロジェクト全体は公開の [text-to-braille リポジトリ][project]にあります。ここでは読み方を案内するだけで、コードのコピーは別途管理していません。

この事例を読むために、点字の規則をあらかじめ学ぶ必要はありません。注目するのは、ユーザーデータ、エディター、Undo/Redo の機能を持ち、性能面での制約もある既存システムで、データモデルを少しずつ変えていく方法です。

## 自分のプロジェクトでも参考にできる設計

| 課題 | この事例での対応 | 活用できる場面 |
| --- | --- | --- |
| 一度読み込んだ対応表を何度も検索する | 構築中は可変コレクションを使い、完成後に frozen インデックスを作る | コード表、料金表、ルールの検索 |
| エディターが既存の可変オブジェクトに依存している | 可変モデルを残し、コレクションの変更を専用メソッドに集める | 編集画面、ツリー、文書編集ツール |
| データを段階的に組み立て、従来の処理も残す必要がある | Builder で新しい結果を作ることも、既存オブジェクトに変更を反映することもできるようにする | 段階的な移行と新旧処理の共存 |
| コピーやデシリアライズでオブジェクト参照が変わる | 内容の等価性とエディター内で使う実行時の識別を分ける | 選択、位置の追跡、Undo/Redo |
| 既存の文書を引き続き読み書きする必要がある | 公開 API と保存形式の互換性をそれぞれ考慮する | JSON、YAML、インポートとエクスポートの互換性 |

<a id="reading-order"></a>

## おすすめの読み順

1. [変更のまとめ][summary]：変更した型と、可変のまま残したモデルを確認します。
2. [検索処理のリファクタリング解説][lookup-story]：なぜ `DataTable.Select(...)` を検索インデックスに置き換えたのかを読みます。
3. [Builder のリファクタリング解説][builder-story]：新しい結果を作る方法と、既存オブジェクトを更新する方法を比較します。
4. [性能低下の原因分析][regression]：設計がわかりやすくなっても、処理が遅くなる場合があることを確認します。
5. [リファクタリング計画][plan]と[開発記録全体][live-docs]：必要に応じて各段階を詳しく追います。
6. [全体の A/B 報告][overall-report]と[検索処理の A/B 報告][lookup-report]：リファクタリング全体で積み重ねた改善と、一つの段階での計測結果を区別します。

ソースコードのコメント、リファクタリングの詳しい記録、ベンチマーク報告は主に中国語で書かれています。中国語、英語、日本語の README では、同じ読み順と要点を案内しています。元の資料は原文のままです。テスト内の中国語テキストは変換機能を確認するための入力データなので、ガイドの翻訳に合わせて変更しないでください。

<a id="source-code"></a>

## 読んでおきたいソースコード

| ファイル | 注目する点 |
| --- | --- |
| [XmlBrailleTable.cs][xml-table] | XML の読み込み、インデックスの構築、検索 |
| [BrailleWordBuilder.cs][word-builder] | `Build()`、`ToBrailleWord()`、`ApplyTo(...)` の役割の違い |
| [BrailleLine.cs][line-model]と[BrailleWord.cs][word-model] | 可変モデル、読み取り専用ビュー、変更メソッド、実行時の識別 |
| [BrailleWordBuilderTest.cs][builder-test] | Builder と結果オブジェクトの動作確認 |
| [YamlSerializationTests.cs][yaml-test] | YAML の読み書きの互換性テスト |
| [BrailleConversionBenchmarks.cs][benchmark-code]と[TestData][test-data] | 計測する処理と入力データ |

Builder と、結果を表す一部のインターフェイスは、プロジェクト内部用の型です。付録のコード例は内部の処理を説明するためのもので、別のアプリケーションにコピーしてそのまま使うことはできません。`Build()` は新しい結果を作り、`ApplyTo(...)` は渡された既存オブジェクトを変更します。この二つの処理は区別する必要があります。

<a id="run"></a>

## テストとベンチマークの実行

まず Git と .NET 10 SDK をインストールし、必要な NuGet パッケージを復元できることを確認してください。以下のコマンドでは、公開リポジトリをダウンロードした後、そのルートディレクトリから実行対象のプロジェクトのパスを指定します。テストと計測の対象はコアライブラリだけで、デスクトップエディターは起動しません。

このガイドのソースコードと報告へのリンク、および以下の `git checkout` コマンドは、いずれも同じ[参照用バージョン][snapshot] `c513c9f1fc8badb1bb843490fee60e57844e4e44` を指定しています。ブランチがその後更新されても、同じ内容を読めるようにするためです。これは読者が内容を確認するためのバージョンであり、後述の過去の計測報告で比較に使ったリファクタリング後のバージョン（candidate）とは異なります。

```sh
git clone --branch immutable-design https://github.com/braillekit/text-to-braille.git
cd text-to-braille
git checkout --detach c513c9f1fc8badb1bb843490fee60e57844e4e44

dotnet test src/EasyBrailleEditApp/BrailleToolkit.Tests/BrailleToolkit.Tests.csproj -c Release

dotnet run --project src/EasyBrailleEditApp/BrailleToolkit.Benchmarks/BrailleToolkit.Benchmarks.csproj -c Release -- --filter "*BrailleConversionBenchmarks*"
```

BenchmarkDotNet の実行には時間がかかります。結果はハードウェア、OS、SDK、実行環境によって変わります。上のコマンドで計測するのはこの参照用バージョンだけで、変更前後の A/B 比較を自動で実行するわけではありません。

<a id="benchmarks"></a>

## 書籍に掲載した計測結果の確認

| 比較対象 | Baseline commit | Candidate commit |
| --- | --- | --- |
| [リファクタリング全体の累積結果][overall-report] | `ba9bc71d6b33b00dbe04de957c5acc2dc07e576d` | `279355533346a66352230062c89e30a5637cacf1` |
| [検索処理とインデックスのリファクタリング][lookup-report] | `dcf71ef2d0aba531b6d724628234bec7d08168e1` | `8251b2df5bc25d0b4bb67826e3745cbc64f59b2a` |

報告の比較結果を再現するには、その手順に従って独立した二つの worktree を用意し、同じ入力データと計測コードを使ってください。リファクタリング全体の比較で使った基準バージョン（baseline）には、まだベンチマークプロジェクトがありません。報告では、元のアプリケーションのコードを変更せずに、計測コードとテストデータだけを追加する手順を説明しています。現在のブランチと任意の旧版をそのまま比較すると、報告とは比較条件が異なる可能性があります。

これらの結果は、検索処理、Builder、データの受け渡し方の変更を合わせた効果を表しています。record、struct、イミュータブル設計なら必ず高速になることを示すものではありません。

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
