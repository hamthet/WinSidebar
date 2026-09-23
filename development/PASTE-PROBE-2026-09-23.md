# Text-snippet paste probe — 2026-09-23

**Status:** IMPLEMENTED ON FEATURE BRANCH / WINDOWS EXECUTION PENDING OWNER.  
**Feature branch:** `feature/text-snippets`.  
**Frozen checkpoint remains untouched:** `checkpoint/functional-menu-approved-20260923` = `6af6343e8d5e6e0dc8cbd19f3d40e378cb583194`.

## Purpose

Before building the final four-row snippet UI, verify the high-risk cross-application insertion path on the owner's actual Windows desktop.

The current spike adds:

- production-shaped `src/TextInjection.cs` component;
- temporary global hotkey `Ctrl+Shift+F1`;
- a hardcoded five-line literal test payload containing Portuguese, Cyrillic, Simplified Chinese and emoji;
- modifier-release wait before synthetic input;
- foreground-target validation;
- temporary Clipboard payload plus Win32 `SendInput` Ctrl+V;
- clipboard sequence/private-marker guard;
- delayed best-effort restoration of the previous Clipboard;
- local-only build/launch runner `development/run-text-snippet-paste-probe-20260923.ps1`.

This is a **diagnostic spike**, not the final snippet UX. No snippet store, editor, four visible rows, mouse target restoration, or final localization strings have been implemented yet.

## Safety properties being tested

- Literal text only; no shell/script execution.
- Existing `Shift+F1..F4` hotkeys remain registered independently.
- The spike waits until Ctrl/Shift/F1 are physically released before sending Ctrl+V.
- Paste aborts if the foreground target changes before injection.
- If another program/user changes the Clipboard after WinSidebar stages the diagnostic payload, WinSidebar does not overwrite the newer Clipboard during restoration.
- `SendInput` failure is surfaced; elevated/UIPI-blocked targets are an expected controlled failure.
- One paste request at a time.

## Required owner test

1. Exit the currently running WinSidebar from the tray.
2. Switch local clone to `feature/text-snippets`, pull, and run the local runner.
3. Put a sentinel in Clipboard: `Set-Clipboard -Value 'CLIPBOARD-ANTES'`.
4. Focus the ChatGPT/browser composer and press/release `Ctrl+Shift+F1`.
5. Expected inserted literal text:

   `WinSidebar paste probe`  
   `Português: ação e configuração`  
   `Русский: тест`  
   `简体中文：测试`  
   `Emoji: 🙂`

6. After ~350 ms, `Get-Clipboard` should still return `CLIPBOARD-ANTES`.
7. Repeat in Notepad.
8. Report separately:
   - whether all five lines appeared;
   - whether the hotkey itself leaked characters/actions;
   - whether Clipboard was restored;
   - whether any dialog/error appeared;
   - browser vs Notepad difference.

Do not test against an elevated/admin application yet; that is a later expected-failure scenario.

## Gate after owner result

If browser + Notepad + clipboard restoration pass, promote `TextInjectionProbe` into the reusable injector and proceed to `SnippetStore`, four-row UI/editor and mouse-click focus restoration. If the probe fails, fix injection semantics before adding the UI.

No GitHub Actions, FILEBRIDGE, merge, `main` change or release is authorized by this spike.
