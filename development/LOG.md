# Engineering journal — append-only

A narrative of **observed work**, not a prediction of work to be done. Entry fields: date/time/timezone, actor, baseline SHA, task/requirement ID, files changed, actual result, tests with exact evidence, known risks, PR/commit SHA and next task. Append an entry for each substantive implementation, test, correction and release. Keep status aligned with evidence; do not erase failed attempts or rewrite unverified work as passed.

## 2026-09-22 — documented starting state

- Source read: `src/WinSidebar.cs`, `src/ShortcutConfig.cs`, `WinSidebar.csproj`, `.github/workflows/build.yml`, `README.md`, original/English SVG sources and tutorial. Starting `main` SHA: `362bda88b7e9da26fa8b3200fda6ce73478d3f2d`. Repository initially had `main` and `fix/complete-english-localization`; no `develop` branch was found.
- Earlier documentation-first development (Git history, not a five-language binary): on `main`, English `README.md`, English tutorial/outreach and English artwork SVG/PNG were added, while program code and ZIP remained Portuguese. Artwork render workflow and prior Windows publish job were reported `success` for their **earlier** corresponding runs, not current product tests. See `BASELINE.md` for source evidence. The mismatch was reported by user, and translation-first was rejected.
- Side branch `fix/complete-english-localization` contains additional partial English guides/draft docs; not merged into `develop`, because existing branch content does not demonstrate runtime translation and its ordering is superseded. Preserve in Git history as a reference; reuse end-user text only *after* product verification.
- Corrected user direction: first translate the **product** with real multi-language support, then the repository; planning and journal stay on development branch and out of final product.

## 2026-09-22 — development dossier creation

