# Project Handoff

Emergency freeze date: 2026-10-02.

This document preserves the actual observed state of `hamthet/WinSidebar` at the moment development was frozen. Do not interpret it as a request to continue automatically. The original working branch is intentionally left unchanged; this handoff branch adds only continuity documentation on top of the frozen work-state commit.

## 1. Objetivo do projeto

WinSidebar 2.0 is a portable Windows 10/11 x64 WinForms sidebar for:
- finding and activating eligible open windows;
- launching configurable folder/website shortcuts;
- pasting reusable literal-text snippets into external applications;
- supporting English, Brazilian Portuguese, Spanish, Russian and Simplified Chinese.

The published 2.0 product is self-contained, single-file, .NET 8, unsigned, and stores user state under `%LOCALAPPDATA%\WinSidebar`.

Current post-release work is not a new product version. It is a documentation/localization/artwork consistency audit intended to align the public repository surfaces with the already-published WinSidebar 2.0 runtime contract.

## 2. Estado atual

Published/product state:
- official public version: 2.0;
- public tag: `v2.0`;
- published source commit: `2597fe5ddda8906fd4e2a31ab8591ed4e7f6a2a4`;
- current public maintenance `main`: `790cd352b11f67625f121a8fb9dcd4d9022ed6e0`;
- GitHub Release: `WinSidebar 2.0 — Windows x64`, public, non-draft, non-prerelease;
- published assets: `WinSidebar-v2.0-win-x64.zip` and `SHA256SUMS.txt`.

Frozen in-progress state:
- original work branch: `docs/localization-art-audit-2.0`;
- frozen work-state SHA: `352f4c560e6d14db7117c5d450f55790abc7cc72`;
- PR #9: `docs(i18n): complete WinSidebar 2.0 localization and artwork audit`;
- PR #9 was observed open, non-draft and `mergeable=true`;
- PR #9 had zero unresolved review threads at freeze time;
- automated Copilot/Codex review attempts were unavailable because of quota, not because a defect was reported;
- PR #9 is 14 commits ahead of `main`, 0 behind;
- its diff contains 31 files and does not touch `src/**`, runtime `i18n/**` catalogs, `tests/**`, or `WinSidebar.csproj`.

This handoff branch is a preservation branch created from the frozen work-state SHA. It must not be treated as a new product line.

## 3. Trabalho realizado

The published WinSidebar 2.0 release had already been completed before this audit:
- canonical release verifier passed on the final release head under .NET SDK 8.0.425;
- owner opened the generated EXE successfully;
- PR #7 was merged;
- annotated tag `v2.0` was created;
- GitHub Release was published;
- later PR #8 synchronized public repository documentation with the published state.

The frozen PR #9 performs post-release public-surface hardening only. Work completed on that branch includes:

1. SVG localization/accessibility:
- all ten concept-art SVGs have explicit `xml:lang`;
- localized SVG accessibility `<title>` and `<desc>` text was translated for pt-BR/es-ES/ru-RU/zh-CN;
- duplicate `CONCEPT_ART_NOT_SCREENSHOT` markers in localized SVGs were normalized to one marker;
- `aria-labelledby="title desc"` remains required.

2. Artwork/runtime alignment:
- pt-BR artwork was corrected to the actual runtime defaults `Pasta local`, `Downloads`, `Acervo`, `Site`;
- Spanish artwork shortcut heading was aligned from abbreviated `ACCESOS` to runtime `ACCESOS DIRECTOS`;
- artwork gates now compare the four default shortcut labels and shortcut heading with the runtime localization catalogs;
- artwork gates require `AltGr+Y`.

3. Public documentation parity:
- English `START-HERE.txt` now explicitly states the bundled self-contained .NET 8 runtime;
- localized outreach paths to hero/linkedin artwork were corrected;
- localized outreach text again states that concept art is not an exact representation of current controls;
- support privacy guidance now preserves the warning that custom icons under `icons/` can reveal filenames/branding;
- first-run language chooser behavior is documented as an intentional bootstrap exception: English framing before a preference exists, with five language choices shown as autonyms.

4. GitHub issue forms:
- five language-specific bug-report forms now exist;
- English, pt-BR, es-ES, ru-RU and zh-CN forms were structurally aligned with equivalent fields, Windows choices, language choices and privacy confirmation;
- localized SUPPORT documents point to their corresponding bug-report form.

