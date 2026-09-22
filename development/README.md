# WinSidebar — self-contained development dossier

**Repository:** `hamthet/WinSidebar` · **integration branch:** `develop` · **established:** 2026-09-22. Dossier language: English; five product UI locales.

This directory records source-grounded facts, product scope, decisions, implementation and observed verification. It belongs only on the public development branch, **not** in final `main` or the end-user ZIP. Never put secrets, customer data, machine-private paths or unredacted logs here. Another contributor should be able to continue from this directory and the repository without relying on chat history. A plan describes future work; it does not prove its implementation.

## Current status

**Window-management prototype:** [`feature/window-management`](https://github.com/hamthet/WinSidebar/tree/feature/window-management) and [draft PR #1](https://github.com/hamthet/WinSidebar/pull/1). Windows compile succeeded; Alt+Tab parity, phantom windows, five simultaneous Calculator aliases and save/reset behavior are **not owner-accepted**. See [`CODE-STATUS.md`](CODE-STATUS.md) and [`LOG-2026-09-22-IMPLEMENTATION.md`](LOG-2026-09-22-IMPLEMENTATION.md).

**English runtime localization:** [`feature/runtime-i18n-en`](https://github.com/hamthet/WinSidebar/tree/feature/runtime-i18n-en) and [draft PR #2](https://github.com/hamthet/WinSidebar/pull/2), dependent on PR #1. The compiled English/Portuguese catalog and 354 executable smoke checks have passed; actual WinForms UI acceptance is still **NOT RUN**. See **[`RUNTIME-I18N-EN-2026-09-22.md`](RUNTIME-I18N-EN-2026-09-22.md)** for exact source, run links and remaining gates. Spanish, Russian, Simplified Chinese and a new release remain incomplete. Neither PR has been merged into `main`.

## Read in order and authority

1. [`BASELINE.md`](BASELINE.md) — the original source/CI snapshot and previous documentation-first failure.
2. **[`CHANGE-REQUEST-2026-09-22.md`](CHANGE-REQUEST-2026-09-22.md) — ACCEPTED owner requirements: Alt+Tab-like genuine windows; remove hard-coded Calculator/Settings exclusions while preventing phantom windows; distinct per-window aliases; reversible ignored-app rules; localized Atalhos header with Restore defaults and Save preferences; owner-approved existing preference behavior and no cross-version migration test.** This supersedes conflicting claims or tests in older documents.
3. [`ENGINEERING-NOTES.md`](ENGINEERING-NOTES.md) and [`IMPLEMENTATION-BACKLOG.md`](IMPLEMENTATION-BACKLOG.md) — technical pitfalls and ordered acceptance-driven work.
4. [`PLAN.md`](PLAN.md) — product-first five-language architecture and release plan, **subject to newer owner decisions**.
5. [`PRODUCT-REVIEW.md`](PRODUCT-REVIEW.md) — dated design baseline; old Calculator/Settings questions superseded.
6. [`VERIFICATION.md`](VERIFICATION.md) and [`RELEASE-GATE-OVERRIDE.md`](RELEASE-GATE-OVERRIDE.md) — tests extended by WM-01..06, SP-01..03 and I18N-01 in the change request; migration tests are `OUT OF SCOPE`, never `PASS`.
7. [`DECISIONS.md`](DECISIONS.md) — updated ADRs documenting accepted inclusion and preference behavior.
8. [`LOG.md`](LOG.md), [`LOG-2026-09-22-CHANGE.md`](LOG-2026-09-22-CHANGE.md), [`LOG-2026-09-22-IMPLEMENTATION.md`](LOG-2026-09-22-IMPLEMENTATION.md), [`CODE-STATUS.md`](CODE-STATUS.md), and [`RUNTIME-I18N-EN-2026-09-22.md`](RUNTIME-I18N-EN-2026-09-22.md) — chronology, compiled prototypes, automated tests and remaining verification.

**Conflict rule:** dated, accepted owner requirements take precedence over older proposals. Preserve the earlier rationale: Calculator/Settings were filtered to suppress *phantom windows*, not because genuinely open windows should be excluded. A new feature is not verified merely because a requirement, source commit or passing compile exists.

## Implementation and release order

1. Record an Alt+Tab/phantom-window baseline and compare it with the prototype on supported Windows builds; review code identity and alias semantics.
2. Complete/review window aliases, ignored-app management and functional Save/Restore controls, with keyboard/DPI/persistence regressions. The prototype compiles, but these behaviors are **not yet accepted**.
3. Validate English and Portuguese runtime UI (already implemented and automated-smoke-tested), then implement Spanish, Russian and Simplified Chinese using the same embedded catalog architecture. Owner will personally test the complete five-language UI; development/CI owns parity, persistence and regression checks.
4. Verify five Calculator windows, close/reopen, no phantom entries, ignore/undo, preferences, monitor/hotkeys and DPI/keyboard behavior against the actual artifact.
5. Only after the product passes, finish five complete repository READMEs, tutorials, localized illustration SVG/PNGs, in-ZIP instructions and a **new** checksummed release. Historical v1.0.0 is Portuguese; never relabel or overwrite it.

Log for each change: baseline SHA, requirement ID, files, behavior, exact commands/results, CI/commit evidence, remaining risks and next work. `develop` is the integration branch; feature branches may target it. Do **not** merge `develop` wholesale into final `main`: create a clean release branch from approved `main` and selectively promote only shipping source, resources, production tests/workflows, end-user docs, artwork and license. Review final Git tree and ZIP separately.

Status terms: `ACCEPTED`, `PROPOSED`, `OPEN`, `CONFIRMED` (source/tool evidence), `PASS` (executed test with evidence), `NOT RUN`, and `OUT OF SCOPE`. Owner approval of existing preferences does not prove new Save/Restore/ignore persistence.