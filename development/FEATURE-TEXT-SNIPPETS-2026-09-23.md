# Feature proposal — four text snippets / quick paste

**Status:** PLANNED / NOT IMPLEMENTED.  
**Recorded:** 2026-09-23.  
**Frozen product baseline:** `6af6343e8d5e6e0dc8cbd19f3d40e378cb583194` on `checkpoint/functional-menu-approved-20260923`.  
**Implementation branch reserved:** `feature/text-snippets`, created from the exact frozen baseline.

## Owner intent

Add four compact rows below the existing **Atalhos / Shortcuts** section. Each row represents one user-defined text snippet (owner informally called it a "script"), with:

- a renameable display name such as `Script 1`;
- a primary row/button action that inserts the saved text into the application the user was typing in;
- a configuration button on the right of that row;
- an editor dialog with at least **Name** and a multiline **Content** field;
- explicit Save/Cancel behavior;
- persistent local storage;
- global hotkeys `Ctrl+Shift+F1` through `Ctrl+Shift+F4`, one per row.

The content is **literal text to insert**, not executable code. WinSidebar must never run the snippet as PowerShell, shell, JavaScript, or another program.

## Interaction contract

1. User is typing in another application (for example a web chat).
2. User invokes `Ctrl+Shift+F1..F4`, or clicks the corresponding snippet row/button.
3. WinSidebar inserts that snippet's saved content into the intended external text target.
4. Invoking the configuration button opens the snippet editor instead of inserting text.
5. The four existing `Shift+F1..F4` WinSidebar navigation hotkeys stay unchanged. The new modifier combination is distinct: Ctrl + Shift.

### Focus requirement

Clicking WinSidebar itself changes foreground focus. Therefore the implementation must remember the most recent eligible **external foreground window** and, for mouse-triggered insertion, restore/focus that window before injecting the text. It must never paste into WinSidebar's own controls by mistake.

For keyboard-triggered insertion, the external application should normally still be foreground; nevertheless validate the target HWND before input.

## Proposed UI

Below the current shortcuts block, add a separate localized section (working title: **Scripts**, final wording to be reviewed per locale) with four rows.

Each row:

`[ renameable name / paste action                         ][ ⚙ ]`

The main row surface inserts the snippet. The gear opens the editor. Tooltips and accessible names must state both the row action and its hotkey.

The sidebar must remain usable at its narrowest width. If vertical space becomes constrained, reduce the window-tree viewport rather than overlapping controls; preserve monitor grouping and status visibility.

## Persistence

Use a separate bounded local store (for example `snippets.json`) under the existing WinSidebar local data root rather than overloading `settings.ini` or `shortcuts.xml`.

Each of four fixed slots should store stable fields equivalent to:

- slot/index;
- user-visible name;
- literal text content.

Requirements:

- four default slots, expandable up to eight scripts;
- names trimmed and bounded;
- content supports multiline Unicode;
- malformed/oversized storage is rejected safely;
- atomic write/replace with actionable error reporting;
- configuration changes do not modify shortcut, language, ignored-app, or window-alias state.

## Text injection design gate

Investigation now recommends **temporary Clipboard + Win32 SendInput Ctrl+V as the primary v1 path**, because the owner's main target is a browser/chat editor and multiline text must retain paste semantics without synthesizing Enter. Direct KEYEVENTF_UNICODE remains an experimental spike, not an automatic fallback.

The paste path must wait for the triggering Ctrl/Shift/F-key to be released, validate/restore the intended foreground HWND, and use a sequence-number/private-marker guard before restoring the previous clipboard so a newer user clipboard value is never overwritten. Prototype against multiline text, Unicode/emoji, long snippets, browser editors, ordinary Windows edit controls and non-text clipboard contents.

Detailed architecture and risk analysis: [`SNIPPET-IMPLEMENTATION-INVESTIGATION-2026-09-23.md`](SNIPPET-IMPLEMENTATION-INVESTIGATION-2026-09-23.md).

## Hotkeys

The owner later replaced the initial hotkey plan. Current contract:

