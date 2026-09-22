# Decision register / ADRs

Decisions are either `ACCEPTED` (user instruction or an established product constraint), `PROPOSED` (implementation preference awaiting proof), or `OPEN` (must resolve before release). Record a new dated entry when an implementation reveals a trade-off; never silently redefine product scope. Keep user-supplied custom data and operating-system-provided labels separate from localizable application text.

| ID | Status | Decision and rationale | Reconsider when |
| --- | --- | --- | --- |
| ADR-001 | ACCEPTED | **Product before repository.** Build and verify five-language application first, then translate README/tutorial/images and package. Documentation-first previously produced English promotional material for Portuguese binaries. | Only explicit user change of sequence |
| ADR-002 | ACCEPTED | **Development-only dossier on `develop`.** Keep `development/**` and process journal out of final `main` and ZIP, and remove legacy `docs/i18n/LOCALIZATION-PLAN.md` at release promotion. Use selective cherry-pick/copy in clean release branch; avoid wholesale `develop` merge. | Promotion audit |
| ADR-003 | ACCEPTED | **English is GitHub root README and unsupported-locale fallback.** Five app choices: en-US, pt-BR, es-ES, ru-RU, zh-CN (Simplified). New installs may auto-detect a supported Windows UI locale. Existing settings without a language field retain Portuguese initially. Manual selection overrides auto-detection. | Compatibility testing |
| ADR-004 | ACCEPTED | **Preserve the scope and retro aesthetic.** Portable Windows 10/11 x64, single-file .NET 8, four shortcuts and existing window-switching controls. No forced redesign, installer, account or cloud dependency. | Reproducible usability blocker / new scope request |
| ADR-005 | PROPOSED | Embed locale resources in EXE, centralized lookup, build-time key/placeholder validation. JSON is acceptable if compile embedding and single-file tests succeed; `.resx` + ResourceManager is an alternative; choose one after a small proven spike and record the result. | Prototype/CI result |
| ADR-006 | ACCEPTED | Never localize serialized tokens (`folder`, `website`, icon IDs), process names, URLs, user-selected shortcut names/paths or window titles. Translate only application-owned display names with a tested non-destructive legacy-default strategy. | Storage schema changes |
| ADR-007 | OPEN | Does 'find and switch open windows' include Calculator/Settings? Existing filters exclude them by title/process. Decide intended inclusion policy, verify OS-language independence, and revise user-facing promise or implementation accordingly. | Window enumeration user tests |
| ADR-008 | OPEN | 3+ monitor semantics: groups may show all screens but placement menu chooses primary or first nonprimary. Support arbitrary monitor positioning or document primary/secondary limit; decide after real test. | Monitor tests |
| ADR-009 | OPEN | Corrupt settings, shortcut XML and backups: determine expected recovery and user feedback; maintain data even if settings cannot be written. Never claim robust recovery before fault-injection tests. | Failure/recovery tests |
| ADR-010 | OPEN | Minimal localized help/first-run discoverability: inspect icon-only gear and language menu using keyboard and new-user tasks; include only if it solves observed friction. | Usability study |
| ADR-011 | ACCEPTED | Never relabel/overwrite the old v1.0.0 release. New source change requires a new version/release/tag and checksummed artifact; unsigned warnings remain. | New release preparation |

## New ADR template

`ADR-### | YYYY-MM-DD | ACCEPTED/PROPOSED/OPEN/SUPERSEDED | context/problem | options considered | chosen behavior | trade-offs | backwards compatibility | evidence/test IDs | implementation commit/PR`. Record opposing options fairly; link original decision when superseding it rather than rewriting history.
