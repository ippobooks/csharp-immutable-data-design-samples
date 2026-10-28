# 附錄 A：點字專案重構案例閱讀導引

[繁體中文](../../zh-tw/ch99/README.md) | [English](../../en/ch99/README.md) | [日本語](../../ja/ch99/README.md)

這裡整理附錄 A 的原始碼、重構紀錄、和量測報告入口。完整專案在公開的 [text-to-braille repo][project]；此目錄只提供閱讀導引，不另外維護一份程式碼。

不用先學點字規則。這個案例要看的，是一個已有使用者資料、編輯器、Undo/Redo、和效能限制的系統，如何逐步調整資料模型。

## 從案例帶走什麼

| 遇到的問題 | 採取的做法 | 可以用在自己的專案 |
| --- | --- | --- |
| 載入後反覆查詢的對照表 | 建構時使用可變集合，完成後建立 frozen 索引 | 代碼表、費率表、和規則查詢 |
| 編輯器依賴同一批可變物件 | 保留可變模型，將集合修改集中到指定方法 | 編輯畫面、樹狀資料、和文件工具 |
| 逐步累積資料，又不能一次改完舊流程 | Builder 可建立新結果，也能把修改套回原物件 | 分階段重構與新舊流程共存 |
| 複製或還原後，物件參考改變 | 明確區分內容相等與編輯器的執行期識別 | 選取、定位、和 Undo/Redo |
| 舊文件仍要能讀寫 | 分開安排公開 API 與儲存格式 | JSON、YAML、和匯入匯出相容性 |

<a id="reading-order"></a>

## 建議閱讀順序

1. [變更總覽][summary]：先看改了哪些型別、哪些地方仍保留可變設計。
2. [查表重構解說][lookup-story]：了解為何用查詢索引取代 `DataTable.Select(...)`。
3. [Builder 重構解說][builder-story]：對照建立新結果與套回舊物件的兩條路徑。
4. [效能退步原因分析][regression]：看看為什麼設計更清楚，不一定執行得更快。
5. [重構計畫][plan]與[完整開發紀錄][live-docs]：需要追查過程時再深入閱讀。
6. [整體 A/B 報告][overall-report]與[查表階段 A/B 報告][lookup-report]：分清楚累積改善與單一階段的結果。

原始碼註解、完整重構紀錄、和量測報告主要使用中文。三語 README 提供相同的閱讀路線與重點摘要；原始資料保留原文。測試中的中文內容是轉換輸入，不應為了翻譯導引而改動。

<a id="source-code"></a>

## 原始碼入口

| 檔案 | 閱讀重點 |
| --- | --- |
| [XmlBrailleTable.cs][xml-table] | 載入 XML、建立索引、以及後續查詢 |
| [BrailleWordBuilder.cs][word-builder] | `Build()`、`ToBrailleWord()`、和 `ApplyTo(...)` 的不同責任 |
| [BrailleLine.cs][line-model]、[BrailleWord.cs][word-model] | 可變模型、唯讀檢視、修改方法、和執行期識別 |
| [BrailleWordBuilderTest.cs][builder-test] | Builder 與結果物件的行為驗證 |
| [YamlSerializationTests.cs][yaml-test] | YAML 讀寫相容性的測試 |
| [BrailleConversionBenchmarks.cs][benchmark-code]、[TestData][test-data] | 量測操作與輸入資料 |

Builder 與部分結果介面是專案內部型別；附錄片段用來解說內部流程，不能直接貼到外部應用程式呼叫。`Build()` 產生新結果，`ApplyTo(...)` 則會修改傳入的既有物件，兩者不能混為一談。

<a id="run"></a>

## 執行測試與量測

先安裝 Git 與 .NET 10 SDK，並確認可以還原 NuGet 套件。以下命令從公開 repo 根目錄使用的實際專案路徑執行；只測試核心函式庫，不啟動桌面編輯器。

連結與 checkout 固定在[參考快照][snapshot] `c513c9f1fc8badb1bb843490fee60e57844e4e44`，避免分支後續變動影響閱讀。此快照是導覽的參考版本，不是下列歷史量測報告的 candidate 版本。

```sh
git clone --branch immutable-design https://github.com/braillekit/text-to-braille.git
cd text-to-braille
git checkout --detach c513c9f1fc8badb1bb843490fee60e57844e4e44

dotnet test src/EasyBrailleEditApp/BrailleToolkit.Tests/BrailleToolkit.Tests.csproj -c Release

dotnet run --project src/EasyBrailleEditApp/BrailleToolkit.Benchmarks/BrailleToolkit.Benchmarks.csproj -c Release -- --filter "*BrailleConversionBenchmarks*"
```

BenchmarkDotNet 需要一段時間執行，結果會受到硬體、作業系統、SDK、和執行環境影響。上述命令只量測參考快照，不會自動執行重構前後的 A/B 比較。

<a id="benchmarks"></a>

## 如何核對書中的量測

| 比較範圍 | Baseline commit | Candidate commit |
| --- | --- | --- |
| [整批重構的累積結果][overall-report] | `ba9bc71d6b33b00dbe04de957c5acc2dc07e576d` | `279355533346a66352230062c89e30a5637cacf1` |
| [查表與索引重構][lookup-report] | `dcf71ef2d0aba531b6d724628234bec7d08168e1` | `8251b2df5bc25d0b4bb67826e3745cbc64f59b2a` |

重現歷史比較時，需依報告使用兩個獨立 worktree、相同測資、和相同量測程式。整批比較的 baseline 尚無 benchmark 專案，報告說明了如何只補入量測程式與測資，不改原有功能程式碼。不要把目前分支與任意舊版本直接比較。

結果包含查表、Builder、和資料流調整的共同效果，不能當成「record、struct、或不可變設計必然比較快」的證明。

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
