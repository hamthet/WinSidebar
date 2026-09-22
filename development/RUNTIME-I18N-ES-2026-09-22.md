# Spanish runtime localization — engineering record, 2026-09-22

**Scope:** add Spanish (`es-ES`) to the **running product**, not merely to the README. Development-only dossier: never merge `development/**` into production `main` or ship it in the end-user ZIP. Base: [`feature/runtime-i18n-en` commit `875977b`](https://github.com/hamthet/WinSidebar/commit/875977b31e150d2b631b43928e979dd8b29b0f53), which depends on the window-management draft PR #1. English/Portuguese runtime is draft PR #2; Spanish is the next dependent stage. No new v1.0.0 release.

## Changes implemented on `feature/runtime-i18n-es`

- New `i18n/es-ES.json`: 115 nonempty Spanish texts with keys exactly matching the existing 115-entry English/Portuguese base catalog. Includes sidebar/menu labels, keyboard/tooltips/accessibility, shortcut editor, defaults, errors, window aliases and ignored-app manager, plus Save/Restore controls. Do not translate user-defined names, actual Windows window titles, URLs, filesystem paths, process identities or XML icon IDs.
- Updated `src/Localization.cs` to merge the Spanish resource from the assembly with strict key parity and fail-closed lookup; Windows Spanish UI locales (`es-ES`, `es-MX`, etc.) choose Spanish on first launch, unsupported languages still choose English, existing settings without a language key still retain Portuguese, `language=es-ES` is supported. Both runtime EXE and smoke test executable embed Spanish as a resource; no external JSON files required by the user.
- Updated `src/WinSidebar.cs` to show a native `Español` option beside English and Português, call `ChangeLanguage("es-ES")`, and maintain the corresponding checked state. The existing language refresh method applies to all three catalogues, including new rename/ignore controls; the current in-place selection still needs interactive testing.
- Updated `tests/LocalizationSmoke.cs`/`.csproj` for exactly three installed languages, resource parity, Spanish Windows detection (including `es-MX`), persisted locale selection and rejection of not-yet-implemented `ru-RU`.
- Added a read-only ongoing `.github/workflows/localization-tests.yml` Spanish parity + runtime smoke + WinForms build gate. One-shot write-enabled workflow and its integration script were **removed** from the Spanish feature branch after they succeeded; their execution is preserved in Git/Actions history, not in the shipping source.

## Executed evidence

- [Integration workflow run `35763873230`](https://github.com/hamthet/WinSidebar/actions/runs/35763873230): **PASS**, final successful run. Anchored source changes passed; **115 Spanish catalog keys**, **589 runtime localization smoke checks passed**; `dotnet publish WinSidebar.csproj -c Release -r win-x64 --self-contained true -o publish` succeeded; a single WinSidebar.exe was verified and uploaded. The Spanish selector source commit is [`04b0984`](https://github.com/hamthet/WinSidebar/commit/04b0984cd2aae8736d74f2028d1924cfb476e775).
- [Downloadable development preview artifact](https://github.com/hamthet/WinSidebar/actions/runs/35763873230/artifacts/10710863951), `WinSidebar-es-ES-dev-preview` (GitHub Actions artifact, NOT a release, expires 2026-09-29). `WinSidebar.exe` size: 71,606,596 bytes; **EXE SHA256** `1EC9708E36A0D144637EF23AB0FF985B6E1E402F1E6CBAA84A78A0F38198F888`. GitHub's compressed artifact ZIP has a **different** SHA256: `86854614bb353092be9aaa35a68d99231ac3499bee43cbd044addffecf36bb26`; do not confuse the two.
- [Ongoing read-only locale verification run `35764083953`](https://github.com/hamthet/WinSidebar/actions/runs/35764083953): started after the permanent CI workflow update. Its final result must be checked separately; do not count it as passed until observed.

## Unverified release gates

- Manual Windows 10/11 review of all three interfaces, especially Spanish text truncation on the 211 px width, high DPI, system fonts and translated dialogs/tooltips/accessibility; owner will validate **all five** after remaining language stages.
- Alt+Tab match: genuine Calculator/Settings windows listed when open, no phantom entries when closed; five independently renamed Calculator windows; ignore app and undo; Save/Restore + failure behavior; existing keyboard and multi-monitor functionality.
- Russian and Simplified Chinese product localizations, end-user README/tutorial/illustration translations, in-ZIP instructions, release version/tag/hash and product acceptance. The existing `main` and historical v1.0.0 remain untouched. Cross-version migration tests are explicitly **out of scope** by owner decision.