- global `F1..F4` open shortcut buttons 1..4;
- global `Shift+F1..F4` paste scripts 1..4;
- scripts 5..8 and shortcuts 5..12 are fully configurable/clickable but receive no default global hotkey;
- the previous window-navigation use of `Shift+F1..F4` is retired.

Report registration conflicts in the existing hotkey-error UX rather than silently failing.

## Acceptance scenarios

- SNIP-01: configure all four names and multiline contents; restart; values persist.
- SNIP-02: while typing in a browser/chat field, `Ctrl+Shift+F1..F4` inserts the correct literal content into the current target.
- SNIP-03: clicking a snippet row returns focus to the previously active external target and inserts there, never into WinSidebar.
- SNIP-04: each row's gear opens a fully visible editor above/beside the topmost sidebar; Save updates one slot, Cancel changes nothing.
- SNIP-05: Unicode, accented Portuguese, Cyrillic, Simplified Chinese, emoji and multiline text survive save/restart/insertion.
- SNIP-06: F1..F4 open shortcuts 1..4; Shift+F1..F4 paste scripts 1..4; extra rows receive no implicit hotkeys.
- SNIP-07: hotkey conflict, unavailable target, invalid/oversized store and denied write produce a controlled localized error rather than data loss or crash.
- SNIP-08: right-click window/global menus, renaming, ignored-app management, five languages and existing shortcuts remain regression-free.

## Scope boundary

This feature starts **after** the frozen functional-menu checkpoint. Do not modify the checkpoint branch. No GitHub Actions, FILEBRIDGE, merge to `main`, or release is authorized by this proposal.

## Later owner UI decisions — 2026-09-23

- Product limits: 12 shortcut buttons (3 rows of 4) and 8 scripts.
- Each section has `+`, `-`, and its own restore-defaults control.
- `-` removes only the last added row/item and never removes the four default entries.
- Restore actions are independent and require confirmation: shortcut restore does not alter scripts/preferences; script restore does not alter shortcuts/preferences.
- The shortcuts section no longer has the original editing gear/mode. Left-click opens; right-click edits any shortcut, including folder shortcuts.
- Script rows retain their individual gear editor buttons.
- The language globe is removed; language remains available from the general right-click menu.
- A mandatory language chooser appears only when no WinSidebar `settings.ini` exists yet (first use), then persists the selected locale.

## Owner follow-up — preserved-profile retest and configurable script hotkeys

- The owner completed a fresh-profile run far enough to show the revised 12-shortcut / script UI and now explicitly wants the **existing profile preserved** on the next opening to see whether the earlier “load shortcuts” error recurs. Test runners must not delete or reset `%LOCALAPPDATA%\\WinSidebar`.
- The old keyboard navigation of the window list remains intentionally retired.
- Shortcut/script sections should auto-fit vertically so their own scrollbars do not appear under the normal supported 12/8 product limits. If the monitor working area is physically too short, internal scrolling remains a fallback instead of overlapping controls.
- Script configuration now requires a persisted keyboard-shortcut selector. Supported script hotkeys for this stage are **none** or **Shift+F1..Shift+F12**. Scripts 1..4 default to Shift+F1..F4; scripts 5..8 default to none. Duplicate script assignments are rejected before save.
- F1..F4 remain the fixed global defaults for shortcut buttons 1..4.

## Sidebar opener hotkey — 2026-09-24

The first owner test of `Alt+"` failed. The owner selected `Ctrl+"` instead. Current implementation registers Ctrl+Shift+`Keys.Oem7` and expands a collapsed sidebar; it deliberately does nothing while already expanded. This does not replace F1..F4 shortcut activation or configurable Shift+F script hotkeys.


## Sidebar opener hotkey follow-up — 2026-09-24

The owner reported that both `Alt+"` and `Ctrl+"` failed in the real Windows test. The current candidate uses **AltGr+Y**, implemented through Win32 `RegisterHotKey` as **Ctrl+Alt+Y**. It expands a collapsed sidebar and is a no-op while already expanded. Existing F1..F4 shortcut hotkeys and configurable Shift+F script hotkeys are preserved.
