# Appendix A: guide to the Braille refactoring case study

[繁體中文](../../zh-tw/ch99/README.md) | [English](../../en/ch99/README.md) | [日本語](../../ja/ch99/README.md)

This guide connects Appendix A to its source code, refactoring notes, and benchmark reports. The full project lives in the public [text-to-braille repository][project]. This directory contains a reading guide, not a separate copy of the implementation.

You do not need to know Braille rules. The case study is about changing an existing system while keeping its editor, Undo/Redo behavior, saved documents, and performance usable.

## What to take from the case study

| Problem | Approach | Where it applies elsewhere |
| --- | --- | --- |
| Lookup data is loaded once and queried repeatedly | Build with mutable collections, then create frozen indexes | Code tables, rates, and rule lookup |
| The editor depends on existing mutable objects | Keep mutable models and route collection changes through explicit methods | Editing screens, trees, and document tools |
| Construction is incremental and older workflows must keep working | Let a builder produce a new result or apply changes to an existing object | Gradual refactoring and coexistence of old and new workflows |
| Copying or deserialization changes object references | Separate content equality from runtime identity used by the editor | Selection, positioning, and Undo/Redo |
| Existing documents must remain readable and writable | Evolve the public API separately from the storage format | JSON, YAML, and import/export compatibility |

<a id="reading-order"></a>

## Suggested reading order

1. [Change summary][summary]: see which types changed and which models stayed mutable.
2. [Lookup refactoring walkthrough][lookup-story]: see why lookup indexes replaced `DataTable.Select(...)`.
3. [Builder refactoring walkthrough][builder-story]: compare producing a new result with updating an existing object.
4. [Performance regression analysis][regression]: see why a clearer design can still run more slowly.
5. [Refactoring plan][plan] and [full development notes][live-docs]: follow individual steps when you need more detail.
6. [Overall A/B report][overall-report] and [lookup-stage A/B report][lookup-report]: distinguish cumulative results from changes measured in one stage.

Most source comments, development notes, and benchmark reports are in Chinese. The three README files provide the same reading route and a summary of the decisions; the original materials remain in their original language. Chinese benchmark text is conversion input, not documentation to translate.

<a id="source-code"></a>

## Source code entry points

| File | What to look for |
| --- | --- |
| [XmlBrailleTable.cs][xml-table] | XML loading, index construction, and lookup |
| [BrailleWordBuilder.cs][word-builder] | The different roles of `Build()`, `ToBrailleWord()`, and `ApplyTo(...)` |
| [BrailleLine.cs][line-model] and [BrailleWord.cs][word-model] | Mutable models, read-only views, mutation methods, and runtime identity |
| [BrailleWordBuilderTest.cs][builder-test] | Checks for builder and result behavior |
| [YamlSerializationTests.cs][yaml-test] | Tests for YAML read/write compatibility |
| [BrailleConversionBenchmarks.cs][benchmark-code] and [TestData][test-data] | Measured operations and input data |

The builder and some result interfaces are internal types. The appendix snippets explain code inside the project; they are not APIs you can call by pasting them into a separate application. `Build()` creates a new result. `ApplyTo(...)` mutates the existing object passed to it.

<a id="run"></a>

## Run the tests and benchmarks

Install Git and the .NET 10 SDK, with access to the required NuGet packages. The commands use the actual paths in the public repository and run the core library tests without launching the desktop editor.

Links and checkout are pinned to [reference snapshot][snapshot] `c513c9f1fc8badb1bb843490fee60e57844e4e44`. This keeps the guide stable as the branch changes. The snapshot is a reference for this guide, not the candidate version in the historical reports below.

```sh
git clone --branch immutable-design https://github.com/braillekit/text-to-braille.git
cd text-to-braille
git checkout --detach c513c9f1fc8badb1bb843490fee60e57844e4e44

dotnet test src/EasyBrailleEditApp/BrailleToolkit.Tests/BrailleToolkit.Tests.csproj -c Release

dotnet run --project src/EasyBrailleEditApp/BrailleToolkit.Benchmarks/BrailleToolkit.Benchmarks.csproj -c Release -- --filter "*BrailleConversionBenchmarks*"
```

BenchmarkDotNet takes time to run. Results depend on hardware, OS, SDK, and the execution environment. These commands measure the reference snapshot only; they do not automatically run a before-and-after comparison.

<a id="benchmarks"></a>

## Check the measurements cited in the book

| Comparison | Baseline commit | Candidate commit |
| --- | --- | --- |
| [Cumulative refactoring results][overall-report] | `ba9bc71d6b33b00dbe04de957c5acc2dc07e576d` | `279355533346a66352230062c89e30a5637cacf1` |
| [Lookup and index refactoring][lookup-report] | `dcf71ef2d0aba531b6d724628234bec7d08168e1` | `8251b2df5bc25d0b4bb67826e3745cbc64f59b2a` |

To reproduce a historical comparison, follow the report: use separate worktrees, identical input data, and the same benchmark harness. The cumulative comparison starts before the benchmark project existed. Its report explains how the harness and test data were added without changing the original production code. Comparing an arbitrary older version with the current branch is a different experiment.

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