5. Verification hardening:
- `tools/verify-release.ps1` was extended to audit localized public-document contract tokens, localized issue forms, artwork paths, SVG accessibility/language metadata and catalog-to-art labels;
- the verifier remains ASCII-only;
- dotted JSON keys use PowerShell 5.1-safe `PSObject.Properties[...]` access;
- one false-negative assertion for the shortcut heading was found during static review and corrected in the frozen work-state commit `352f4c560e6d14db7117c5d450f55790abc7cc72`;
- `.github/workflows/marketing-assets.yml` contains equivalent read-only concept-art checks and never writes back to the repository.

No runtime functionality was intentionally changed by PR #9.

## 4. Arquitetura relevante

Main runtime files:
- `src/WinSidebar.cs`: WinForms UI, global hotkeys, sidebar layout, shortcut/snippet controls, profile settings integration.
- `src/Localization.cs`: runtime catalog loading, supported-language behavior and persisted language selection.
- `src/FirstRunLanguageDialog.cs`: bootstrap language chooser.
- `src/ShortcutConfig.cs`: shortcut data model, persistence, validation and editor.
- `src/SnippetStore.cs`: snippet slots, persistence, validation and hotkey contract.
- `src/SnippetEditor.cs`: snippet editing UI.
- `src/TextInjection.cs`: literal-text clipboard/paste injection and restoration behavior.
- `src/WindowManagement.cs`: temporary aliases, persistent ignored-app rules and window context operations.

Localization:
- `i18n/catalog.json`: en-US + pt-BR base catalog.
- `i18n/es-ES.json`, `i18n/ru-RU.json`, `i18n/zh-CN.json`: additional catalogs.
- observed catalog contract during audit: 152 keys in each runtime language, no blank entries at the time of static inspection.

Persistence root:
- `%LOCALAPPDATA%\WinSidebar`
- `settings.ini`
- `shortcuts.xml`
- `snippets.json`
- `ignored-apps.json`
- `icons/`

Distribution:
- `WinSidebar.csproj`: `net8.0-windows`, `win-x64`, self-contained, single-file, no trimming, no ReadyToRun.
- end-user artifact: `WinSidebar.exe`.
- archive: `WinSidebar-v2.0-win-x64.zip`.

Verification:
- `tools/verify-release.ps1`: canonical local release/build/package gate.
- `.github/workflows/build.yml`: manual workflow delegating to the canonical verifier.
- `.github/workflows/localization-tests.yml`: manual localization/build checks.
- `.github/workflows/marketing-assets.yml`: manual read-only artwork audit/render workflow.

## 5. Arquivos importantes

- `PROJECT.md`: maintainer entry point and current public-release baseline.
- `project.json`: machine-readable product/index contract; implementation and executable tests remain authoritative.
- `README.md`: English user-facing product overview and language navigation.
- `START-HERE.txt`: English quick-start shipped with release ZIP.
- `SUPPORT.md`: English support/privacy guidance.
- `docs/README.md`: documentation map.
- `docs/DESIGN-DECISIONS.md`: non-obvious product decisions, including first-run bootstrap-language behavior.
- `docs/KNOWN-LIMITATIONS.md`: explicit 2.0 boundaries.
- `docs/DEVELOPMENT.md`: build/test/development instructions.
- `docs/RELEASE.md`: release procedure and current published state.
- `docs/i18n/**`: localized README/tutorial/FAQ/START-HERE/support/release-notes/outreach documents.
- `assets/hero-illustration.svg`, `assets/linkedin-illustration.svg`: English concept artwork.
- `assets/i18n/<locale>/*.svg`: localized concept artwork.
- `.github/ISSUE_TEMPLATE/bug_report*.yml`: five localized bug-report forms.
- `tools/verify-release.ps1`: modified canonical verifier; this is the main unvalidated item.
- `.gitignore`: ignores build/release output and selected local profile files.

## 6. Decisões tomadas

Product/release decisions:
- public version is `2.0`; Windows technical file versions may be `2.0.0.0`.
- `v2.0` is the immutable source baseline for the published 2.0 release.
- current `main` can advance with maintenance/docs without redefining the published artifacts.
- English is default for a brand-new profile.
- first-run chooser is bootstrap UI: English framing before preference exists; five language choices are autonyms.
- legacy profiles without persisted language may retain Portuguese.
- application-owned strings are localized; user-authored names, paths, snippet content, external window titles and application identities are not translated.
- Scripts/snippets are literal text, not executable code.
- AltGr+Y toggles the sidebar.
- F1-F4 launch shortcuts 1-4.
- snippet hotkeys are None or Shift+F1..Shift+F12; first four default to Shift+F1..F4.
- window discovery is heuristic, not guaranteed exact Alt+Tab parity.
- executable is portable, unsigned, self-contained, no installer, no automatic startup, no telemetry.
- concept artwork must remain explicitly identified as concept art, not a screenshot or exact UI representation.

