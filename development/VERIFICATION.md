# Verification and release gates

> **Historical 2026-09-22 matrix — not the current release checklist.** The row statuses below were not retroactively rewritten as the product evolved. Current 2.0 release verification is defined by `DEVELOPMENT-ENTRYPOINT.md`, the clean `chore/release-2.0` branch and its `tools/verify-release.ps1`. Do not treat old `NOT RUN` rows as a statement that the approved current runtime is unimplemented.

**Status as of 2026-09-22:** all new five-language runtime tests `NOT RUN`. Prior Windows publish and concept-art render CI succeeded for earlier commits only; neither is evidence that a multilingual release exists. Update every status with evidence; never precheck a future test.

## Evidence format for each test

`ID | commit SHA | Windows edition/build | locale + display scale | clean or upgraded installation | steps/command | expected | actual | PASS/FAIL/BLOCKED/NOT RUN | screenshot/log/artifact URL (redacted) | reviewer/date`. Keep test artifacts in CI or redacted issue/PR attachments; no personal paths, keys or window titles. If no automated Windows interaction is available, record a human-executed manual test and why automation was unavailable. Inspect both code tree and **actual ZIP and EXE**; a doc link is not release evidence.

## Traceability: core functionality, locale and failure paths

| ID | Scenario / check | Acceptance | Status |
| --- | --- | --- | --- |
| LOC-01 | Exact key-set parity across en-US, pt-BR, es-ES, ru-RU, zh-CN | Build fails on missing/empty key, malformed UTF-8 or mismatched placeholders | NOT RUN |
| LOC-02 | New install language selection | Supported Windows language selected, unsupported fallback en-US; zh-Hant not falsely claimed | NOT RUN |
| LOC-03 | Upgrade from current settings.ini (no locale) | Legacy Portuguese retained; width, side, monitor and browser unchanged | NOT RUN |
| LOC-04 | Manual language selection and restart | Five choices accessible, preference persists, complete refresh including menus, tray, tooltips, status, accessible names and subsequent dialogs | NOT RUN |
| LOC-05 | User-defined shortcuts and saved tokens | Names, paths, URLs, icons, browser preference and XML schema intact after five switches and restart; only pristine defaults localized safely | NOT RUN |
| LOC-06 | Dynamic text/error coverage | Counts, monitor descriptions, format placeholders, confirmation, input/file picker messages, invalid URL, missing icon/browser/folder, duplicate launch and hotkey conflict translated | NOT RUN |
| LOC-07 | Typography and layout | No clipped essential text, missing glyphs, overlapping actions or inaccessible labels in all five locales at 100%, 150%, 200% Windows DPI | NOT RUN |
| CORE-01 | First launch from fresh extracted ZIP | Works under standard user without installer or separate .NET install; appropriate welcome/quick-start available | NOT RUN |
| CORE-02 | Window enumeration and switching | Documented inclusion policy, primary/secondary grouping, minimized window restore, mouse double-click, Shift+F1…F4 | NOT RUN |
| CORE-03 | Multi-monitor and reconfiguration | Primary/nonprimary selection, monitor unplug/replug, primary change, left/right, width limits, taskbar/work-area change stay usable | NOT RUN |
| CORE-04 | Four shortcuts | Edit/cancel/save, folder, HTTP/HTTPS website, system/custom browser, built-in/custom icon, restart persistence | NOT RUN |
| CORE-05 | Tray, lifecycle, errors | Tray toggle/menu, single instance, exit confirmation, hotkey conflicts and no orphaned hotkey registrations | NOT RUN |
| CORE-06 | Failure and recovery | Malformed/oversized XML, interrupted/denied write, missing folder/browser/icon, backup restore without data loss, actionable localized errors | NOT RUN |
| A11Y-01 | Keyboard and assistive access | Navigate all essential controls and language choice by keyboard; screen reader gets correct localized names; focus visible | NOT RUN |
| PACK-01 | Build and structure | .NET 8 win-x64 self-contained single EXE; catalogs embedded, no external runtime download; release ZIP has one EXE + five valid user-facing guides, no untranslated legacy quick start | NOT RUN |
| PACK-02 | Consistency and provenance | Version/tag/source SHA/executable/release text match; SHA256SUMS verifies downloaded ZIP; license and unsigned warning accurate | NOT RUN |
| PACK-03 | Source and privacy scan | No credentials, personal examples, sensitive paths, telemetry newly introduced or raw personal logs | NOT RUN |
| REPO-01 | Five complete READMEs and tutorials | English root, valid cross-links, descriptions match current app/release, localized images and alt text | NOT RUN |
| ART-01 | Five hero + promotional SVG/PNG pairs | Correct language, disclosure within images, rendered PNG inspected for text overflow, no conceptual artwork claimed as screenshot | NOT RUN |
| CLEAN-01 | Final tree isolation | Final `main` has no `development/**` or `docs/i18n/LOCALIZATION-PLAN.md`, no dev journal/internal plans; ZIP has no dev files | NOT RUN |
| USER-01 | Unassisted usage study | Participants can launch, switch, configure, change language, restart and exit/uninstall; blockers tracked and corrected | NOT RUN |

