# Engineering journal — append-only

A narrative of **observed work**, not a prediction of work to be done. Entry fields: date/time/timezone, author/actor, baseline SHA, task/requirement ID, files changed, actual result, tests with exact evidence, known risks, PR/commit SHA and next task. Append an entry for each substantive implementation, test, correction and release. Keep the current status summary aligned with evidence; do not erase failed attempts or rewrite unverified work as passed.

## 2026-09-22 — documented starting state

- Source read: `src/WinSidebar.cs`, `src/ShortcutConfig.cs`, `WinSidebar.csproj`, `.github/workflows/build.yml`, `README.md`, original/English SVG sources and tutorial. Baseline `main` SHA: `362bda88b7e9da26fa8b3200fda6ce73478d3f2d`. Repository initially had `main` and `fix/complete-english-localization`; no `develop` branch was found.
- Earlier documentation-first development (Git history, not a five-language binary): on `main`, English `README.md`, English tutorial/outreach and English artwork SVG/PNG were added, while program code and ZIP remained Portuguese. Artwork render workflow and a prior Windows publish job were reported `success` for their **earlier** corresponding runs, not current product tests. See `BASELINE.md` for source evidence. The mismatch was reported by user, and translation-first was rejected.
- Side branch `fix/complete-english-localization` contains additional partial English guides/draft docs; not merged into `develop`, because existing branch content does not demonstrate runtime translation and its ordering is superseded. Preserve in Git history as a reference, cherry-pick useful end-user text only *after* product verification.
- Corrected user direction: 'first translate the product — meaning actual multi-language support — then the repository'; all planning and engineering notes must remain on development branch, out of final product.

## 2026-09-22 — development dossier creation

- Created branch `develop` from `main` using GitHub. First development-document commit: [`da6c09f`](https://github.com/hamthet/WinSidebar/commit/da6c09f4a089bd8183f534a5f9f2e832e7181c00) (`development/README.md`).
- Added source-grounded baseline: [`16880b1`](https://github.com/hamthet/WinSidebar/commit/16880b1951fff3bc05916abe82ff2af13e3c64ce) (`development/BASELINE.md`).
- Added corrected implementation plan: [`0c18bef`](https://github.com/hamthet/WinSidebar/commit/0c18bef800f6ec13ac8a8ac5565c742f65670105) (`development/PLAN.md`).
- Added product-design assessment: [`ea20859`](https://github.com/hamthet/WinSidebar/commit/ea208599e1b5f992094d983352e9554e64b4da77) (`development/PRODUCT-REVIEW.md`).
- Added verification/release gates: [`4c674f4`](https://github.com/hamthet/WinSidebar/commit/4c674f4229535ddea9dfca60cbdb2a064a891f65) (`development/VERIFICATION.md`).
- Added decision register: [`ff7f9cc`](https://github.com/hamthet/WinSidebar/commit/ff7f9cc9abd9e69e31245c5177e7d1ace126743c) (`development/DECISIONS.md`).
- **Actual tests this entry:** repository reads and GitHub write responses; no Windows build, manual UI run, resource parity test or five-language runtime implementation performed for this dossier. The previous CI successes must not be counted again here. Product completion remains `NOT VERIFIED`.

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