Repository/process decisions:
- no merge of PR #9 until the modified verifier has actually been executed on Windows PowerShell 5.1 with .NET 8 and the result recorded.
- absence of automated review is not approval.
- no change to published `v2.0` tag/release assets as part of PR #9.
- editing the already-published GitHub Release body is a separate publication action.
- do not delete divergent historical feature/checkpoint branches merely because v2.0 is published.
- four fully incorporated refs had been classified as safe cleanup candidates, but physical deletion was still pending because the connector did not expose ref deletion:
  - `checkpoint/pre-final-review-fixes-20260925`
  - `chore/release-2.0`
  - `chore/release-2.0.0`
  - `docs/post-release-2.0-sync`
  Do not perform this cleanup while the project is frozen unless the owner explicitly restarts that task.

## 7. Restrições

- PROJECT IS FROZEN by explicit emergency instruction dated 2026-10-02.
- Do not add features, refactor, modernize, experiment or broaden scope during the frozen state.
- Do not merge PR #9 merely because GitHub reports it mergeable.
- Do not change runtime code as part of the localization/artwork audit without a new explicit decision.
- Do not move or recreate tag `v2.0`.
- Do not alter published v2.0 assets during this audit.
- Do not claim tests that were not run.
- Do not rely on GitHub Actions credits; hosted workflows are manual and the user reported no available Actions credits.
- Corporate/native environment constraints previously given by the owner: PowerShell without admin; no CMD; Python 3.12, .NET SDK 8 and 10, Node 24; FileBridge may be used for temporary test files.
- Home mode, when a future user message explicitly begins with `em casa`, may use admin/CMD and broader local capabilities, but must not create repository state that conflicts with returning to the corporate environment. GitHub remains the shared boundary; do not commit machine-specific paths/configuration/artifacts.
- For the 2.0 canonical gate, use .NET 8 even if .NET 10 is installed.
- `development/**` is an internal dossier kept on `develop`; it must not enter shipping `main`/ZIP.
- The producer repository currently has no local `AGENTS.md` and no local `GITHUB_WORKFLOW_ECONET.md`. Canonical workflow was consulted from `hamthet/EconetGovernanca`; do not silently “fix” governance during product work.

## 8. Estado incompleto

Critical incomplete item:
- The modified `tools/verify-release.ps1` on PR #9 has NOT been executed under Windows PowerShell 5.1 after its audit extensions. Static inspection is not sufficient because the verifier itself changed.

Other incomplete/pending items:
- PR #9 has not been merged.
- PR #9 has no successful automated code review; Copilot/Codex attempts were blocked by quota.
- GitHub Actions were not used for the current branch.
- The already-published GitHub Release body remains English-first. It links to localized release-note paths, but publication-level link behavior was not conclusively revalidated during the final audit. Treat any Release edit as a separate publication task.
- The four fully incorporated cleanup refs listed above remain physically present unless a later Git-capable executor removes them.
- No decision was made to translate maintainer-only architecture/release/governance documents into all five languages. The five-language target applies to user/public product surfaces; maintainer technical documentation may remain English unless the owner changes that decision.

## 9. Bugs e problemas conhecidos

No new runtime bug was introduced or confirmed in PR #9.

Audit defects found and corrected on the frozen branch:
- localized SVG accessibility title/description remained English;
- localized SVGs lacked explicit `xml:lang`;
- duplicate concept-art markers existed;
- outreach asset paths were incorrect/ambiguous;
- English START-HERE was less explicit than localized files about bundled .NET 8;
- support privacy text in some languages omitted the custom-icon warning;
- localized outreach omitted the warning against treating concept art as an exact control representation;
- pt-BR artwork used labels that did not match runtime defaults;
- Spanish artwork abbreviated the current shortcut-section label;
- bug-report issue form existed only in English;
- an early PowerShell gate implementation used risky dynamic dotted-property access and was replaced with `PSObject.Properties`;
- a static gate initially required `>SHORTCUTS<`-style exact closure even though the artwork contains controls after the heading; this false-negative was fixed by commit `352f4c560e6d14db7117c5d450f55790abc7cc72`.

Known product limitations remain documented in `docs/KNOWN-LIMITATIONS.md`, including:
- unsigned executable / SmartScreen warnings;
- global-hotkey conflicts;
- paste into elevated applications can fail;
- clipboard restoration is best effort;
- window discovery is heuristic;
- temporary rename is temporary;
- no automatic startup/installer;
- cross-version profile migration is outside the 2.0 validation scope.

