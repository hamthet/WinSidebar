# WinSidebar — self-contained development dossier

**Repository:** `hamthet/WinSidebar` · **integration branch:** `develop` · **established:** 2026-09-22. Dossier language: English; five product UI locales.

This directory records source-grounded facts, product scope, decisions, implementation and observed verification. It belongs only on the public development branch, **not** in final `main` or the end-user ZIP. Never put secrets, customer data, machine-private paths or unredacted logs here. Another contributor should be able to continue from this directory and the repository without relying on chat history. A plan describes future work; it does not prove its implementation.

## Read in order and authority

1. [`BASELINE.md`](BASELINE.md) — the original source/CI snapshot and previous documentation-first failure.
2. **[`CHANGE-REQUEST-2026-09-22.md`](CHANGE-REQUEST-2026-09-22.md) — ACCEPTED newer owner requirements: Alt+Tab-like genuine windows; remove hard-coded Calculator/Settings exclusions while still preventing phantom windows; distinct per-window aliases; reversible ignored-app rules; localized Atalhos header with Restore defaults and Save preferences; owner-approved existing preference behavior and no cross-version migration test. This supersedes conflicting claims or tests in older documents.**
3. [`ENGINEERING-NOTES.md`](ENGINEERING-NOTES.md) — implementation hazards and non-assumptions for that change request.
4. [`PLAN.md`](PLAN.md) — product-first five-language architecture, implementation and release plan, **subject to the newer change-request overrides**.
5. [`PRODUCT-REVIEW.md`](PRODUCT-REVIEW.md) — dated design baseline; its old Calculator/Settings question and preference risk are superseded as described above.
6. [`VERIFICATION.md`](VERIFICATION.md) — test matrix, extended by WM-01..06, SP-01..03 and I18N-01 in the newer change request. Migration tests are `OUT OF SCOPE`, not `PASS`.
7. [`DECISIONS.md`](DECISIONS.md) — decision history; ADR-007 and portions of ADR-009 are superseded by the accepted change request.
8. [`LOG.md`](LOG.md) — chronological, append-only evidence of actual commits, runs, corrections and remaining work.

**Conflict rule:** dated, accepted owner requirements take precedence over older proposals. Keep superseded rationale: Calculator/Settings were filtered to suppress *phantom windows*, not because genuine open windows should be excluded. A new feature is not verified merely because a requirement or helper script exists.

## Implementation and release order

1. Record a reproducible Windows baseline and compare actual listed windows with Alt+Tab; inspect the phantom-window cause.
2. Implement window aliases, ignored applications and their reachable manager; implement localized heading and functional Save/Restore controls while preserving the already owner-approved existing preferences.
3. Implement five complete **runtime** UI localizations (en-US, pt-BR, es-ES, ru-RU, zh-CN), including all new menu/dialog/button/accessibility text and locale resources embedded in one portable EXE. Owner will personally test the five-language UI; development/CI still owns automated parity, persistence and regression tests.
4. Verify five Calculator windows, window close/reopen, phantom-window absence, ignore/undo, preferences save/restore, monitor/hotkey behavior, DPI and keyboard access against the actual built artifact.
5. Only after the product passes, finish five complete repository READMEs, tutorials, localized illustration sources and PNGs, in-ZIP instructions and a new checksummed release. The historical v1.0.0 executable is Portuguese; never relabel or overwrite it.

Log for each change: baseline SHA, requirement ID, files, behavior, exact commands/results, CI/commit evidence, remaining risks and next work. `develop` is the integration branch; feature branches may target it. Do **not** merge `develop` wholesale into final `main`: create a clean release branch from approved `main` and selectively promote only shipping source, resources, production tests/workflows, end-user docs, artwork and license. Review the final Git tree and ZIP separately.

Status terms: `ACCEPTED`, `PROPOSED`, `OPEN`, `CONFIRMED` (source/tool evidence), `PASS` (executed test with evidence), `NOT RUN`, and `OUT OF SCOPE`. Owner approval of existing preferences does not prove new Save/Restore/ignore persistence.