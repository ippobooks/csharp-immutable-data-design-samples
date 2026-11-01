# 附錄 A：點字專案重構案例閱讀導引

[繁體中文](../../zh-tw/ch99/README.md) | [English](../../en/ch99/README.md) | [日本語](../../ja/ch99/README.md)

這份導引整理了附錄 A 相關的原始碼、重構紀錄、和量測報告連結。完整專案位於公開的 [text-to-braille 儲存庫][project]；這裡只說明閱讀方式，不另存一份程式碼。

閱讀這個案例不需要先學點字規則。重點是了解一個已有使用者資料、編輯器、Undo/Redo、和效能限制的系統，如何逐步調整資料模型。

## 這個案例有哪些值得參考的做法

| 遇到的問題 | 採取的做法 | 適用情境 |
| --- | --- | --- |
| 載入後反覆查詢的對照表 | 建構時使用可變集合，完成後建立 frozen 索引 | 代碼表、費率表、和規則查詢 |
| 編輯器依賴同一批可變物件 | 保留可變模型，將集合修改集中到指定方法 | 編輯畫面、樹狀資料、和文件工具 |
| 逐步累積資料，又不能一次改完舊流程 | Builder 可建立新結果，也能把修改套回原物件 | 分階段重構與新舊流程共存 |
| 複製或還原後，物件參考改變 | 明確區分內容相等與編輯器的執行期識別 | 選取、定位、和 Undo/Redo |
| 舊文件仍要能讀寫 | 分別處理公開 API 與儲存格式的相容性 | JSON、YAML、和匯入匯出相容性 |

<a id="reading-order"></a>

## 建議閱讀順序

1. [變更總覽][summary]：先看改了哪些型別、哪些地方仍保留可變設計。
2. [查表重構解說][lookup-story]：了解為何用查詢索引取代 `DataTable.Select(...)`。
3. [Builder 重構解說][builder-story]：比較建立新結果與將修改套回原物件這兩種做法。
4. [效能退步原因分析][regression]：看看為什麼設計更清楚，不一定執行得更快。
5. [重構計畫][plan]與[完整開發紀錄][live-docs]：需要追查過程時再深入閱讀。
6. [整體 A/B 報告][overall-report]與[查表階段 A/B 報告][lookup-report]：區分整批重構的累積改善與單一階段的量測結果。

原始碼註解、完整重構紀錄、和量測報告主要使用中文。中、英、日三個版本的 README 都提供相同的閱讀順序與重點摘要；原始資料則保留原文。測試中的中文是用來驗證轉換功能的輸入資料，不應因翻譯導引而更動。

<a id="source-code"></a>

## 建議閱讀的原始碼

| 檔案 | 閱讀重點 |
| --- | --- |
| [XmlBrailleTable.cs][xml-table] | 載入 XML、建立索引、以及後續查詢 |
| [BrailleWordBuilder.cs][word-builder] | `Build()`、`ToBrailleWord()`、和 `ApplyTo(...)` 各自的用途 |
| [BrailleLine.cs][line-model]、[BrailleWord.cs][word-model] | 可變模型、唯讀檢視、修改方法、和執行期識別 |
| [BrailleWordBuilderTest.cs][builder-test] | Builder 與結果物件的行為驗證 |
| [YamlSerializationTests.cs][yaml-test] | YAML 讀寫相容性的測試 |
| [BrailleConversionBenchmarks.cs][benchmark-code]、[TestData][test-data] | 量測操作與輸入資料 |

Builder 與部分用來表示結果的介面是專案內部型別；附錄中的程式片段用來解說內部流程，無法直接複製到外部應用程式使用。`Build()` 產生新結果，`ApplyTo(...)` 則會修改傳入的既有物件，兩者不能混為一談。

<a id="run"></a>

## 執行測試與量測

先安裝 Git 與 .NET 10 SDK，並確認可以還原 NuGet 套件。以下命令會下載公開儲存庫，再從儲存庫的根目錄指定要執行的專案路徑。測試與量測只針對核心函式庫，不會啟動桌面編輯器。

本導引的原始碼與報告連結，以及下方 `git checkout` 命令，都指定同一個[參考版本][snapshot]：`c513c9f1fc8badb1bb843490fee60e57844e4e44`，避免分支後續變動影響閱讀。這是供讀者查閱的版本，並非下列歷史量測報告中用來比較的重構後版本（candidate）。

```sh
git clone --branch immutable-design https://github.com/braillekit/text-to-braille.git
cd text-to-braille
git checkout --detach c513c9f1fc8badb1bb843490fee60e57844e4e44

dotnet test src/EasyBrailleEditApp/BrailleToolkit.Tests/BrailleToolkit.Tests.csproj -c Release

dotnet run --project src/EasyBrailleEditApp/BrailleToolkit.Benchmarks/BrailleToolkit.Benchmarks.csproj -c Release -- --filter "*BrailleConversionBenchmarks*"
```

BenchmarkDotNet 需要一段時間執行，結果會受到硬體、作業系統、SDK、和執行環境影響。上述命令只會量測這個參考版本，不會自動執行重構前後的 A/B 比較。

<a id="benchmarks"></a>

## 如何核對書中的量測結果

| 比較範圍 | Baseline commit | Candidate commit |
| --- | --- | --- |
| [整批重構的累積結果][overall-report] | `ba9bc71d6b33b00dbe04de957c5acc2dc07e576d` | `279355533346a66352230062c89e30a5637cacf1` |
| [查表與索引重構][lookup-report] | `dcf71ef2d0aba531b6d724628234bec7d08168e1` | `8251b2df5bc25d0b4bb67826e3745cbc64f59b2a` |

若要重現報告中的比較結果，請依報告說明準備兩個獨立的 worktree，並使用相同的測資與量測程式。整批重構比較所用的基準版本（baseline）尚未加入效能量測專案；報告說明了如何只補入量測程式與測資，而不更動原有的功能程式碼。不要直接拿目前分支與任意舊版本比較，以免比較條件與報告不同。

這些結果反映了查表、Builder、和資料流調整所帶來的整體效果，不能用來證明「record、struct、或不可變設計必然比較快」。

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
