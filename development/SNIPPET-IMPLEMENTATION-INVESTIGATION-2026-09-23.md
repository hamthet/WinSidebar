# Investigation — implementation of four text snippets / quick paste

**Date:** 2026-09-23  
**Status:** DESIGN INVESTIGATED / PRODUCT CODE NOT MODIFIED  
**Baseline reviewed:** `checkpoint/functional-menu-approved-20260923` = `6af6343e8d5e6e0dc8cbd19f3d40e378cb583194`  
**Reserved implementation branch:** `feature/text-snippets` (still rooted at that frozen baseline at the start of this investigation)

## 1. Existing architecture that the feature should reuse

The current WinSidebar already has the pieces needed to add this feature without redesigning the application:

- `SidebarWindow` owns the fixed four shortcut controls, global hotkeys, foreground/window P/Invoke declarations and manual layout.
- Existing navigation hotkeys are `Shift+F1..F4`, registered with `RegisterHotKey`, IDs `9801..9804`, modifier mask `0x4004` (Shift + MOD_NOREPEAT).
- `ShortcutStore` already demonstrates bounded local persistence under `%LOCALAPPDATA%\WinSidebar`, validation, temporary-file write, `File.Replace` backup and update-in-memory only after a successful write.
- `ShortcutEditor` demonstrates a fixed WinForms editor dialog; `SidebarWindow.EditShortcut` already has the correct edge-aware/topmost placement that should be reused for the snippet editor.
- `Localization` embeds a base pt-BR/en-US catalog plus es-ES, ru-RU and zh-CN resources. Current catalog has 115 keys and the smoke test enforces five-locale parity.
- `Program.Main` is STA, so the WinForms Clipboard API can be used from the existing UI thread without introducing a clipboard worker thread.

The feature should therefore be additive: new snippet model/store/editor/injector classes plus a narrow integration in `SidebarWindow`.

## 2. Recommended class/file split

Avoid adding another large block to the already-large `WinSidebar.cs`.

### `src/SnippetStore.cs` — no WinForms UI
Responsibilities:

- `SnippetEntry { Name, Content }` with `Copy()`.
- exactly four fixed slots;
- defaults;
- JSON v1 read/validate/write;
- bounded names and text;
- atomic temp + replace + backup;
- overloads that accept an explicit file path so the store can be smoke-tested in a temp directory.

Recommended file: `%LOCALAPPDATA%\WinSidebar\snippets.json`.

Suggested limits for the first implementation:

- name: 1..48 characters after trimming;
- content: 0..65,536 UTF-16 characters per slot;
- physical JSON file: <= 2 MiB;
- exactly four entries, stable IDs 0..3.

Do **not** trim Content. Leading/trailing whitespace and multiline formatting are user data.

### `src/SnippetEditor.cs`
A fixed dialog with:

- Name field;
- multiline Content field;
- vertical scrollbar;
- Save and Cancel;
- no execution/test button (the feature stores literal text only);
- `ShowInTaskbar = false`;
- positioned with the same monitor-clamped/topmost behavior already proven for the shortcut editor.

### `src/TextInjection.cs`
Own all cross-application paste behavior:

- clipboard transaction;
- SendInput Ctrl+V;
- modifier-release gating;
- clipboard sequence/restore guard;
- input result validation;
- no logging of snippet content.

### `src/WinSidebar.cs`
Only integration:

- four row controls and four per-row gear controls;
- load snippet entries;
- click/edit handlers;
- last external foreground HWND capture;
- four new global hotkeys;
- WndProc dispatch into the injector;
- layout and localization refresh;
- restore-defaults participation.

## 3. UI layout recommendation

Keep the existing four icon shortcuts. Add one new compact section directly below them:

```
[ SCRIPTS / localized heading                     ]
[ Script 1 / paste action                     ][⚙]
[ Script 2 / paste action                     ][⚙]
[ Script 3 / paste action                     ][⚙]
[ Script 4 / paste action                     ][⚙]
```

The large left surface is a Button with left-aligned, ellipsized text; the right button opens configuration. Do not reuse the existing global shortcut configure mode because the owner explicitly requested a gear on each snippet row.

Suggested geometry at the existing default 504 px sidebar height:

- snippet header: 20 px;
- rows: 23–25 px each;
- total new block: roughly 116–124 px;
- keep the existing 61 px shortcut block;
- reduce only the window-tree viewport. The TreeView already scrolls vertically, so this is preferable to overlapping controls.

