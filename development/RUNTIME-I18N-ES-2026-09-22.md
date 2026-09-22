# Spanish runtime localization — engineering record, 2026-09-22

**Scope:** add Spanish (`es-ES`) to the running product, NOT merely the README. Development-only dossier: never merge `development/**` into production `main` or ship it in the end-user ZIP. Baseline: [`feature/runtime-i18n-en` commit `875977b`](https://github.com/hamthet/WinSidebar/commit/875977b31e150d2b631b43928e979dd8b29b0f53), including window-management PR #1 and English/Portuguese runtime PR #2. Spanish: [draft PR #3](https://github.com/hamthet/WinSidebar/pull/3), based on `feature/runtime-i18n-en`; neither draft is approved for release.

## Changes implemented on `feature/runtime-i18n-es`

- `i18n/es-ES.json`: 115 nonempty Spanish texts with keys exactly matching the existing 115-entry English/Portuguese catalog. Application-owned labels, dialogs, shortcut defaults, validation/errors, tooltips/accessibility, per-window aliases, ignored-app manager, and Save/Restore controls are covered. Do not translate user-defined names, OS window titles, URLs, paths, process identities or XML icon IDs.
- `src/Localization.cs` embeds and merges the Spanish resource with strict key parity and fail-closed lookup; Spanish Windows UI locales (`es-ES`, `es-MX`, etc.) choose Spanish on new profiles, unsupported locales default to English, and existing settings without `language=` remain Portuguese. Saved `language=es-ES` is accepted. Both the production and smoke executables embed the Spanish resource, without requiring external JSON at runtime.
- `src/WinSidebar.cs`: native `Español` option beside English and Português, `ChangeLanguage("es-ES")` callback and corresponding checked state; existing in-place refresh translates the new window-management controls. Source commit: [`04b0984`](https://github.com/hamthet/WinSidebar/commit/04b0984cd2aae8736d74f2028d1924cfb476e775).
- `tests/LocalizationSmoke.cs`/`.csproj`: 3-locale catalogue checks, resource parity, Windows `es-ES`/`es-MX` detection, persistent choice and rejection of unimplemented `ru-RU`.
- `.github/workflows/localization-tests.yml`: ongoing **read-only** source-key/Spanish parity, smoke and WinForms compile checks. The temporary write-enabled integration workflow and its one-shot source-editing script were deleted from this feature branch after successful application. Their execution remains in Git and Actions history, not the final product.

## Executed verification (PASS)

1. [Integration and portable Windows publish, run `35763873230`](https://github.com/hamthet/WinSidebar/actions/runs/35763873230): all job steps completed successfully; **115 Spanish catalog keys**, **589 runtime localization smoke checks**, self-contained `dotnet publish` and developer artifact upload. The integration script asserted exact source anchors before the new selector was committed.
2. [Permanent read-only CI, run `35764083953`](https://github.com/hamthet/WinSidebar/actions/runs/35764083953): final `success`; **114 source-referenced translation keys**, **115 complete English/Portuguese/Spanish entries**, **589 runtime checks**, WinForms Release build with **0 warnings, 0 errors**. This is code/catalog validation, not interactive GUI acceptance.
3. [Developer-preview EXE artifact](https://github.com/hamthet/WinSidebar/actions/runs/35763873230/artifacts/10710863951), `WinSidebar-es-ES-dev-preview` (GitHub Actions artifact; expires 2026-09-29, NOT a release). `WinSidebar.exe` size **71,606,596 bytes**, **EXE SHA256** `1EC9708E36A0D144637EF23AB0FF985B6E1E402F1E6CBAA84A78A0F38198F888`. The compressed ZIP has a DIFFERENT SHA256 `86854614bb353092be9aaa35a68d99231ac3499bee43cbd044addffecf36bb26`.

## Still NOT RUN / release gates

- Actual GUI testing on Windows 10/11 in English, Portuguese and Spanish: menus, dialogs, small 211 px width, DPI 100–200%, fonts, keyboard accessibility and restart/persistence. The owner will validate the five-language UI once the remaining catalogs exist.
- Alt+Tab parity: Calculator/Settings show only with genuine windows open; no phantom windows when closed; five independent Calculator aliases, app ignore/undo, Save/Restore and failure behavior, monitors/hotkeys.
- Russian and Simplified Chinese **product** localizations; five language READMEs, translated image examples, tutorial, correct in-ZIP instructions, versioning and new checked release. Historical v1.0.0 and `main` remain unchanged. Owner explicitly excluded testing cross-version preference migrations; do not report it as PASS.