- Created branch `develop` from `main` using GitHub. First development-document commit: [`da6c09f`](https://github.com/hamthet/WinSidebar/commit/da6c09f4a089bd8183f534a5f9f2e832e7181c00) (`development/README.md`).
- Added source-grounded baseline: [`16880b1`](https://github.com/hamthet/WinSidebar/commit/16880b1951fff3bc05916abe82ff2af13e3c64ce) (`development/BASELINE.md`).
- Added corrected implementation plan: [`0c18bef`](https://github.com/hamthet/WinSidebar/commit/0c18bef800f6ec13ac8a8ac5565c742f65670105) (`development/PLAN.md`).
- Added product-design assessment: [`ea20859`](https://github.com/hamthet/WinSidebar/commit/ea208599e1b5f992094d983352e9554e64b4da77) (`development/PRODUCT-REVIEW.md`).
- Added verification/release gates: [`4c674f4`](https://github.com/hamthet/WinSidebar/commit/4c674f4229535ddea9dfca60cbdb2a064a891f65) (`development/VERIFICATION.md`).
- Added decision register: [`ff7f9cc`](https://github.com/hamthet/WinSidebar/commit/ff7f9cc9abd9e69e31245c5177e7d1ace126743c) (`development/DECISIONS.md`).
- **Tests during dossier creation:** repository reads and GitHub write responses; no Windows build, manual UI run, locale-parity test or five-language implementation. Prior CI successes are not counted again here. Product completion remains `NOT VERIFIED`.

## 2026-09-22 — eliminate obsolete plan from default branch and dev copy

- Removed the old documentation-first `docs/i18n/LOCALIZATION-PLAN.md` from `main` in [`f291352`](https://github.com/hamthet/WinSidebar/commit/f291352b7456227d221b3708fd8a283925dc3143), so the default branch no longer carries that planning document. The earlier baseline correctly records that it **was** present at the starting SHA.
- Removed the inherited obsolete copy from `develop` in [`1b7ebb4`](https://github.com/hamthet/WinSidebar/commit/1b7ebb43356c464bc55f47351b8f07f3df9f905c). Its history remains in Git; the self-contained current plan is `development/PLAN.md`.
- **Test evidence:** GitHub file deletion responses succeeded; confirm eventual source-tree and final package absence again at release (`CLEAN-01`). No executable/source implementation changed, and this does not establish a product build or a five-language release.

## 2026-09-22 — later accepted requirements and window-management prototype

- The owner clarified that only actual Alt+Tab-like windows should appear, including genuinely opened Calculator/Settings windows; previous exclusions were a workaround for phantom entries. Accepted separate live-window aliases, reversible persisted per-app ignore rules, `ATALHOS` heading plus Save/Restore controls. Existing preference reliability is owner-approved; cross-version migration testing is OUT OF SCOPE. The owner will test the completed five-language UI.
- Requirements, implementation and evidence are documented in [`CHANGE-REQUEST-2026-09-22.md`](CHANGE-REQUEST-2026-09-22.md), [`LOG-2026-09-22-IMPLEMENTATION.md`](LOG-2026-09-22-IMPLEMENTATION.md) and [`CODE-STATUS.md`](CODE-STATUS.md). Windows feature compile passed in [CI run 35756515790](https://github.com/hamthet/WinSidebar/actions/runs/35756515790); source is in [draft PR #1](https://github.com/hamthet/WinSidebar/pull/1), not `develop` or `main`.
- **NOT RUN:** actual Alt+Tab parity, phantom-window test, five Calculator alias workflow, ignore/undo usability, restore rollback and Windows DPI/keyboard acceptance. A successful publish is not a behavior test.

## 2026-09-22 — English/Portuguese runtime implementation, source parity and smoke tests

- Starting from `feature/window-management`, created [`feature/runtime-i18n-en`](https://github.com/hamthet/WinSidebar/tree/feature/runtime-i18n-en). Added embedded `i18n/catalog.json`, `src/Localization.cs`, and localized application-owned strings in `src/WinSidebar.cs`, `src/ShortcutConfig.cs`, `src/WindowManagement.cs`; localized icon display labels while keeping XML icon IDs stable; added English/Portuguese context-menu selection, in-place UI update and a saved `language=` preference. Prior settings without a language field retain Portuguese; new profiles select English fallback or supported Windows language. Source integration was performed through an anchored development script, and its one-shot CI workflow was retired after successful source compilation.
- [Windows single-EXE publish and source integration: PASS](https://github.com/hamthet/WinSidebar/actions/runs/35758403513). [Catalog parity and executable smoke tests: PASS](https://github.com/hamthet/WinSidebar/actions/runs/35758684168): 114 referenced source keys / 115 bilingual entries, 354 runtime checks passed, full WinForms build 0 warnings / 0 errors. A heuristic untranslated-literal report found only the native `Português (Brasil)` option label.
- Opened dependent [draft PR #2](https://github.com/hamthet/WinSidebar/pull/2), targeting PR #1's feature branch, not `main`. Full implementation and evidence are self-contained in [`RUNTIME-I18N-EN-2026-09-22.md`](RUNTIME-I18N-EN-2026-09-22.md). The branch includes ordinary smoke tests and development-only script/report; development-only material must be excluded from final product.
- **NOT RUN / NOT APPROVED:** live WinForms English/Portuguese visual and accessibility testing, actual language switch/restart, every error/modal path, Windows 10/11 and scaling, Calculator/Settings phantom-window behavior. Three further locale catalogs and user acceptance are pending. The published v1.0.0 remains Portuguese. No cross-version migration test was run, by owner decision.

## Stage board (update only with evidence)

| Stage | Status | Evidence needed |
| --- | --- | --- |
| Branch and self-contained development documentation | DOCUMENTED | Dossier and implementation evidence in `develop` |
| Core functional inventory and engineering design | PARTIAL | Full Alt+Tab scope/behavior evidence |
| Runtime localization infrastructure | COMPILED + SMOKE PASS | Live Windows GUI and persistence checks |
| English and Portuguese runtime | IMPLEMENTED + AUTOMATED PASS; UI NOT RUN | Owner/Windows UI verification |
| Spanish runtime | NOT STARTED | Locale and UI evidence |
| Russian runtime | NOT STARTED | Locale and UI evidence |
| Simplified Chinese runtime | NOT STARTED | Locale and UI evidence |
| Window-management features | PROTOTYPE COMPILED; UI NOT RUN | Phantom/alias/ignore/save-reset acceptance |
| Product hardening / end-user validation | NOT STARTED | P0/P1 review + test evidence |
| Five-language repository/docs/artwork | PARTIAL, PREMATURE | Revalidate/revise only after product gates |
| New artifact / final release | NOT STARTED | Version, ZIP manifest, SHA256, end-to-end verification |

## Follow-up entry template

```
### YYYY-MM-DD HH:MM TZ — concise action
Baseline branch/SHA:
Requirement/test IDs:
Files and concrete change:
Why / decision ID:
Executed commands and observed result:
Evidence (CI run, screenshots, PR, commit):
Regressions and data-compatibility result:
Open issues / next action:
```

**Recording rule:** If a task has only a proposed design or an unexecuted command, mark it `PLANNED`/`NOT RUN`; if a tool fails, include failure and mitigation. Documentation of future development is a procedure, not evidence that future steps happened.