## 10. Testes

Historical published v2.0 evidence:
- final release gate passed under .NET SDK 8.0.425 on release head `81a80987fe37eca822bc7462355a0e1c63dbc1bc`;
- owner opened the generated EXE and confirmed startup;
- published source was merged and tagged after that validation.

PR #9 / frozen branch evidence:
- static diff audit: PASS for scope isolation; no runtime/catalog/test/project-file changes observed;
- catalog parity inspection: 152 entries per supported runtime language and no blanks at the time checked;
- README/tutorial/release-notes section/token parity checks: passed statically;
- START-HERE/support/outreach contract-token checks: passed statically after corrections;
- SVG checks: all ten were statically checked for concept marker, `aria-labelledby`, `xml:lang`, localized title/description and current `AltGr+Y`;
- artwork default labels were compared against current runtime catalogs after corrections;
- five issue forms were structurally compared for matching field IDs/shape and privacy/platform/language coverage;
- verifier text was checked as ASCII-only and for balanced braces/parentheses;
- PR #9 observed mergeable with zero unresolved review threads.

NOT TESTED on the final PR #9 head:
- `tools/verify-release.ps1` has not been executed under Windows PowerShell 5.1 after the new gates were added;
- no current-branch canonical publish/package PASS exists;
- no current-branch Windows real-use regression test was run;
- automated Copilot/Codex review did not run successfully due quota;
- GitHub Actions were not run.

Relevant commands:

    dotnet build .\WinSidebar.csproj -c Release

    dotnet run --project .\tests\LocalizationSmoke.csproj -c Release

    dotnet run --project .\tests\SnippetStoreSmoke.csproj -c Release

Canonical gate, from repository root in Windows PowerShell 5.1 with .NET 8:

    .\tools\verify-release.ps1 -Dotnet dotnet -OutputRoot 'artifacts\release'

The canonical gate publishes/package-tests as part of its procedure. Do not report PASS unless its final PASS output is actually observed.

## 11. Como executar

Requirements for normal development/test:
- Windows 10/11 x64;
- .NET 8 SDK;
- Git;
- PowerShell for canonical verifier.

Build:

    dotnet build .\WinSidebar.csproj -c Release

Run smoke tests:

    dotnet run --project .\tests\LocalizationSmoke.csproj -c Release
    dotnet run --project .\tests\SnippetStoreSmoke.csproj -c Release

Publish-only diagnostic:

    dotnet publish .\WinSidebar.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish

Canonical verification:

    .\tools\verify-release.ps1 -Dotnet dotnet -OutputRoot 'artifacts\release'

End-user profile is separate from the repository under `%LOCALAPPDATA%\WinSidebar`.

## 12. Próximos passos

The project is frozen. Do not execute these until the owner explicitly resumes work.

Priority 1 — validate the modified canonical verifier.
- Objective: prove that PR #9's modified `tools/verify-release.ps1` works under Windows PowerShell 5.1 and .NET 8.
- Likely files involved: ideally none; test the frozen branch exactly as-is. If the verifier fails because of a defect in the new gate, only then inspect `tools/verify-release.ps1` and the specific referenced docs/artwork.
- Completion condition: observed `WINSIDEBAR 2.0 RELEASE VERIFICATION: PASS` on the frozen PR branch, with SDK 8.x, plus recorded output. A failing result must be preserved verbatim and must not be described as PASS.

Priority 2 — review PR #9 after the gate.
- Objective: confirm final diff still contains only the intended documentation/artwork/issue-form/verifier/workflow changes and no runtime changes.
- Likely files: the 31 PR files.
- Completion condition: branch remains clean relative to intended scope, no unresolved review findings, test evidence recorded.

Priority 3 — human merge decision.
- Objective: decide whether PR #9 should merge to `main`.
- Completion condition: explicit owner approval after current-branch gate evidence. Mergeability alone is insufficient.

Priority 4 — publication surface follow-up, separate task.
- Objective: verify the already-published GitHub Release body's localized links and decide whether English-first release text should be changed.
- Likely surface: GitHub Release `v2.0`, not runtime source.
- Completion condition: link behavior is actually verified and any publication edit receives explicit authorization. Do not modify tag/assets.

Priority 5 — optional repository cleanup, separate task.
- Objective: remove only previously proven fully incorporated refs if still desired.
- Completion condition: each ref is revalidated as fully incorporated immediately before deletion and owner authorization remains applicable. Never delete divergent historical branches by name alone.

