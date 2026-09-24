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
- Mouse-driven row paste now continuously remembers the last eligible external foreground window and also samples it on mouse entry/`WM_MOUSEACTIVATE`; it returns focus, confirms foreground ownership and only then pastes.
- Five-language UI strings for the snippet section/editor/errors.
- User snippet names/content remain literal user data and are never translated.
- No script execution; content is text only.

## Deliberately not changed yet

- Existing Restore Defaults does **not** clear `snippets.json` in this preview. Deleting user-authored snippets is deferred until the owner chooses the desired reset semantics.
- No public docs/release/main integration.
- No GitHub Actions or FILEBRIDGE.

## Owner result — first four-slot preview

Owner GUI test on branch state `34444baf3bacf9cfd6b500872901f83937bc6fb1`:

- **PASS:** configured snippet hotkey path works in the real chat/editor workflow.
- **FAIL:** clicking the snippet row did not paste. WinSidebar took foreground focus and reported that no paste target window was available; the `WM_MOUSEACTIVATE`-only target capture was therefore insufficient on the owner's machine.
- **Requested UI change:** replace the former decrease/increase-width pair with two one-way cyclic controls: one cycles horizontal width and one cycles vertical height.

Source correction is now staged on `feature/text-snippets`:

- a 100 ms foreground watcher continuously remembers the last eligible external foreground HWND; mouse-enter capture remains an additional pre-click signal;
- mouse-triggered paste still validates the remembered HWND and lets `TextInjector` restore/confirm focus before Ctrl+V;
- horizontal sizes cycle `211 -> 260 -> 324 -> 211`;
- vertical sizes cycle `504 -> 640 -> 780 -> 504`, clamped to the current monitor working area;
- both size indices persist in `settings.ini`;
- Restore Defaults now resets and rolls back the vertical size index together with the existing width state;
- new cyclic size tooltips are localized in all five catalogs.

These corrections are **SOURCE STAGED / OWNER RETEST PENDING**. Do not mark mouse-click paste or vertical sizing accepted until the owner runs the updated preview.

## Owner result — click-target and size-cycle retest

The owner retested the corrected preview and reported **“parece ótimo”** before requesting the next UI refinement. Treat this as positive GUI acceptance of the previously failing mouse-click paste path plus the horizontal/vertical cyclic sizing behavior, not as final release acceptance.

The next refinement is now source-staged:

- the original shortcut-edit gear/mode is removed;
- shortcut left-click keeps opening the target;
- shortcut right-click (or context-menu key) directly opens that shortcut editor;
- a `+` control in **Atalhos / Shortcuts** adds one new row of four shortcuts;
- a `+` control in **Scripts** adds one additional script row;
- storage remains backward-readable for the existing four-entry formats and switches to an expanded format only after extra rows are created;
- additional snippet rows are mouse-driven only; global `Ctrl+Shift+F1..F4` remains reserved for the first four scripts;
- each section is bounded at 40 entries and scrolls when its visible area is exceeded.

This refinement is **SOURCE STAGED / OWNER BUILD + GUI RETEST PENDING**. Removal of added rows is not implemented in this preview because destructive semantics were not requested.

## Owner result — expandable rows preview and final UI corrections

The owner tested the expandable preview and supplied a screenshot showing 12 shortcuts and 8 scripts rendered with scroll. The next product decisions are explicit:

- cap the product at **12 shortcuts / 8 scripts**;
- add `-` beside each `+`; shortcuts remove one last row of four, scripts remove the last script, never below four;
- add an independent script restore button and keep shortcut/script restores independent, each with confirmation;
- remove the language globe because Language already exists in the general context menu;
- add a first-use language chooser;
- retain right-click editing for all shortcut buttons, including folders; scripts retain per-row gears;
- revise default global keys to `F1..F4` for shortcut buttons 1..4 and `Shift+F1..F4` for scripts 1..4. Extra rows have no default global key.

The pre-change expandable state is preserved at `checkpoint/expandable-rows-tested-20260923` (`b87e34510dc9d435ee096c6cd9a4dd6cce96afe6`).

The requested changes are now **SOURCE STAGED / OWNER BUILD + GUI RETEST PENDING**. No Actions, FILEBRIDGE, main merge or release.

## Owner result — fresh profile view and next retest

The owner supplied a fresh-profile screenshot showing the revised independent section controls. The earlier shortcut-load error was not reported during that fresh launch, but recurrence has **not** been ruled out. The next run must preserve the newly created profile and reopen against it.

New source-staged changes:

- normal 12-shortcut / 8-script layouts allocate the full required section height and disable their internal scrollbars;
- the overall sidebar height automatically grows when needed, capped by the monitor working area; only physically constrained desktops fall back to section scrolling;
- the script gear editor now includes a persisted keyboard-shortcut selector: None or Shift+F1..Shift+F12;
- scripts 1..4 migrate/default to Shift+F1..F4 when an older snippet file has no hotkey field; scripts 5..8 default to none;
- duplicate script hotkeys are rejected before save;
- runtime hotkey registrations are rebuilt after edit/add/remove/restore, so changes take effect without restarting;
- the old window-list keyboard navigation remains intentionally removed;
- the owner profile is explicitly preserved by the current runner so the load-error recurrence can be observed.

The pre-change fresh-profile state is preserved at `checkpoint/fresh-profile-controls-pass-20260923` (`088b90e73960c662b530528d355fa260a9ed6c44`).

**Status:** SOURCE STAGED / OWNER BUILD + PRESERVED-PROFILE RETEST PENDING.

## Owner follow-up — Alt+double-quote sidebar opener

After reporting the latest preserved-profile/autofit/configurable-hotkey preview as **“parece tudo ok”**, the owner requested one additional global command:

- `Alt+"` opens the sidebar when it is collapsed;
- if the sidebar is already open, the command is a no-op;
- existing F1..F4 shortcut keys and configured Shift+F script keys are preserved;
- implementation uses Win32 RegisterHotKey with Alt+Shift+the quote/apostrophe OEM key (`Keys.Oem7`).

The pre-change accepted state is preserved at `checkpoint/snippets-configurable-hotkeys-pass-20260924` (`2b4202277d2bb0330a25a27a2eaef6513a2e146d`).

**Status:** SOURCE STAGED / OWNER HOTKEY RETEST PENDING.

## Owner-run preview

Use only:

`development/run-text-snippets-preview-20260923.ps1`

The historical paste-probe runner is no longer the current feature preview.

Required acceptance:

1. Four rows render below Atalhos/Shortcuts without clipping at the narrow width.
2. The `⇔` header control cycles the three widths in one direction; the `⇕` control cycles the three heights in one direction.
3. Gear opens a visible editor; Save/Cancel work.
4. Name/content and selected width/height persist after app restart.
5. Ctrl+Shift+F1..F4 paste the corresponding content.
6. Rapid repeat while a transaction is active produces no modal.
7. Clicking a snippet row while a browser/chat field was active returns focus to that field and pastes there.
8. Unicode/multiline content remains exact.
9. Existing Shift+F1..F4, language selection and window/global context menus remain functional.
10. Clipboard restoration remains a separate explicit observation.

Do not merge or release on compile success alone.
