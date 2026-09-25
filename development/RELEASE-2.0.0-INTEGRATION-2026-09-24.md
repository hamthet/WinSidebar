# Naming update — 2026-09-25

## Current closure — 2026-09-25

The active clean release branch is `chore/release-2.0`, not the historical `chore/release-2.0.0` branch described later in this file.

Current checkpoint:
- head: `a156c3fff7497fc7f33c262eb3d20f029818d59e`;
- PR: #7 `release: prepare WinSidebar 2.0` -> `main`;
- live comparison at this checkpoint: 28 commits ahead / 0 behind `main`;
- runtime/source behavior: unchanged from the owner-approved 2.0 candidate;
- post-approval changes: public documentation and release verification only;
- verifier now requires the complete five-language public documentation/artwork set and an exact flat OutputRoot with no leftover technical directories or unrelated stale content;
- PR #7 records explicit owner runtime approval.

Canonical gate result: PASS. The owner ran `.\tools\verify-release.ps1` on Windows at artifact-producing head `72a6a76f58d67751abdb2e4bb8025dcefafde11f` with .NET SDK `8.0.425`; it returned `WINSIDEBAR 2.0 RELEASE VERIFICATION: PASS` and produced the documented flat release artifacts. Current head `a156c3fff7497fc7f33c262eb3d20f029818d59e` changes only the manual workflow upload path from the removed `dist/` directory to the flat `artifacts/release/` directory, so the validated artifact-producing inputs are unchanged. If not already done, opening the produced EXE once remains an artifact sanity check only. Final PR review is complete with no release blocker found and PR #7 is ready for review. Merge and GitHub Release publication remain separate actions.

The historical integration sequence below is preserved for provenance and must not override this current closure.

The owner simplified the public release name from **2.0.0** to **2.0**. The original integration record below remains historical evidence of the first clean-release pass.

Current release integration:
- branch: `chore/release-2.0`
- draft PR: #7 `release: prepare WinSidebar 2.0`
- PR #6 / `chore/release-2.0.0`: closed as superseded, not merged
- public package: `WinSidebar-v2.0-win-x64.zip`
- intended tag: `v2.0`
- technical Windows AssemblyVersion/FileVersion remain `2.0.0.0`
- repository now includes `PROJECT.md`, architecture/data/development/release docs, CONTRIBUTING, five-language START-HERE files and five-language beginner FAQ

Do not treat the older 2.0.0 public naming in the historical section as the current release contract.

Current repository-organization closure (2026-09-25):
- `PROJECT.md` is the human-readable maintainer/AI entry point;
- `project.json` is a machine-readable index and is validated by the canonical release verifier;
- `tools/verify-release.ps1` is the single local build/test/package gate and the manual build workflow delegates to it;
- `SUPPORT.md`, localized support pages and the GitHub bug form provide beginner-friendly, privacy-aware reporting;
- release ZIP entry documents are named `START-HERE*.txt` and the executable remains at archive root;
- PR #7 is the active draft; PR #6 remains closed/superseded.

---

# WinSidebar 2.0.0 — clean release integration

**Date:** 2026-09-24  
**Producer:** `hamthet/WinSidebar`  
**Version decision:** owner approved **2.0.0** for the next release line.  
**Official `main`:** still unchanged at `f291352b7456227d221b3708fd8a283925dc3143`.

## Software source closure

The final stacked development source is preserved at:

- `checkpoint/software-2.0.0-final-20260924`
- commit `8f85cd1377f1677f710366ae121e9fe7bc98be55`

The owner reported the final software state as functional after the PowerShell 5.1 release-gate encoding defect was corrected. No raw console log was supplied in that message, so this remains an owner-reported real-machine result, not independently observed output.

The product contract at this checkpoint includes:

- English default for brand-new installations;
- runtime languages `en-US`, `pt-BR`, `es-ES`, `ru-RU`, `zh-CN`;
- AltGr+Y toggle;
- F1..F4 shortcuts 1..4;
- configurable snippet hotkeys, default Shift+F1..F4 for snippets 1..4;
- 4..12 shortcuts and 4..8 literal-text snippets;
- self-contained single-file .NET 8 Windows x64 publication.

## Clean release branch

