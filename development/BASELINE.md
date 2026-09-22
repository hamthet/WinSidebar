# Baseline audit — 2026-09-22

Repository: https://github.com/hamthet/WinSidebar . This is an evidence-based snapshot, not an assertion that the program has been manually tested. **Starting main commit:** `362bda88b7e9da26fa8b3200fda6ce73478d3f2d`. `develop` was created from `main` after this commit. Existing side branch: `fix/complete-english-localization` (contains partial work; not merged as product localization). Verify branch heads again before taking any new action: they can change.

## Confirmed source state

| Area | Evidence | Observation |
| --- | --- | --- |
| Framework and delivery | [`WinSidebar.csproj`](../WinSidebar.csproj) | C# Windows Forms, .NET 8 `net8.0-windows`, `win-x64`, self-contained single-file publish. Version 1.0.0. Only `src/WinSidebar.cs` and `src/ShortcutConfig.cs` are explicitly compiled. |
| Core workflow | [`src/WinSidebar.cs`](../src/WinSidebar.cs) | Borderless collapsible sidebar, tray menu, window enumeration and activation, monitor grouping, `Shift+F1..F4` global hotkeys, widths 211/260/324, nominal height 504, four shortcut buttons, context menu and exit confirmation. |
| Shortcut editor and storage | [`src/ShortcutConfig.cs`](../src/ShortcutConfig.cs) | Four entries stored under `%LOCALAPPDATA%/WinSidebar/shortcuts.xml`, version 1, with stable internal types `folder`/`website` and icon tokens. Defaults are `Pasta local`, `Downloads`, `Acervo`, `Site`. Editor, validation, icon import, and error messages are Portuguese; icon token names are shown directly. |
| Settings | [`src/WinSidebar.cs`](../src/WinSidebar.cs) | `settings.ini` holds width, side, secondary monitor and browser settings; it has no language key. Writes can silently fail on IO/permission errors; no explicit migration/version handling is visible. |
| Language coverage | [`src/WinSidebar.cs`](../src/WinSidebar.cs), [`src/ShortcutConfig.cs`](../src/ShortcutConfig.cs) | Application-owned text, tooltips, accessible names, notifications, startup and error strings are embedded in C#. No runtime locale selector or resource catalog exists. |
| Actual package | [`.github/workflows/build.yml`](../.github/workflows/build.yml) | CI publishes an EXE, then packages only `WinSidebar.exe` + `LEIA-ME.txt`, verifies precisely these names and generates `SHA256SUMS.txt`. The included `LEIA-ME.txt` is Portuguese. No automatic run of UI interaction or language tests is defined. |
| Marketing | [`README.md`](../README.md), [`assets/i18n/en-US/`](../assets/i18n/en-US/) | English README, tutorial and illustrated English SVG/PNG assets exist, while the actual application and packaged instructions remain Portuguese. Original Portuguese illustrations remain in `assets/`. The images are conceptual, not screenshots. |
| Dev plan leaked to main | [`docs/i18n/LOCALIZATION-PLAN.md`](../docs/i18n/LOCALIZATION-PLAN.md) | Prior short planning memo is already on `main`; remove from *final* product tree during controlled release promotion. Do not silently call that tree planning-free. |
| Existing CI evidence | [Windows workflow](https://github.com/hamthet/WinSidebar/actions/workflows/build.yml), [artwork workflow](https://github.com/hamthet/WinSidebar/actions/workflows/marketing-assets.yml) | A prior Windows build and English illustration render were reported successful in this conversation. They are evidence for those specific runs only, not proof of current multi-language functionality or tested release. |
| License / security disclosure | [`LICENSE`](../LICENSE), [`README.md`](../README.md) | MIT license and unsigned-binary warning documented. Do not imply a checksum equals digital signing. No telemetry is described in source/docs; confirm with build/runtime audit before claiming it for a new version. |

## Functional risks to investigate (not confirmed bugs)

- `Snapshot()` explicitly excludes some Calculator/Settings windows by localized title and by process name; confirm the intended scope against 'find open windows', across five Windows UI languages. Never translate process names or persisted machine tokens.
- `TargetScreen()` falls back to primary if a secondary monitor is missing; validate unplug/replug, DPI changes, work-area changes and saved `secondary` preference. `Reposition()` is called on defined interactions, not evidently on every display change.
- `ShortcutStore.Write()` creates backups and preserves data on many failures, but `ShortcutStore.Read()` errors lead to in-memory defaults and an alert; a tested backup-recovery flow is not evident. Inspect before deciding what to change.
- First-run defaults and existing user-edited shortcut names are stored as strings. Naively translating persisted names would destroy user intent. Existing files need a non-destructive detection/migration strategy, including older `settings.ini` without a locale.
- `ShortcutEditor` is laid out using fixed pixel sizes. Russian/Chinese font coverage, 125–200% scaling, longer translations and keyboard-only navigation require evidence, not assertions.
- App startup and global hotkeys can fail due to an already-running instance or conflicts; check localization and actionable feedback for both cases. UI tests for screen readers are not present in the inspected build workflow.

## What happened and why the plan changed

The first implementation translated the GitHub entry page, tutorial, promotion copy and images before the actual application. A working English README pointed to a Portuguese `LEIA-ME.txt` and the Portuguese v1.0.0 executable. CI could compile this state because it did not audit locale coverage. The user identified that this was a **planning and product-boundary failure**. English documentation now explicitly discloses the actual binary language, but an English-facing repository is not evidence of an English product. This corrective baseline supersedes the old order.

Prior work on `fix/complete-english-localization` includes a draft `README.txt`, English quick-start, release checklist and other documentation; assess individual files for reuse after product implementation. Do **not** merge that branch wholesale: its primary content is documentation-first and no runtime localization is evidenced. Commit history and this baseline, rather than reconstructed claims about unobserved actions, serve as the provenance. Earlier project history beyond inspected source and commits is **not exhaustively known**; new work must be logged prospectively.

## Evidence freshness and change control

For each new work session record the actual `main`, `develop` and feature-branch SHA in `LOG.md`. A branch name is not an immutable baseline. If another contributor changes a file, reconcile before updating. Treat `NOT RUN` as distinct from `FAIL` and `PASS`. Never mark a behavior verified solely because it is present in source code.
