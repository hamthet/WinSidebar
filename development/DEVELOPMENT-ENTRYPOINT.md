# Continue development here — current WinSidebar 2.0 state

This file is the current internal entry point on `develop`. The older dated plans, reviews and verification matrices remain useful historical evidence, but they are not the current product status.

## Current authority

1. `development/README.md` — current internal status and branch/PR map.
2. `development/RELEASE-2.0.0-INTEGRATION-2026-09-24.md` — integration history plus the current 2.0 closure note.
3. `main` — published WinSidebar 2.0 product/source state; public tag `v2.0` points to merge commit `2597fe5ddda8906fd4e2a31ab8591ed4e7f6a2a4`.
4. On `main`: `PROJECT.md`, `project.json`, `docs/DEVELOPMENT.md`, `docs/RELEASE.md`, source, executable smoke tests and the manual canonical-verifier workflow.

## Pre-publication checkpoint — 2026-09-25

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

Corrective release head: `81a80987fe37eca822bc7462355a0e1c63dbc1bc`.

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

The owner already authorized merge and release publication, but the runtime-changing commit invalidates the previous artifact gate for merge purposes. Current required gate: run `.\tools\verify-release.ps1` on Windows at `81a80987...`, then open the resulting EXE once. Only after that current-head PASS may the authorized merge proceed.


Follow-up hardening in `81a80987fe37eca822bc7462355a0e1c63dbc1bc` removes the remaining `File.Exists` ambiguity: settings initialization now distinguishes truly missing files from present-but-unreadable state, and rollback snapshots distinguish missing files by explicit read exceptions rather than existence probing. PR #7 was intentionally returned to draft until the canonical Windows gate passes on this exact head.


## 2026-09-25 — canonical PASS on final preservation head

The owner ran `.\tools\verify-release.ps1` on `81a80987fe37eca822bc7462355a0e1c63dbc1bc` with .NET SDK `8.0.425`. Result: `WINSIDEBAR 2.0 RELEASE VERIFICATION: PASS`. The expected flat EXE/ZIP/checksum artifacts were produced under `artifacts\release`.

All review threads are resolved and PR #7 is mergeable. One current-head technical check remains: open the `WinSidebar.exe` produced by this exact PASS once. The earlier artifact launch covered the pre-hardening build and does not substitute for this final-head launch. Owner authorization for merge and publication remains in force after that final launch.


## 2026-09-25 — merge complete; publication pending

PR #7 was merged to `main` with merge commit `2597fe5ddda8906fd4e2a31ab8591ed4e7f6a2a4`.

The tested release head `81a80987fe37eca822bc7462355a0e1c63dbc1bc` and the merge commit have identical Git tree SHA `00d4c1502e529bcaea609a8da6971088171fa6aa`. Therefore the files now on `main` are byte-for-byte the same repository tree that passed the canonical verifier and artifact sanity launch.

The intended public tag remains `v2.0`. At this checkpoint no `v2.0` tag or GitHub Release exists yet; the only public release observed is `v1.0.0`. Publication remains authorized by the owner and pending through the local `gh` workflow because the connected GitHub write surface does not expose tag/release creation.


## FINAL CLOSURE — WinSidebar 2.0 published — 2026-09-25

This section supersedes the pre-publication checkpoints above for current state. The earlier sections remain preserved as provenance.

- Canonical Windows verifier PASS on tested head `81a80987fe37eca822bc7462355a0e1c63dbc1bc`, SDK `8.0.425`.
- Final artifact sanity launch: PASS; owner opened the EXE produced by that exact gate and confirmed normal startup.
- PR #7 merged to `main` as `2597fe5ddda8906fd4e2a31ab8591ed4e7f6a2a4`.
- Tested head and merge commit share identical Git tree `00d4c1502e529bcaea609a8da6971088171fa6aa`.
- Annotated tag `v2.0` exists publicly and points exactly to merge commit `2597fe5ddda8906fd4e2a31ab8591ed4e7f6a2a4`.
- GitHub Release `WinSidebar 2.0 — Windows x64` is public, not draft and not prerelease; published at `2026-09-25T18:50:55Z`.
- Public release assets are exactly:
  - `SHA256SUMS.txt` — 177 bytes; GitHub asset digest `sha256:ff2c7645613ec948930b7445ec64cefcbe32ac5cb287b306b38c9ca7885e6dac`;
  - `WinSidebar-v2.0-win-x64.zip` — 66,068,922 bytes; GitHub asset digest `sha256:decf7fe7d7616f7ecb03ec08175dd7cafae8a455b37340642a670cbb92436fdf`.
- The published release body matches `docs/RELEASE-NOTES.md` after line-ending normalization.
- The shipping workflow on `main` is manual-only and delegates to `tools/verify-release.ps1`, uploading the flat `artifacts/release/` output.
- No further merge/tag/publication action remains for WinSidebar 2.0.

The `develop` dossier remains internal provenance only. Do not merge `development/**` into shipping `main`.