A clean integration branch was created **from the current `main`**, not from the stacked feature history:

`chore/release-2.0.0`

The branch intentionally contains no `development/**` tree.

Integration sequence:

1. `0a07dcbd...` — approved runtime source, localization resources and production smoke tests integrated; assembly/package version set to 2.0.0.
2. `8f919aa0...` — English-default README/quick-start plus Brazilian Portuguese, Spanish, Russian and Simplified Chinese equivalents.
3. `95ffee2d...` — five-language tutorials and publishing guidance.
4. `1c09468a...` — five-language concept-art SVG sources; obsolete Portuguese-root and duplicated English-locale artwork removed.
5. `82f8e6f8...` — manual-only build, localization and artwork workflows for 2.0.0.
6. `d4411e62...` — five-language 2.0.0 release notes.
7. `86fac179...` — explicit Python runtime setup for the manual localization workflow.

## Public-language structure

English is the default repository/public entry point:

- `README.md`
- `README.txt`
- `docs/TUTORIAL.md`
- `docs/OUTREACH.md`
- `docs/RELEASE-NOTES.md`
- `assets/hero-illustration.svg`
- `assets/linkedin-illustration.svg`

Localized counterparts live under `docs/i18n/` and `assets/i18n/<locale>/` for:

- `pt-BR`
- `es-ES`
- `ru-RU`
- `zh-CN`

The old root Portuguese-only files and obsolete 1.0.0 documentation/artwork paths were removed from the clean release branch. History remains available through Git.

## Static integration audit

Observed on the clean release branch before local build validation:

- no `development/**` paths;
- version = `2.0.0`;
- neutral/default language = `en-US`;
- `SelfContained=true`;
- `PublishSingleFile=true`;
- 149 localization keys in the base catalog and exact 149-key parity in Spanish, Russian and Simplified Chinese;
- 10 localized concept-art SVGs: two English-default root assets plus two assets for each of the four other locales;
- no automatic `push` or `pull_request` trigger in build/localization/artwork workflows;
- all required README/tutorial/outreach/release-note language paths present.

This is a **static repository audit**, not a Windows compilation/run of the clean integration branch.

## Remaining gate

Before merge/publication:

1. checkout `chore/release-2.0.0` locally;
2. run localization and snippet-store smoke tests;
3. publish the self-contained single EXE from the clean release branch;
4. verify the output and five-language ZIP packaging;
5. perform a short real-machine smoke of the resulting WinSidebar.exe;
6. review the Pull Request against `main`;
7. merge only after explicit owner approval;
8. publish tag/release `v2.0.0` separately after merge.

Git merge, owner functional approval and GitHub release publication remain separate acts.

## Pull Request

Draft PR [#6](https://github.com/hamthet/WinSidebar/pull/6), **release: prepare WinSidebar 2.0.0**, now proposes `chore/release-2.0.0` -> `main`.

At creation, GitHub reports the PR as mergeable and the branch is 7 commits ahead / 0 behind `main`. The PR remains draft because clean-branch local Windows build/smoke evidence is still pending. No hosted Actions were dispatched.


## Owner approval and final presentation adjustment — 2026-09-25

The owner reported the clean WinSidebar 2.0 candidate as **tested and approved** on the real Windows machine.

Immediately after that approval, the owner correctly identified that the verifier's local OutputRoot still exposed build-oriented `publish/`, `staging/` and `dist/` directories. Those directories were useful to the build implementation but were not user-friendly.

A non-runtime release-tooling adjustment on `chore/release-2.0` now keeps those directories transient and removes them after PASS. The successful OutputRoot is flat and directly exposes:

- `WinSidebar.exe`
- `WinSidebar-v2.0-win-x64.zip`
- `SHA256SUMS.txt`
- `START-HERE.txt` plus PT-BR / ES / RU / ZH-CN variants
- `LICENSE`

This amendment does **not** change WinSidebar runtime source, localization catalogs or persisted-data contracts. The owner's functional approval therefore remains valid for the executable behavior. The flat-output copy/cleanup amendment has been statically audited in source but was not separately re-executed in this chat after the approval.

Draft PR #7 may now be treated as ready for review. Merge and GitHub Release publication remain separate acts and require their own explicit authorization.
