# Four-slot text snippets preview — 2026-09-23

**Status:** SOURCE STAGED / LOCAL BUILD + GUI ACCEPTANCE PENDING.  
**Feature branch:** `feature/text-snippets`.  
**Known-good injection checkpoint:** `checkpoint/snippet-hotkey-paste-pass-20260923` at `fe74d2b02b1e6b409aba6e769f86622edad10e8a`.  
**Original functional-menu checkpoint remains untouched:** `checkpoint/functional-menu-approved-20260923`.

## Implemented on the feature branch

- Four compact rows below the existing shortcuts block.
- Renameable display name per row.
- Per-row gear button.
- Fixed dialog with Name + multiline Content + Save/Cancel.
- Persistent four-slot `%LOCALAPPDATA%\WinSidebar\snippets.json`.
- Atomic replace + backup and isolated store smoke tests.
- `Ctrl+Shift+F1..F4` mapped to slots 1..4 without replacing existing `Shift+F1..F4`.
- Reusable Clipboard + SendInput injector based on the owner-passed spike.
- Busy paste requests are silently ignored; no modal popup.
- Mouse-driven row paste remembers the prior external foreground window through `WM_MOUSEACTIVATE`, returns focus to it, confirms foreground ownership and only then pastes.
- Five-language UI strings for the snippet section/editor/errors.
- User snippet names/content remain literal user data and are never translated.
- No script execution; content is text only.

## Deliberately not changed yet

- Existing Restore Defaults does **not** clear `snippets.json` in this preview. Deleting user-authored snippets is deferred until the owner chooses the desired reset semantics.
- No public docs/release/main integration.
- No GitHub Actions or FILEBRIDGE.

## Owner-run preview

Use only:

`development/run-text-snippets-preview-20260923.ps1`

The historical paste-probe runner is no longer the current feature preview.

Required acceptance:

1. Four rows render below Atalhos/Shortcuts without clipping at the narrow width.
2. Gear opens a visible editor; Save/Cancel work.
3. Name/content persist after app restart.
4. Ctrl+Shift+F1..F4 paste the corresponding content.
5. Rapid repeat while a transaction is active produces no modal.
6. Clicking a snippet row while a browser/chat field was active returns focus to that field and pastes there.
7. Unicode/multiline content remains exact.
8. Existing Shift+F1..F4, language selection and window/global context menus remain functional.
9. Clipboard restoration remains a separate explicit observation.

Do not merge or release on compile success alone.
