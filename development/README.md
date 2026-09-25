# WinSidebar — development dossier

**Repository:** `hamthet/WinSidebar` · **development branch:** `develop` · updated 2026-09-25. Project source, tests, resources and engineering records belong in WinSidebar. `development/**` must **not** enter shipping `main` or the release ZIP. Target languages: English, Brazilian Portuguese, Spanish, Russian and Simplified Chinese. Official **v2.0 is published**; v1.0.0 remains a historical public release.

## Operating decisions

Read [`WORKFLOW-LOCAL-FILEBRIDGE.md`](WORKFLOW-LOCAL-FILEBRIDGE.md) before working. The owner confirmed GitHub Actions credits unavailable: Windows PowerShell without admin/CMD, portable .NET SDK, `git` and `gh` are the local build/test workflow; do not dispatch Actions. Inspect each base branch's inherited workflow triggers before opening or merging PRs. FILEBRIDGE is only temporary transfer of a specific artifact the owner expressly needs to download/test, never routine source storage, documentation or release hosting. The independent local checkout is `C:\git\WinSidebar`; do not involve ChatGPT Collector or publish personal local paths or screenshot content in shipping material.

## Product and branch / PR stack

1. [Draft PR #1](https://github.com/hamthet/WinSidebar/pull/1) — window-management prototype; historical compilation evidence, comprehensive real Alt+Tab/phantom-window/alias/ignored-app/monitor acceptance still pending.
2. [Draft PR #2](https://github.com/hamthet/WinSidebar/pull/2) — English/Portuguese runtime, stacked on #1; historical automated smoke/build evidence. Owner previously reported positive local language review.
3. [Draft PR #3](https://github.com/hamthet/WinSidebar/pull/3) — Spanish runtime, stacked on #2; historical automated smoke/preview evidence. Spanish base still has a `pull_request` workflow trigger: neutralize safely before opening a Russian PR against it.
4. [Russian feature branch](https://github.com/hamthet/WinSidebar/tree/feature/runtime-i18n-ru) — based on Spanish. Russian catalog/loader/embedded resources and smoke source; Russian menu committed in [`ed98ddd`](https://github.com/hamthet/WinSidebar/commit/ed98ddd). UI correction [`67fe834`](https://github.com/hamthet/WinSidebar/commit/67fe83493bbe07861acfbfa55d076f922bff9280) adds a visible language chooser, removes redundant sidebar Save and positions the shortcut editor above the bar. Russian PR not yet opened.
5. [Draft PR #4](https://github.com/hamthet/WinSidebar/pull/4) — Chinese (`zh-CN`) stacked on Russian. Chinese catalog, loader, embeddings, five-language smoke source and fifth menu option as [`29f88af`](https://github.com/hamthet/WinSidebar/commit/29f88af) are committed. Owner reopened the local preview and replied **“aprovado”**. This approves the five-language preview/Chinese locale stage, not blanket functional acceptance.
6. [Draft PR #5](https://github.com/hamthet/WinSidebar/pull/5) — functional context-menu hardening. Owner now confirms: per-window right-click works; Rename and Reset Name work; the restored global menu on blank/list-nonwindow areas also works. The tested branch head is [`6af6343`](https://github.com/hamthet/WinSidebar/commit/6af6343e8d5e6e0dc8cbd19f3d40e378cb583194). This does not yet certify Ignore/undo, Alt+Tab fidelity, persistence failure paths or the broader release matrix.

The stacked feature PRs remain historical snapshots and were not merged wholesale. The clean integration PR #7 has been merged to `main`; distinguish historical source commits, executable test output, owner preview approval, functional acceptance and the published release.

## Frozen checkpoints and release integration

- **Functional-menu checkpoint:** [`checkpoint/functional-menu-approved-20260923`](https://github.com/hamthet/WinSidebar/tree/checkpoint/functional-menu-approved-20260923) at `6af6343e8d5e6e0dc8cbd19f3d40e378cb583194`.
- **Final software checkpoint:** [`checkpoint/software-2.0.0-final-20260924`](https://github.com/hamthet/WinSidebar/tree/checkpoint/software-2.0.0-final-20260924) at `8f85cd1377f1677f710366ae121e9fe7bc98be55`. The owner subsequently reported the software state as functional.
- **Release integration branch:** [`chore/release-2.0`](https://github.com/hamthet/WinSidebar/tree/chore/release-2.0) was the clean shipping candidate and intentionally contained no `development/**`. PR #7 merged this state to `main` as `2597fe5ddda8906fd4e2a31ab8591ed4e7f6a2a4`. PR #6 / `chore/release-2.0.0` remains closed as superseded without merge.
- **Integration record:** [`RELEASE-2.0.0-INTEGRATION-2026-09-24.md`](RELEASE-2.0.0-INTEGRATION-2026-09-24.md).

## Official v2.0 closure
- **Canonical Windows gate: PASS.** `tools/verify-release.ps1` was run on artifact-producing head `72a6a76f58d67751abdb2e4bb8025dcefafde11f` with .NET SDK `8.0.425` and returned `WINSIDEBAR 2.0 RELEASE VERIFICATION: PASS`. Current release head `a156c3fff7497fc7f33c262eb3d20f029818d59e` differs only by the manual workflow upload path; runtime, verifier and package inputs are unchanged.
- **Artifact sanity: PASS.** The owner opened the generated `WinSidebar.exe` successfully after the canonical release gate. This did not reopen feature acceptance.
- **Pull Request:** final review of PR #7 against `main` is complete with no release blocker found; PR #7 is now ready for review. Old stacked PRs remain historical evidence and must not be merged wholesale.
- **Merge:** all known technical gates are closed; merge remains a separate explicit action.
- **Publication:** tag/release `v2.0` only after merge. Git merge and GitHub Release publication remain separate acts.
- **Scope:** runtime behavior is frozen and owner-approved; avoid opportunistic functional changes during release review. Cross-version preference migration remains out of scope by owner decision.

Supporting records: [`BASELINE.md`](BASELINE.md), [`ENGINEERING-NOTES.md`](ENGINEERING-NOTES.md), [`IMPLEMENTATION-BACKLOG.md`](IMPLEMENTATION-BACKLOG.md), [`PLAN.md`](PLAN.md), [`PRODUCT-REVIEW.md`](PRODUCT-REVIEW.md), [`VERIFICATION.md`](VERIFICATION.md), [`RELEASE-GATE-OVERRIDE.md`](RELEASE-GATE-OVERRIDE.md), [`DECISIONS.md`](DECISIONS.md), [`LOG.md`](LOG.md), dated `LOG-*.md` and locale dossiers.


## 2026-09-25 — owner approval / flat release presentation

Owner reported the WinSidebar 2.0 clean candidate as tested and approved. Runtime behavior is approved.

Final release-tooling presentation was then simplified: canonical verifier work folders `publish/`, `staging/` and `dist/` are transient only. After PASS, OutputRoot is flat with the EXE, ZIP, checksums, LICENSE and five START-HERE files directly visible. This is a packaging/tooling presentation change only; runtime source was not changed.

Follow-up audit commit `a156c3fff7497fc7f33c262eb3d20f029818d59e` strengthens that contract: the verifier now rejects unrelated pre-existing OutputRoot content, requires an exact flat final file set, requires all localized README/tutorial/release-note/outreach files plus localized concept-art SVGs, and performs stable-token parity checks across the main translated product docs. The same commit makes end-user recovery wording less engineering-oriented. No `src/**` or runtime localization catalog changed.


## 2026-09-25 — final automated-review hardening

Marking PR #7 ready triggered a new automated review that found valid runtime/data-safety issues not covered by the previous owner PASS. State before these fixes is preserved at `checkpoint/pre-final-review-fixes-20260925`.

Commit `81a80987fe37eca822bc7462355a0e1c63dbc1bc` addresses the confirmed findings: paste-target recheck, partial SendInput cleanup, rollback snapshot preservation, invariant Downloads fallback, ignored-app read/write/recovery hardening, unreadable-settings write suppression, AltGr+Y tooltip correction and localized generated snippet names. Smoke coverage was extended for localization/settings and localized snippet defaults. Static audit: 152 localization keys, exact five-language parity, no undefined source keys.

Because this is a runtime-changing commit, the previous canonical PASS is historical evidence only. Merge/release remain owner-authorized but blocked until the canonical verifier and one artifact launch pass on the new head.


Follow-up commit `81a80987fe37eca822bc7462355a0e1c63dbc1bc` hardens missing-vs-unreadable detection for settings and rollback snapshots. PR #7 is intentionally draft pending the canonical Windows PASS and artifact launch on this exact head. Owner authorization for merge/publication remains valid once the technical gate closes.


Final preservation head `81a80987fe37eca822bc7462355a0e1c63dbc1bc` passed the canonical Windows verifier with .NET SDK `8.0.425`. All PR review threads are resolved and the PR is mergeable. Remaining technical gate: launch the EXE produced by this exact PASS once; the prior launch applied to an earlier build.


## 2026-09-25 — PR #7 merged

PR #7 merged successfully into `main` as `2597fe5ddda8906fd4e2a31ab8591ed4e7f6a2a4`. The merge commit tree equals the tested head tree exactly: `00d4c1502e529bcaea609a8da6971088171fa6aa`. Thus the merged repository state matches the canonical-PASS/artifact-launched build.

Publication step remains: create tag `v2.0` from this `main` commit and publish a GitHub Release with the verified ZIP and `SHA256SUMS.txt`. No `v2.0` tag/release existed at this checkpoint.


## 2026-09-25 — v2.0 publication complete

WinSidebar 2.0 is publicly released.

- final tested source head: `81a80987fe37eca822bc7462355a0e1c63dbc1bc`;
- merge commit on `main`: `2597fe5ddda8906fd4e2a31ab8591ed4e7f6a2a4`;
- identical tested/merged tree: `00d4c1502e529bcaea609a8da6971088171fa6aa`;
- public annotated tag: `v2.0`, pointing exactly to the merge commit;
- GitHub Release: `WinSidebar 2.0 — Windows x64`;
- release state: public, non-draft, non-prerelease;
- published: `2026-09-25T18:50:55Z`;
- assets: `WinSidebar-v2.0-win-x64.zip` and `SHA256SUMS.txt`;
- ZIP GitHub digest: `sha256:decf7fe7d7616f7ecb03ec08175dd7cafae8a455b37340642a670cbb92436fdf`;
- checksum-file GitHub digest: `sha256:ff2c7645613ec948930b7445ec64cefcbe32ac5cb287b306b38c9ca7885e6dac`;
- release body matches the versioned `docs/RELEASE-NOTES.md` after line-ending normalization.

The 2.0 release cycle is closed. Future product work should start from current `main`; retain this `develop` dossier as provenance and planning history, not as shipping source.
