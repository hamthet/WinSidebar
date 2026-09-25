# WinSidebar 2.0 tutorial

[README](../README.md) · [Português](i18n/TUTORIAL.pt-BR.md) · [Español](i18n/TUTORIAL.es-ES.md) · [Русский](i18n/TUTORIAL.ru-RU.md) · [简体中文](i18n/TUTORIAL.zh-CN.md)

## 1. Start the portable application

Extract the WinSidebar 2.0 ZIP and run WinSidebar.exe. No installer or separate .NET runtime is required.

On a brand-new profile, English is preselected. The first-run chooser also offers Português (Brasil), Español, Русский and 简体中文. Your choice is stored in %LOCALAPPDATA%\WinSidebar.

If Windows displays a SmartScreen warning, remember that the current executable is unsigned. Follow your organization’s security policy.

## 2. Open and close the sidebar

Use either method:

- click the narrow tab attached to the screen edge;
- press AltGr+Y.

AltGr+Y is a toggle: collapsed becomes open, open becomes collapsed.

The header controls let you move the sidebar to the opposite side and cycle through available widths and heights. These settings persist.

## 3. Switch windows

Open the sidebar and click an eligible window in the list. WinSidebar groups listed windows by monitor.

Right-click a listed window for:

- Rename — gives the current live window a temporary WinSidebar label;
- Reset name — removes that temporary label;
- Ignore this application — persistently hides windows belonging to that application.

Use the general context menu to manage ignored applications and show them again.

Window discovery uses Windows metadata and heuristics. A program may expose unusual top-level windows that do not exactly match the system Alt+Tab list.

## 4. Configure shortcuts

The Shortcuts section starts with four entries.

- Left-click: open the configured folder or website.
- Right-click: edit the shortcut.
- +: add one row of four shortcuts.
- −: remove the last added row, never below four.
- Restore: restore only the shortcut section to defaults after confirmation.

The product limit is 12 shortcuts.

The editor can configure name, folder/website target, icon and browser behavior. Custom icons are copied into the WinSidebar profile.

Global F1–F4 open shortcuts 1–4. Additional shortcuts are mouse-driven.

## 5. Configure text snippets

The Scripts section contains literal text snippets. They are not executable scripts.

- Click the snippet row: paste its saved text into the most recently active eligible external application.
- Gear: edit name, content and hotkey.
- +: add one snippet.
- −: remove the last added snippet, never below four.
- Restore: restore only the snippets section after confirmation.

The product limit is 8 snippets.

Snippets 1–4 default to Shift+F1–F4. Each snippet can instead use None or Shift+F1 through Shift+F12. Duplicate snippet hotkeys are rejected.

WinSidebar temporarily uses the Windows clipboard for paste injection, then attempts to restore the previous clipboard content. Input can fail when the target application runs at a higher integrity level, such as an elevated administrator process.

## 6. Change language

Open the general context menu and choose Language. The five supported runtime languages are:

- English
- Português (Brasil)
- Español
- Русский
- 简体中文

The selection takes effect in the application and is stored in settings.ini.

New installations default to English regardless of the Windows display language. Older profiles created before the language setting existed may initially remain in Portuguese.

## 7. Profile files

WinSidebar keeps user state under:

    %LOCALAPPDATA%\WinSidebar

Depending on the features you use, the folder can contain:

- settings.ini — language, position and layout preferences;
- shortcuts.xml — shortcut definitions;
- snippets.json — text snippet definitions and hotkeys;
- ignored-apps.json — persistent ignored-application rules;
- icons\ — copied custom shortcut icons;
- .bak files created by atomic replacement paths.

User data such as shortcut names, paths, snippet contents and external window titles is never translated.

## 8. Restore and recovery

Shortcut Restore and Script Restore are intentionally independent. Restoring one section does not reset the other section.

If a profile file becomes unreadable, preserve the original file before manually deleting or replacing it. A malformed file can contain useful recovery evidence.

Cross-version preference migration is not part of the 2.0 validation contract.

## 9. Uninstall

Exit WinSidebar, then delete WinSidebar.exe and any extracted release documentation.

The profile under %LOCALAPPDATA%\WinSidebar is independent. Delete it only if you also want to erase saved shortcuts, snippets, ignored applications, icons and preferences.

## 10. Keyboard reference

| Command | Action |
| --- | --- |
| AltGr+Y | Toggle sidebar |
| F1–F4 | Open shortcuts 1–4 |
| Shift+F1–F4 | Default snippet hotkeys 1–4 |
| Shift+F1–F12 | Available configurable snippet hotkeys |

The old Shift+F window-list navigation is intentionally removed in 2.0.