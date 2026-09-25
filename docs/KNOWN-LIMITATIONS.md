# Known limitations — WinSidebar 2.0

These are current product boundaries, not a list of promised future changes.

## Platform

WinSidebar 2.0 targets Windows 10/11 x64. Other operating systems and CPU architectures are not part of the release contract.

## Unsigned executable

The current executable is not digitally signed. Windows SmartScreen or organizational security policy can display a warning.

## Global hotkeys can conflict

Windows can refuse a global hotkey if another application or system feature already owns the same combination. WinSidebar reports unavailable hotkeys instead of silently pretending they were registered.

AltGr+Y is implemented through the Win32 Ctrl+Alt+Y modifier combination.

## Elevated applications can reject paste input

Windows integrity boundaries can block simulated input from a normal WinSidebar process into an application running with higher privileges. Text injection therefore cannot be guaranteed for elevated targets.

## Clipboard restoration is best effort

Snippet paste temporarily uses the Windows clipboard. WinSidebar attempts to restore the previous clipboard content only when it still owns the clipboard generation it created. Preparation, input or restoration can fail and are represented as explicit failure states in `src/TextInjection.cs`.

## Window discovery is not exact Alt+Tab

The sidebar uses Windows window metadata and heuristics. Applications can expose unusual top-level windows, so the visible list can differ from the operating system's Alt+Tab list.

## Temporary rename is temporary

A renamed live window receives a WinSidebar alias tied to that window/process lifetime. It is not a persistent rename of the external application.

## No automatic startup or installer

WinSidebar does not install itself, register automatic startup or create system shortcuts. Users choose where to keep the portable executable.

## Cross-version preference migration

Formal migration testing across historical profile versions is outside the WinSidebar 2.0 validation scope. Preserve profile files before manual recovery or experimentation.