## 13. Último ponto exato de trabalho

Immediately before the emergency freeze, the task was auditing whether all WinSidebar 2.0 public surfaces were translated and aligned with the actual software, including AI-generated/localized concept artwork.

The exact last technical change already committed before freeze was:

`352f4c560e6d14db7117c5d450f55790abc7cc72 — fix(docs): accept shortcut heading controls in artwork gate`

That commit fixed a false-negative in the new canonical verifier: concept artwork headings include controls after the localized shortcut heading, so the gate must accept a heading prefix rather than require an immediate closing text tag.

After that commit, the branch was revalidated:
- PR #9 open/non-draft/mergeable;
- 14 ahead / 0 behind relative to `main`;
- 31 changed files;
- zero runtime/catalog/test/project-file changes;
- verifier ASCII-only and delimiter-balanced;
- zero unresolved review threads;
- actual Windows PowerShell 5.1 execution of the modified verifier still pending.

No further functional development should occur until the owner explicitly resumes.

## 14. Git

Repository:
- `hamthet/WinSidebar`

Official current branch:
- `main` = `790cd352b11f67625f121a8fb9dcd4d9022ed6e0`

Published release baseline:
- tag `v2.0`;
- published source commit `2597fe5ddda8906fd4e2a31ab8591ed4e7f6a2a4`.

Frozen original work branch:
- `docs/localization-art-audit-2.0`
- work-state SHA: `352f4c560e6d14db7117c5d450f55790abc7cc72`
- PR #9 against `main`.

Emergency preservation branch:
- `handoff/emergency-checkpoint-20261002`
- created directly from `352f4c560e6d14db7117c5d450f55790abc7cc72`;
- commits after `352f4c560e6d14db7117c5d450f55790abc7cc72` on this branch are continuity documentation only (`HANDOFF.md` and `RESUME_PROMPT.md`).

Internal dossier branch:
- `develop` was observed at `821a780ff1bc7409389e96d618a6878ed1a6d4e7` before the freeze.

Working-tree limitation:
- this conversation used GitHub server-side connector writes, not a local checkout controlled by the assistant. Therefore all assistant-produced repository changes visible here are commits on GitHub; there is no assistant local untracked working tree to save.
- the user's home/corporate local checkouts were NOT inspected. A future session must not claim those machines are clean without checking them locally.
- server-side comparison of the frozen work branch against `main` showed 14 commits ahead, 0 behind, with all known branch work committed.

No secrets/tokens/credentials were intentionally added. Do not copy authentication tokens from old terminal logs or chats into Git.

## 15. Contexto que não está no código

Operational mode convention established by the owner:
- no prefix = NATIVE/corporate mode, following corporate constraints;
- a message beginning with `em casa` = HOME mode with broader local/admin/CMD access;
- work done at home must not create conflicts or requirements that prevent returning to the corporate environment;
- GitHub is the shared boundary between environments;
- do not commit machine-specific configuration, generated artifacts, credentials or personal paths.

Corporate/native environment described by owner:
- PowerShell available;
- no administrator rights;
- no CMD;
- Python 3.12;
- .NET SDK 8 and 10;
- Node 24;
- FileBridge repository can temporarily expose files for tests;
- GitHub Actions have no available credits, so local PowerShell testing is expected.

Home mode:
- administrator rights and CMD are available;
- may replicate or exceed the corporate toolset;
- should still run product validation as a normal user when the product contract is normal-user operation, using elevation only when needed for setup or explicitly elevated test cases.

Important release history not to reconstruct from memory:
- v2.0 is already published and closed as a release.
- PR #9 is maintenance hardening, not a new release and not a reason to retag v2.0.
- PRs #1–#5 were closed as superseded without merge; their divergent branches were preserved.
- PR #8 was merged to align repository documentation with the published state.
- do not merge old stacked feature branches wholesale.

Governance evidence consulted for this emergency checkpoint:
- `hamthet/EconetGovernanca` canonical `GITHUB_WORKFLOW_ECONET.md` at blob `947a3def132e7efe476a2d110de47e47af7a5268`;
- operational `FONTE_UNIVERSAL.md` content blob `6fe1c2cb2e95c44aea708a0d6852e799a024844c`;
- WinSidebar currently has no local `AGENTS.md` and no local `GITHUB_WORKFLOW_ECONET.md`.

Freeze rule:
- preserve first;
- do not “finish quickly”;
- do not merge, delete branches, retag, publish, refactor or broaden scope until explicit owner instruction resumes the project.