## Windows manual matrix to execute (do not infer from CI)

At minimum run clean and upgrade cases on Windows 10 x64 and Windows 11 x64 where available. For each locale exercise default and compact widths at normal and high DPI; at least one keyboard-only flow and accessibility pass. Use a single display and two displays; test a disconnected secondary, monitor change, missing folder/browser, nonexistent icon and denied settings folder. Capture screenshot evidence after rendering at each DPI; document exact failures and fixes. If one OS or display configuration is not available, keep it `BLOCKED`/`NOT RUN` and narrow the release compatibility statement rather than reporting a pass.

## CI changes to implement and prove

1. Trigger on source, i18n resources, tests, csproj, packaging instructions and workflow changes; run on PR to `develop`/release branch and tagged release as configured. Current workflow has hard-coded two-file source audits and only checks `LEIA-ME.txt` + EXE: replace its invariants deliberately.
2. Static locale checker: parse all resources, compare key sets, placeholder signatures, encoding; reject missing/empty strings and unintended locale files in release; optionally emit a human-review report. No automatic translation from a failing locale.
3. Unit tests for locale selection, fallback, formatting, default-vs-custom shortcut behavior, settings migration and atomic write/restore semantics. Keep tests outside shipping payload; design platform-specific tests to run on Windows.
4. Build with `dotnet publish ... -r win-x64 --self-contained true`; check exactly one executable and embedded resources, smoke launch in a controlled Windows runner if feasible. Report limitation if GUI automation is not possible.
5. Stage the documented five locale guides + EXE, enumerate ZIP entries exactly, reject dev docs, read each guide as UTF-8, calculate checksum and upload artifact. Compare official release asset after publishing.
6. Render language-specific SVGs to PNGs and check dimensions, disclosure and font coverage; automated SVG parsing is not enough: keep a manual visual approval entry.

## Final release checklist / stop conditions

- Every P0 (UX-01…UX-08) and associated LOC/CORE/PACK tests passes with a commit/artifact-linked report. Release cannot be promoted on a green compile alone.
- P1 questions have explicit scope decisions, release docs accurately limit any unsupported case. No unresolved known blocker affecting basic window switching or saved user data.
- A release branch is created **from approved main**. A diff review confirms only product code, embedded locale resources, useful production tests/workflows, user documentation and approved illustration assets. Do not merge `develop` wholesale; the legacy `docs/i18n/LOCALIZATION-PLAN.md` must be deleted in final release tree.
- Rebuild the final release branch, inspect archive manifest and checksum, install/test *that* executable; verify documented URLs and tag. If the produced binary changes after testing, rerun release checks.
- Sign-off record: product owner, engineering reviewer, locale reviewer(s), artifact SHA256 and date. If any gate fails or evidence is unavailable, mark release `BLOCKED` rather than rewriting the definition of done.