Do not increase the default height in the first pass; that would be an unrelated visible behavior change. Reassess after owner preview.

## 4. Focus restoration: mouse-triggered paste

This is the most important interaction detail.

When the user clicks a snippet row, Windows normally activates WinSidebar before the Button Click runs. Calling `GetForegroundWindow()` from the Click handler is therefore too late.

Recommended approach:

1. Add `WM_MOUSEACTIVATE = 0x0021` handling to `SidebarWindow.WndProc`.
2. Before passing that message to the base WndProc, capture `GetForegroundWindow()`.
3. Keep it only if it is a valid external HWND and belongs to a PID different from WinSidebar.
4. On snippet-row Click, create a pending paste request targeting this remembered HWND.
5. Call `SetForegroundWindow(target)`.
6. Do **not** inject yet; a short UI timer/state machine verifies that `GetForegroundWindow() == target`.
7. If focus cannot be restored by the deadline, abort with a localized error rather than pasting into WinSidebar or another window.

This uses a message Windows sends before the subsequent mouse-button message/activation, which is exactly the point at which the former foreground target is still recoverable.

Do not use `AttachThreadInput` or global hooks in the first implementation. They add lifecycle and focus-coupling risk that is unnecessary for the owner's workflow.

## 5. Keyboard-triggered paste and modifier release

Reserve IDs `9811..9814` for the new hotkeys. Modifier mask:

`MOD_CONTROL | MOD_SHIFT | MOD_NOREPEAT = 0x0002 | 0x0004 | 0x4000 = 0x4006`.

The existing `Shift+F1..F4` registrations remain unchanged.

A `WM_HOTKEY` arrives while Ctrl/Shift/Fn can still be physically held. Injecting Ctrl+V immediately is unsafe because existing keyboard state can alter the synthetic input. Therefore:

1. capture the current external foreground HWND when the new hotkey arrives;
2. create a pending paste request;
3. use a small WinForms timer (for example 15–25 ms) to wait until Ctrl, Shift and that F-key are released, using `GetAsyncKeyState`;
4. abort after a bounded timeout (suggested 1.5 s);
5. before paste, verify the target is still the foreground window. If the user changed apps while releasing the keys, cancel instead of forcing focus back unexpectedly.

Mouse-triggered requests can use the same state machine, but with the explicit refocus step described above.

## 6. Injection method decision after research

### Primary recommendation: temporary Clipboard + SendInput Ctrl+V

Use paste semantics, not character-by-character typing, for the first implementation.

Reason:

- the owner's primary scenario is a browser/chat composer;
- multiline text must be inserted as pasted text without synthesizing Enter, because Enter may submit a chat message;
- clipboard paste naturally preserves multiline semantics expected by browser editors;
- direct `KEYEVENTF_UNICODE` generates Unicode keyboard/character input, but Windows documentation does not guarantee that arbitrary browser editors will treat multiline control characters like a paste operation.

Use `SendInput`, not `SendKeys`, for the synthetic Ctrl+V so the code can check how many input events Windows accepted.

### Do not make direct Unicode injection the automatic fallback in v1

It remains useful as an experimental spike, especially for simple single-line text, but mixing two insertion semantics would make failures harder to reason about. If clipboard injection fails, report the failure.

## 7. Clipboard transaction

The paste must avoid gratuitously destroying the user's clipboard.

Recommended transaction:

1. On the existing STA UI thread, call `Clipboard.GetDataObject()` and retain the previous IDataObject if available.
2. Create a DataObject containing:
   - UnicodeText = snippet content;
   - a private marker format/value unique to this paste request.
3. Use `Clipboard.SetDataObject(..., copy: true, retryTimes, retryDelay)`; use retries because the Clipboard can be temporarily busy.
4. Record `GetClipboardSequenceNumber()` immediately after writing the snippet.
5. Issue synthetic Ctrl+V.
6. Wait before restoration. **Do not restore synchronously**: real applications may consume clipboard data after the keyboard event has been queued. Start conservatively around 250 ms and tune from the owner test matrix.
7. Restore the previous IDataObject only if:
   - the clipboard sequence number is still the one recorded after WinSidebar wrote the snippet; and
   - the private marker is still present.
8. If the user or another application changed the clipboard during the delay, skip restoration. Never overwrite newer clipboard content.
9. If restoration fails, leave the current clipboard intact and surface a nonfatal localized warning.

