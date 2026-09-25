# Naming update — 2026-09-25

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
