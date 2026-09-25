# WinSidebar — development dossier

**Repository:** `hamthet/WinSidebar` · **development branch:** `develop` · updated 2026-09-25. Project source, tests, resources and engineering records belong in WinSidebar. `development/**` must **not** enter shipping `main` or the release ZIP. Target languages: English, Brazilian Portuguese, Spanish, Russian and Simplified Chinese. Official v1.0.0 remains unchanged.

## Operating decisions

Read [`WORKFLOW-LOCAL-FILEBRIDGE.md`](WORKFLOW-LOCAL-FILEBRIDGE.md) before working. The owner confirmed GitHub Actions credits unavailable: Windows PowerShell without admin/CMD, portable .NET SDK, `git` and `gh` are the local build/test workflow; do not dispatch Actions. Inspect each base branch's inherited workflow triggers before opening or merging PRs. FILEBRIDGE is only temporary transfer of a specific artifact the owner expressly needs to download/test, never routine source storage, documentation or release hosting. The independent local checkout is `C:\git\WinSidebar`; do not involve ChatGPT Collector or publish personal local paths or screenshot content in shipping material.

## Product and branch / PR stack

1. [Draft PR #1](https://github.com/hamthet/WinSidebar/pull/1) — window-management prototype; historical compilation evidence, comprehensive real Alt+Tab/phantom-window/alias/ignored-app/monitor acceptance still pending.
2. [Draft PR #2](https://github.com/hamthet/WinSidebar/pull/2) — English/Portuguese runtime, stacked on #1; historical automated smoke/build evidence. Owner previously reported positive local language review.
3. [Draft PR #3](https://github.com/hamthet/WinSidebar/pull/3) — Spanish runtime, stacked on #2; historical automated smoke/preview evidence. Spanish base still has a `pull_request` workflow trigger: neutralize safely before opening a Russian PR against it.
4. [Russian feature branch](https://github.com/hamthet/WinSidebar/tree/feature/runtime-i18n-ru) — based on Spanish. Russian catalog/loader/embedded resources and smoke source; Russian menu committed in [`ed98ddd`](https://github.com/hamthet/WinSidebar/commit/ed98ddd). UI correction [`67fe834`](https://github.com/hamthet/WinSidebar/commit/67fe83493bbe07861acfbfa55d076f922bff9280) adds a visible language chooser, removes redundant sidebar Save and positions the shortcut editor above the bar. Russian PR not yet opened.
5. [Draft PR #4](https://github.com/hamthet/WinSidebar/pull/4) — Chinese (`zh-CN`) stacked on Russian. Chinese catalog, loader, embeddings, five-language smoke source and fifth menu option as [`29f88af`](https://github.com/hamthet/WinSidebar/commit/29f88af) are committed. Owner reopened the local preview and replied **“aprovado”**. This approves the five-language preview/Chinese locale stage, not blanket functional acceptance.
6. [Draft PR #5](https://github.com/hamthet/WinSidebar/pull/5) — functional context-menu hardening. Owner now confirms: per-window right-click works; Rename and Reset Name work; the restored global menu on blank/list-nonwindow areas also works. The tested branch head is [`6af6343`](https://github.com/hamthet/WinSidebar/commit/6af6343e8d5e6e0dc8cbd19f3d40e378cb583194). This does not yet certify Ignore/undo, Alt+Tab fidelity, persistence failure paths or the broader release matrix.

No feature PR is merged into `develop` or `main`. Older logs and stage sections are historical snapshots; distinguish a source commit, executable test output, owner preview approval, functional acceptance and official release.

## Frozen checkpoints and release integration

- **Functional-menu checkpoint:** [`checkpoint/functional-menu-approved-20260923`](https://github.com/hamthet/WinSidebar/tree/checkpoint/functional-menu-approved-20260923) at `6af6343e8d5e6e0dc8cbd19f3d40e378cb583194`.
- **Final software checkpoint:** [`checkpoint/software-2.0.0-final-20260924`](https://github.com/hamthet/WinSidebar/tree/checkpoint/software-2.0.0-final-20260924) at `8f85cd1377f1677f710366ae121e9fe7bc98be55`. The owner subsequently reported the software state as functional.
- **Clean release branch:** [`chore/release-2.0`](https://github.com/hamthet/WinSidebar/tree/chore/release-2.0), created from the current `main` lineage, contains the approved runtime, production smoke tests, public version **2.0**, five-language beginner/user documentation, `PROJECT.md`, machine-readable `project.json`, deterministic `tools/verify-release.ps1`, privacy-aware support guidance and AI/maintainer project maps. It intentionally contains no `development/**`. Draft PR #7 targets `main`. PR #6 / `chore/release-2.0.0` was closed as superseded without merge.
- **Integration record:** [`RELEASE-2.0.0-INTEGRATION-2026-09-24.md`](RELEASE-2.0.0-INTEGRATION-2026-09-24.md).

## Outstanding gates before official v2.0
- **Canonical Windows gate: PASS.** `tools/verify-release.ps1` was run on `chore/release-2.0` at `72a6a76f58d67751abdb2e4bb8025dcefafde11f` with .NET SDK `8.0.425` and returned `WINSIDEBAR 2.0 RELEASE VERIFICATION: PASS`.
- **Artifact sanity:** the gate produced the flat OutputRoot/ZIP contract. If not already done locally, open the newly produced `WinSidebar.exe` once; this is not a reopening of feature acceptance.
- **Pull Request:** complete final review of PR #7 against `main`; old stacked PRs remain historical evidence and must not be merged wholesale.
- **Merge:** separate explicit action after final review.
- **Publication:** tag/release `v2.0` only after merge. Git merge and GitHub Release publication remain separate acts.
- **Scope:** runtime behavior is frozen and owner-approved; avoid opportunistic functional changes during release review. Cross-version preference migration remains out of scope by owner decision.

Supporting records: [`BASELINE.md`](BASELINE.md), [`ENGINEERING-NOTES.md`](ENGINEERING-NOTES.md), [`IMPLEMENTATION-BACKLOG.md`](IMPLEMENTATION-BACKLOG.md), [`PLAN.md`](PLAN.md), [`PRODUCT-REVIEW.md`](PRODUCT-REVIEW.md), [`VERIFICATION.md`](VERIFICATION.md), [`RELEASE-GATE-OVERRIDE.md`](RELEASE-GATE-OVERRIDE.md), [`DECISIONS.md`](DECISIONS.md), [`LOG.md`](LOG.md), dated `LOG-*.md` and locale dossiers.


## 2026-09-25 — owner approval / flat release presentation

Owner reported the WinSidebar 2.0 clean candidate as tested and approved. Runtime behavior is approved.

Final release-tooling presentation was then simplified: canonical verifier work folders `publish/`, `staging/` and `dist/` are transient only. After PASS, OutputRoot is flat with the EXE, ZIP, checksums, LICENSE and five START-HERE files directly visible. This is a packaging/tooling presentation change only; runtime source was not changed.

Follow-up audit commit `72a6a76f58d67751abdb2e4bb8025dcefafde11f` strengthens that contract: the verifier now rejects unrelated pre-existing OutputRoot content, requires an exact flat final file set, requires all localized README/tutorial/release-note/outreach files plus localized concept-art SVGs, and performs stable-token parity checks across the main translated product docs. The same commit makes end-user recovery wording less engineering-oriented. No `src/**` or runtime localization catalog changed.
