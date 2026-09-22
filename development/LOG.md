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

## Stage board (update only with evidence)

| Stage | Status | Evidence needed |
| --- | --- | --- |
| Branch and self-contained development documentation | DOCUMENTED | Dossier files in `develop`, path verification |
| Core functional inventory and engineering design | PARTIAL | Exhaustive string inventory + recorded design spike |
| Runtime localization infrastructure | NOT STARTED | Code, locale resource parity test, Windows compile |
| English and Portuguese runtime | NOT STARTED | Tested runtime workflows, upgrade preservation |
| Spanish runtime | NOT STARTED | Locale and UI evidence |
| Russian runtime | NOT STARTED | Locale and UI evidence |
| Simplified Chinese runtime | NOT STARTED | Locale and UI evidence |
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
