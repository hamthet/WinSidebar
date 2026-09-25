# Continue development here — current WinSidebar 2.0 state

This file is the current internal entry point on `develop`. The older dated plans, reviews and verification matrices remain useful historical evidence, but they are not the current product status.

## Current authority

1. `development/README.md` — current internal status and branch/PR map.
2. `development/RELEASE-2.0.0-INTEGRATION-2026-09-24.md` — integration history plus the current 2.0 closure note.
3. Shipping branch `chore/release-2.0` — current clean product/repository candidate.
4. On that shipping branch: `PROJECT.md`, `project.json`, `docs/DEVELOPMENT.md`, `docs/RELEASE.md`, source and executable smoke tests.

## Current state — 2026-09-25

- WinSidebar 2.0 runtime behavior is frozen and explicitly owner-approved.
- Active clean shipping branch: `chore/release-2.0`.
- Current shipping head: `a156c3fff7497fc7f33c262eb3d20f029818d59e`.
- Draft PR #7 targets `main`; live comparison is 28 commits ahead / 0 behind at this checkpoint.
- The current head changes public documentation and release verification only; it does not modify `src/**`, runtime localization catalogs or persisted-data contracts.
- The canonical verifier now requires the complete five-language public documentation/artwork set and requires the successful OutputRoot to be exactly flat: EXE, ZIP, checksums, LICENSE and five START-HERE files, with no technical subdirectories or stale unrelated content.
- User-facing docs are separated from maintainer/AI documentation. Internal `development/**` remains excluded from shipping `main` and the ZIP.

## Current release gate result

The canonical Windows release gate was executed on artifact-producing head `72a6a76f58d67751abdb2e4bb8025dcefafde11f` and returned:

- `WINSIDEBAR 2.0 RELEASE VERIFICATION: PASS`;
- .NET SDK `8.0.425`;
- flat OutputRoot under `artifacts\release`;
- generated `WinSidebar.exe`, `WinSidebar-v2.0-win-x64.zip` and `SHA256SUMS.txt`.

This PASS validates all artifact-producing inputs. The only later release-branch commit is `a156c3fff7497fc7f33c262eb3d20f029818d59e`, which changes only `.github/workflows/build.yml` so the manual workflow uploads the verified flat `artifacts\release\` directory instead of the now-transient/removed `dist\` directory. Runtime, verifier and package inputs are unchanged. Runtime feature acceptance remains the previously recorded owner approval and is not being reopened.

Final PR #7 diff review is complete with no release blocker found, PR #7 is ready for review, and the owner has opened the generated `WinSidebar.exe` successfully after the canonical PASS. All known technical release gates are closed. Merge to `main` and GitHub Release publication remain separate explicit actions. Merge to `main`, tag `v2.0` and GitHub Release publication remain separate actions.

## Historical material

`PLAN.md`, `PRODUCT-REVIEW.md`, `VERIFICATION.md`, `CODE-STATUS.md`, `FEATURE-STATUS.md`, dated logs and old feature PR notes describe earlier development stages. Preserve them for provenance; do not infer current implementation status from their old `PLANNED`, `NOT RUN` or pre-localization statements.


## 2026-09-25 — post-ready automated review reopened runtime gate

After PR #7 was marked ready, a fresh automated review of head `a156c3fff...` found additional valid runtime/data-safety issues, including a P1 focus race in snippet paste injection. The pre-fix state is preserved at `checkpoint/pre-final-review-fixes-20260925`.

Corrective release head: `10e8aaf78b495b71bb6b26fd77ceea23252e9c11`.

The commit changes runtime/source behavior to:
- recheck snippet target focus immediately before `SendInput`;
- clean up partially injected Ctrl/V key-down events;
- preserve files when rollback snapshot capture itself fails;
- use a nonlocalized physical Downloads fallback;
- validate ignored-app persistence symmetrically, catch case collisions and allow safe UI reset with backup;
- suppress preference writes when an existing `settings.ini` was unreadable at startup;
- correct the sidebar tooltip to AltGr+Y;
- localize generated default snippet names.

Localization catalogs now contain 152 keys with exact five-language parity. Prior review threads were resolved after the fixes and a fresh `@codex review` was requested.

The owner already authorized merge and release publication, but the runtime-changing commit invalidates the previous artifact gate for merge purposes. Current required gate: run `.\tools\verify-release.ps1` on Windows at `10e8aaf78...`, then open the resulting EXE once. Only after that current-head PASS may the authorized merge proceed.
