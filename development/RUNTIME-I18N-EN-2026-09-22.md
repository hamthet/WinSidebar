# English runtime localization — evidence and handoff (2026-09-22)

**Status: ENGINEERING CHECKPOINT; NOT RELEASED OR OWNER-APPROVED.** This is a development-only note. It must not be copied into final `main` or end-user ZIP. Source-of-truth requirements: [`CHANGE-REQUEST-2026-09-22.md`](CHANGE-REQUEST-2026-09-22.md), [`PLAN.md`](PLAN.md), [`VERIFICATION.md`](VERIFICATION.md). Initial code starts from `feature/window-management`; the English implementation is in [`feature/runtime-i18n-en`](https://github.com/hamthet/WinSidebar/tree/feature/runtime-i18n-en) and draft [PR #2](https://github.com/hamthet/WinSidebar/pull/2), whose base is the feature branch for draft [PR #1](https://github.com/hamthet/WinSidebar/pull/1), **not** `main`.

## Implementation actually committed

- `src/Localization.cs`: a centralized runtime lookup using stable descriptive keys; `i18n/catalog.json` embedded in the portable EXE via `WinSidebar.csproj`. Portuguese (`pt-BR`) and English (`en-US`) are the only selectable languages at this stage. Missing keys fail rather than silently displaying an unrelated language.
- `src/WinSidebar.cs`, `src/ShortcutConfig.cs`, `src/WindowManagement.cs`: application-owned strings for main controls, errors, dialogs, tray menu, window rename/ignore management, shortcut editor and default names route through the catalog. The language selection appears in the shared tray/sidebar context menu and updates extant controls. Individual user names, external window titles, stable XML/icon type tokens, process identities, URLs and paths must never be translated.
- A profile with no settings file chooses a supported Windows UI language (Portuguese/English) or English fallback; existing `settings.ini` without a `language=` field retains Portuguese. A manual selection is saved alongside existing settings. Default shortcut names are generated in the chosen language for new/reset defaults; existing saved names are not rewritten.
- `development/apply-runtime-i18n-en.py` was used as an anchored, one-shot source-integration script, preserved only for development history. Its temporary write-enabled workflow was **removed** after a successful build; the continuing validation workflow has read-only permissions. Do not reapply the one-shot script to transformed source.
- The initial English catalog covers all mapped user-facing literals in the three source files; the heuristic report `development/i18n-untranslated-candidates.txt` identifies only `Português (Brasil)`, which is intentionally the native-language label of the language option. This heuristic does not replace a human UI audit.

## Verification actually executed

1. [Anchored integration and Windows self-contained publish — PASS](https://github.com/hamthet/WinSidebar/actions/runs/35758403513): source integration, resource validation, single-EXE publish and bot commit to the feature branch completed successfully. The old `LEIA-ME.txt` / v1.0.0 release was **not** replaced or translated by this build.
2. [Runtime localization audit and executable smoke test — PASS](https://github.com/hamthet/WinSidebar/actions/runs/35758684168): **114 source-referenced localization keys**, **115 complete English/Portuguese catalog entries**; **354 executable checks** for embedded resource lookup, exact translation values, fallback/initial-selection behavior, prior settings without language, saved language, missing key and unsupported language. Full WinForms `dotnet build -c Release`: **0 warnings, 0 errors** on the Windows Actions runner. These are test observations, not forecasts.
3. Source review: the candidate untranslated-literal report contains no identified Portuguese-language UI literal other than the deliberate native locale label. This is a textual audit, **not** evidence of full layout/accessibility coverage or external Windows-native dialog translations.

## Still OPEN / NOT RUN

- Execute a built WinSidebar interactively on Windows and verify all English and Portuguese UI surfaces (menus, tooltips, editor, error dialogs, accessibility, 100–200% display scaling), manual language change and a real restart. The user will test the complete five-language UI when available; do not represent automated tests as their approval.
- Validate Alt+Tab parity and absence of phantom Calculator/Settings windows; independently rename five live Calculator windows; ignore/unignore an app; inspect reset/save rollback and any icon/browser regressions. PR #1 remains draft pending this verification.
- Spanish, Russian and Simplified Chinese runtime catalogs, translation/layout tests, five-language package quick starts, final README/tutorials/artwork and a new versioned release **have not been delivered**. Preserve the historical Portuguese v1.0.0 release and do not advertise it as English.
- Existing preference behavior is owner-approved; **cross-version migration tests are OUT OF SCOPE by explicit instruction**, not PASS. New language persistence and Save/Restore functionality still require their own functional tests.

## Next engineering handoff

Review PR #2 against PR #1 and run manual English/Portuguese UI tests; record actual behavior and failures in `LOG.md` and `VERIFICATION.md`. Only after this checkpoint is evaluated should the next locale be added. Promote production source/resources/tests selectively to a clean release branch after all five locales pass; retain the development directory, scripts and this note solely in development history.
