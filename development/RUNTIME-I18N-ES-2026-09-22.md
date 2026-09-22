# Spanish runtime localization — 2026-09-22

**Scope:** product first, `es-ES` only; no Spanish README/artwork/distribution translation at this stage. This is development-only documentation and must never enter `main` or the release ZIP. Baseline: [`feature/runtime-i18n-en` at `875977b31e150d2b631b43928e979dd8b29b0f53`](https://github.com/hamthet/WinSidebar/commit/875977b31e150d2b631b43928e979dd8b29b0f53), including the window-management changes from draft PR #1 and English/Portuguese runtime from draft PR #2.

## Source and implementation

- Work branch: [`feature/runtime-i18n-es`](https://github.com/hamthet/WinSidebar/tree/feature/runtime-i18n-es), created from the English feature head; it must be reviewed/integrated after PR #2, which depends on PR #1. Do not merge into `main` or release prematurely.
- Added `i18n/es-ES.json` with one Spanish string per key in the existing 115-entry application catalog; all application-owned labels, accessible names, dialogs, shortcut defaults, validation errors, window rename/ignore controls and restore/save preferences belong to this coverage. Windows-owned/system strings, user-defined names, external window titles, icon identifiers, URLs and process identities are not translated.
- Updated `src/Localization.cs` to embed and merge Spanish translations with the original English/Portuguese catalog; fail closed on missing/extra/empty Spanish entries; allow `es-ES` in `Select`, settings restore and supported Spanish Windows UI locales; default English for unsupported UI locales; retain Portuguese for existing settings without `language=`.
- Updated `WinSidebar.csproj` and `tests/LocalizationSmoke.csproj` to embed `i18n/es-ES.json` inside both executables. Updated smoke tests for English, Portuguese and Spanish, including `es-ES`, `es-MX`, persisted language and rejection of not-yet-implemented Russian.
- The sidebar/tray language menu needs the native `Español` option, its callback and exclusive check mark. One-shot anchored integration script `development/apply-runtime-i18n-es.py` and temporary branch-only Action `.github/workflows/runtime-i18n-es.yml` exist to apply this change, execute runtime smoke checks and publish a **development preview**. The workflow must be removed after success; CI must have read-only permissions for ongoing tests. No migration tests, per owner.

## Evidence and release gates

- [Spanish integration workflow](https://github.com/hamthet/WinSidebar/actions/runs/35763873230). **Verify the final run conclusion and job steps before reporting PASS**; check the committed `src/WinSidebar.cs` and executable artifact. An Action being queued or running is not evidence of success.
- Unverified until evidence: parity count; smoke check count; portable Windows EXE; artifact availability; interactive Spanish UI in all dialogs at 100–200% DPI; actual Windows Alt+Tab/Calculator phantom behavior; five-calculator renaming, ignored applications, Save/Restore; English/Portuguese regressions. Owner personally validates the five language UIs only after all locales are implemented.
- Neither English-only README nor historical Portuguese v1.0.0 demonstrates Spanish support. Repository translation and packaging are separate **later** gates. Keep this dossier out of the shipping product, and preserve the user's scope exclusion of cross-version migration tests.