A development spike must verify restoration with at least: plain text, file-drop list and image clipboard content. Clipboard preservation is best-effort across arbitrary custom/delayed formats; the application must not claim perfect preservation until tested.

## 8. SendInput constraints

Generate four input events for Ctrl+V:

- Ctrl down;
- V down;
- V up;
- Ctrl up.

Validate `SendInput`'s return count. A standard-integrity WinSidebar cannot inject into a higher-integrity/elevated target because of UIPI. This is a supported limitation: show an actionable message such as “The target may be running as administrator or blocking simulated input.” Do not request elevation for WinSidebar merely to work around UIPI.

## 9. Pending paste state machine

Do not implement paste as a blocking sleep inside a Button Click or WndProc.

Suggested states:

- `WaitingForModifierRelease`
- `RestoringTargetFocus` (mouse path)
- `PreparingClipboard`
- `SendingPaste`
- `WaitingToRestoreClipboard`
- `Completed / Failed / Cancelled`

One WinForms timer can advance the current request. Permit one active paste request at a time; a second request while one is active should be ignored or replace only before clipboard mutation. Queueing is unnecessary for four manual shortcuts.

This keeps the UI responsive and gives deterministic cleanup.

## 10. Persistence and Restore Defaults

Editor save flow:

1. copy the in-memory four-entry array;
2. replace only the edited slot;
3. validate all four;
4. atomically write `snippets.json`;
5. only after success replace the in-memory entry and refresh the row.

This mirrors the safe shortcut-edit pattern.

**Product decision still required before integration:** whether global Restore Defaults clears snippet contents. Engineering recommendation: yes, because snippets become app configuration, but the confirmation text must explicitly state that saved snippet text will be cleared, and the rollback snapshot must include `snippets.json`. The corrupt/previous file should remain recoverable through the atomic backup.

## 11. Localization additions

Expected new key families (names illustrative):

- `snippets.title`
- `snippets.default_name`
- `snippets.paste_hint`
- `snippets.configure_hint`
- `snippets.editor_title`
- `snippets.name`
- `snippets.content`
- `snippets.empty`
- `snippets.save_failed`
- `snippets.load_failed`
- `snippets.target_unavailable`
- `snippets.focus_failed`
- `snippets.paste_failed`
- `snippets.clipboard_busy`
- `snippets.clipboard_restore_failed`
- storage validation messages.

Add all keys to pt-BR/en-US catalog and exact parity in es-ES, ru-RU, zh-CN. Snippet content and user-renamed names are user data and are never translated.

## 12. Automated/local test split

### Pure smoke tests
Add a new `SnippetStoreSmoke` executable test that runs without touching the real profile:

- exactly four defaults;
- round-trip Unicode/multiline;
- names/content boundary values;
- rejects wrong version, duplicate/missing IDs, oversized file, oversized fields;
- atomic replacement and backup behavior where feasible;
- serialization preserves whitespace exactly.

Keep the existing localization smoke and extend it to new strings.

### Windows owner-run integration tests
A fresh locally published preview must cover:

- Chrome/Edge ChatGPT-style textarea/contenteditable;
- Notepad;
- multiline text;
- accents, Cyrillic, Simplified Chinese, emoji;
- all four Ctrl+Shift+F1..F4;
- row click after typing in external app;
- clipboard initially containing text, file(s), image;
- user changes clipboard during restore delay;
- target closes between request and paste;
- elevated target (expected controlled failure);
- hold Ctrl/Shift longer than usual;
- hotkey conflict;
- all existing right-click/global menus and Shift+F1..F4 regression checks.

## 13. Implementation order

1. Store/model + tests.
2. Editor + localized row UI only; no injection yet.
3. Global hotkey registrations and target-capture state, still no clipboard mutation.
4. Clipboard/SendInput injector behind one common `PasteSnippet(slot, source)` path.
5. Owner preview: hotkey path first.
6. Owner preview: mouse row path/focus restoration.
7. Clipboard restoration stress tests.
8. Integrate Restore Defaults after explicit product decision.
9. Only then update public docs/release planning.

## 14. Explicit non-goals

- No arbitrary script execution.
- No shell/PowerShell/JavaScript mode.
- No administrator requirement.
- No background keyboard hook.
- No per-application macros in the first version.
- No cloud sync.
- No GitHub Actions or FILEBRIDGE.
- No modification of the frozen checkpoint, `main`, or v1.0.0.
