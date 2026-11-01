# Appendix A: guide to the Braille refactoring case study

[繁體中文](../../zh-tw/ch99/README.md) | [English](../../en/ch99/README.md) | [日本語](../../ja/ch99/README.md)

This guide collects links to the source code, refactoring notes, and benchmark reports for Appendix A. The full project is available in the public [text-to-braille repository][project]. This directory provides guidance on what to read; it does not maintain a separate copy of the code.

You do not need to know Braille rules to follow this case study. The focus is on gradually changing the data model in a system that already has user data, an editor, Undo/Redo support, and performance constraints.

## Approaches you can apply in your own projects

| Problem | Approach | Where it applies |
| --- | --- | --- |
| Lookup data is loaded once and queried repeatedly | Build with mutable collections, then create frozen indexes | Code tables, rates, and rule lookup |
| The editor depends on existing mutable objects | Keep mutable models and route collection changes through explicit methods | Editing screens, trees, and document tools |
| Construction is incremental and older workflows must keep working | Let a builder produce a new result or apply changes to an existing object | Gradual refactoring and coexistence of old and new workflows |
| Copying or deserialization changes object references | Separate content equality from runtime identity used by the editor | Selection, positioning, and Undo/Redo |
| Existing documents must remain readable and writable | Handle compatibility for the public API and the storage format separately | JSON, YAML, and import/export compatibility |

<a id="reading-order"></a>

## Suggested reading order

1. [Change summary][summary]: see which types changed and which models stayed mutable.
2. [Lookup refactoring walkthrough][lookup-story]: see why lookup indexes replaced `DataTable.Select(...)`.
3. [Builder refactoring walkthrough][builder-story]: compare producing a new result with updating an existing object.
4. [Performance regression analysis][regression]: see why a clearer design can still run more slowly.
5. [Refactoring plan][plan] and [full development notes][live-docs]: follow individual steps when you need more detail.
6. [Overall A/B report][overall-report] and [lookup-stage A/B report][lookup-report]: separate the cumulative improvements across the full refactoring effort from the results of a single stage.

Source comments, full refactoring notes, and benchmark reports are primarily in Chinese. The Chinese, English, and Japanese README files provide the same reading order and summarize the same key points; the source materials remain in their original language. Chinese text in the tests is input data used to verify conversion behavior and should not be changed when translating the guide.

<a id="source-code"></a>

## Suggested source files to read

| File | What to look for |
| --- | --- |
| [XmlBrailleTable.cs][xml-table] | XML loading, index construction, and lookup |
| [BrailleWordBuilder.cs][word-builder] | What `Build()`, `ToBrailleWord()`, and `ApplyTo(...)` each do |
| [BrailleLine.cs][line-model] and [BrailleWord.cs][word-model] | Mutable models, read-only views, mutation methods, and runtime identity |
| [BrailleWordBuilderTest.cs][builder-test] | Checks for builder and result behavior |
| [YamlSerializationTests.cs][yaml-test] | Tests for YAML read/write compatibility |
| [BrailleConversionBenchmarks.cs][benchmark-code] and [TestData][test-data] | Measured operations and input data |

The builder and some interfaces used to represent results are internal types. The snippets in the appendix explain the project's internal workflow and cannot be copied directly into a separate application for use there. `Build()` creates a new result, whereas `ApplyTo(...)` modifies the existing object passed to it. These are distinct operations.

<a id="run"></a>

## Run the tests and benchmarks

Install Git and the .NET 10 SDK, and make sure you can restore the required NuGet packages. The commands below download the public repository, then specify the project paths relative to its root directory. The tests and benchmarks cover only the core library; they do not launch the desktop editor.

The source code and report links in this guide, along with the `git checkout` command below, all use the same [reference version][snapshot]: `c513c9f1fc8badb1bb843490fee60e57844e4e44`. This keeps subsequent branch changes from affecting the guide. This version is provided for readers to explore; it is not the version used as the refactoring candidate in the historical benchmark reports below.

```sh
git clone --branch immutable-design https://github.com/braillekit/text-to-braille.git
cd text-to-braille
git checkout --detach c513c9f1fc8badb1bb843490fee60e57844e4e44

dotnet test src/EasyBrailleEditApp/BrailleToolkit.Tests/BrailleToolkit.Tests.csproj -c Release

dotnet run --project src/EasyBrailleEditApp/BrailleToolkit.Benchmarks/BrailleToolkit.Benchmarks.csproj -c Release -- --filter "*BrailleConversionBenchmarks*"
```

BenchmarkDotNet takes time to run. Results depend on hardware, OS, SDK, and the execution environment. These commands measure only this reference version; they do not automatically run a before-and-after comparison.

<a id="benchmarks"></a>

## Check the measurements cited in the book

| Comparison | Baseline commit | Candidate commit |
| --- | --- | --- |
| [Cumulative refactoring results][overall-report] | `ba9bc71d6b33b00dbe04de957c5acc2dc07e576d` | `279355533346a66352230062c89e30a5637cacf1` |
| [Lookup and index refactoring][lookup-report] | `dcf71ef2d0aba531b6d724628234bec7d08168e1` | `8251b2df5bc25d0b4bb67826e3745cbc64f59b2a` |

To reproduce the comparisons in the reports, follow their instructions to set up two separate worktrees and use the same input data and benchmark code. The baseline for the cumulative comparison predates the benchmark project. The report explains how to add only the benchmark code and test data, leaving the original application code unchanged. Do not compare the current branch directly with an arbitrary older version, as the comparison conditions may differ from those in the report.

The results combine lookup, builder, and data-flow changes. They do not show that records, structs, or immutable designs are inherently faster.

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
