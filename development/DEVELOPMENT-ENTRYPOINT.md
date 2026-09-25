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
