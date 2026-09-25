# Implementation status — historical 2026-09-22 snapshot

> **SUPERSEDED.** This note described the state before the CR-2026-09-22 features and five-language runtime were implemented. It must not be used as the current product status.

Current authority is `DEVELOPMENT-ENTRYPOINT.md` plus `README.md`. The WinSidebar 2.0 runtime is now implemented, frozen and explicitly owner-approved. The active clean shipping candidate is `chore/release-2.0`; its current post-approval head changes documentation/release tooling rather than runtime behavior.

## Historical snapshot

CR-2026-09-22 was documented but not yet implemented at the time of this note. `src/WinSidebar.cs` retained Calculator/Settings hard-coded exclusions and `PASTAS / WEB`, and no tested per-window rename, reversible ignored-app manager or Save/Restore controls existed. The approved pre-existing preference behavior was distinct from untested new persistence code; cross-version migration tests were out of scope. Owner acceptance of all five runtime languages was pending their implementation.

Preserve this paragraph only as evidence of the earlier stage.
