# WinSidebar — development dossier

**Repository:** `hamthet/WinSidebar` · **development branch:** `develop` · updated 2026-09-23. Project source, tests, resources and engineering records belong in WinSidebar. `development/**` must **not** enter shipping `main` or the release ZIP. Target languages: English, Brazilian Portuguese, Spanish, Russian and Simplified Chinese. Official v1.0.0 remains unchanged.

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

## Frozen checkpoint and next feature

- **Frozen checkpoint:** [`checkpoint/functional-menu-approved-20260923`](https://github.com/hamthet/WinSidebar/tree/checkpoint/functional-menu-approved-20260923) points exactly to `6af6343e8d5e6e0dc8cbd19f3d40e378cb583194`. Treat it as read-only project state: five-language preview accepted; per-window context menu plus Rename/Reset accepted; global context menu on blank/non-window list space restored and owner-confirmed.
- **Next feature reserved:** [`feature/text-snippets`](https://github.com/hamthet/WinSidebar/tree/feature/text-snippets), created from that exact checkpoint. No product code for it exists yet. Specification: [`FEATURE-TEXT-SNIPPETS-2026-09-23.md`](FEATURE-TEXT-SNIPPETS-2026-09-23.md).

## Outstanding gates before a new official version
- **Functional:** verify real Alt+Tab parity, no Calculator/Settings ghost windows, five separately named Calculator windows, reversible ignore, auto-save/Restore and failure handling, multi-monitor, editor foreground, keyboard/hotkeys. Assess narrow-width CJK/Cyrillic font rendering, restart persistence and 100–200% DPI. Owner excluded cross-version preference migration tests.
- **GitHub:** review stacked drafts #1–#5 and their diffs; neutralize automatic workflows on relevant base branches before Russian PR or integration. Prepare a clean release branch based on `main` with only approved shipping source, resources, production tests and public assets. Never merge development planning wholesale or include one-shot scripts in final tree/ZIP.
- **Documentation/release:** internal engineering notes exist; keep their test statuses accurate. Finish public READMEs, tutorials, images and in-ZIP instructions in five languages after functional gates. Build and checksum the Windows deliverable locally and publish a new GitHub release in WinSidebar; FILEBRIDGE only on an explicit request for temporary test transfer.

Supporting records: [`BASELINE.md`](BASELINE.md), [`ENGINEERING-NOTES.md`](ENGINEERING-NOTES.md), [`IMPLEMENTATION-BACKLOG.md`](IMPLEMENTATION-BACKLOG.md), [`PLAN.md`](PLAN.md), [`PRODUCT-REVIEW.md`](PRODUCT-REVIEW.md), [`VERIFICATION.md`](VERIFICATION.md), [`RELEASE-GATE-OVERRIDE.md`](RELEASE-GATE-OVERRIDE.md), [`DECISIONS.md`](DECISIONS.md), [`LOG.md`](LOG.md), dated `LOG-*.md` and locale dossiers.