## 2026-09-25 — post-release repository-state synchronization

A documentation-only follow-up was prepared from published `main` to make the GitHub repository describe the current post-release state explicitly:

- branch: `docs/post-release-2.0-sync`;
- commit: `b8a1990daea0da11cf610bb8b8ac0577a7322b07`;
- PR: #8 `docs: align repository with published WinSidebar 2.0`;
- scope: `PROJECT.md`, `project.json`, `CONTRIBUTING.md`, `docs/README.md`, `docs/DEVELOPMENT.md`, `docs/DESIGN-DECISIONS.md`, `docs/RELEASE.md` only;
- no runtime, localization, test, verifier, packaging or workflow files changed;
- live tag/release metadata was cross-checked against GitHub and matches the proposed `project.json` release record;
- PR #8 is open, ready for review and currently mergeable.

Important branch-authority rule: after the v2.0 publication, `main` plus the immutable `v2.0` tag are the producer authority. The non-`development/**` tree on this historical `develop` branch is older and must not be used to infer current source, workflows, packaging or product state. Use `develop` only for the internal dossier until a separately authorized synchronization/merge decision is made.


## 2026-09-25 — post-release documentation sync merged

PR #8 (`docs: align repository with published WinSidebar 2.0`) merged successfully into `main` as `790cd352b11f67625f121a8fb9dcd4d9022ed6e0`.

Post-merge verification:
- `main` differs from the published v2.0 source baseline only by the PR #8 documentation/index change and its merge commit;
- changed paths are limited to `CONTRIBUTING.md`, `PROJECT.md`, `project.json`, `docs/README.md`, `docs/DEVELOPMENT.md`, `docs/DESIGN-DECISIONS.md` and `docs/RELEASE.md`;
- annotated tag `v2.0` remains unchanged and continues to target published source commit `2597fe5ddda8906fd4e2a31ab8591ed4e7f6a2a4`;
- GitHub Release `v2.0` remains public/non-draft/non-prerelease with `WinSidebar-v2.0-win-x64.zip` and `SHA256SUMS.txt`;
- no runtime, localization, test, packaging, verifier or workflow change occurred.

Current authority is now explicit in public documentation: use `v2.0` for the exact shipped 2.0 source baseline and `main` for current repository/maintenance guidance.


## 2026-09-25 — branch/PR hygiene classification after v2.0

Read-only comparison against current `main` (`790cd352b11f67625f121a8fb9dcd4d9022ed6e0`) distinguishes refs that are fully incorporated from refs that preserve divergent history.

Fully incorporated refs (zero commits unique relative to current `main`; deletion may be considered later as a separate owner-authorized cleanup):
- `chore/release-2.0`;
- `chore/release-2.0.0`;
- `checkpoint/pre-final-review-fixes-20260925`;
- the merged documentation branch `docs/post-release-2.0-sync` is likewise a normal post-merge cleanup candidate.

Divergent historical checkpoints/features retain commits not present in the clean `main` lineage and must not be deleted merely because v2.0 is published. They are provenance/rollback evidence unless separately reviewed.

Open draft PRs #1–#5 remain historical WIP records on the stacked development lineage. They were not closed or modified in this pass. Closing those PRs, deleting branches, and restructuring `develop` are separate repository-governance actions requiring explicit owner authorization.

Repository UI metadata observed after the sync: default branch `main`, public repository, current latest release `v2.0`; description remains `WinSidebar by Hamthet. For those who hate ALT+TAB confusion`, homepage is unset and topics are empty. These metadata fields are not contradictory to the software state, but they were not changed because the connected GitHub mutation surface does not expose repository-description/homepage/topic editing.


## 2026-09-25 — historical PR closure / incorporated-ref cleanup gate

Owner authorized the post-release housekeeping described in the preceding audit.

Completed:
- PRs #1, #2, #3, #4 and #5 were closed as **superseded without merge**.
- Each PR received a provenance comment stating that WinSidebar 2.0 was integrated through the clean release line/PR #7 and published as `v2.0`.
- Their divergent head branches were intentionally preserved.
- No open pull requests remain at this checkpoint.

Revalidated deletion candidates against current `main` `790cd352b11f67625f121a8fb9dcd4d9022ed6e0`:
- `chore/release-2.0` — fully incorporated; no unique commits;
- `chore/release-2.0.0` — fully incorporated; no unique commits;
- `checkpoint/pre-final-review-fixes-20260925` — fully incorporated; no unique commits;
- `docs/post-release-2.0-sync` — merged and fully incorporated; no unique commits.

All four refs are unprotected. The connected GitHub mutation surface available in this session does not expose branch/ref deletion, so those deletions were **not** claimed or simulated. They remain pending as a local authenticated Git/gh cleanup step. No divergent feature/checkpoint branch is authorized for deletion by this classification.

No rulesets were observed. Direct branch-protection read returned integration-level 403, so no claim is made about branch-protection configuration beyond the branch-list `protected:false` values observed for the four cleanup candidates.
