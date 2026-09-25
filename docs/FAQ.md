# WinSidebar 2.0 FAQ

[Português](i18n/FAQ.pt-BR.md) · [Español](i18n/FAQ.es-ES.md) · [Русский](i18n/FAQ.ru-RU.md) · [简体中文](i18n/FAQ.zh-CN.md)

## Do I need to install WinSidebar?

No. Download the ZIP, extract it, and double-click WinSidebar.exe.

## Do I need to install .NET?

No. The release is self-contained and carries the required .NET 8 runtime inside WinSidebar.exe.

## Windows warned me about the file. Is that expected?

The current executable is unsigned, so Windows SmartScreen or company security policy can show a warning. Follow the security rules of the computer you are using.

## Where should I put WinSidebar.exe?

Anywhere you control. Extract the ZIP first; do not run WinSidebar from inside the ZIP.

## How do I open or hide the sidebar?

Press AltGr+Y or use the narrow tab at the screen edge.

## How do I change a shortcut?

Right-click the shortcut to edit it. Left-click opens it.

## What are Scripts?

They are reusable text snippets, not executable code. Click one to paste its saved text into the external application you were using.

## Where are my settings saved?

Under %LOCALAPPDATA%\WinSidebar. The executable and your saved profile are separate.

## If I delete WinSidebar.exe, do I lose my shortcuts?

No. Deleting only the executable does not delete the saved profile.

## How do I completely remove everything?

Exit WinSidebar, delete WinSidebar.exe, then delete %LOCALAPPDATA%\WinSidebar only if you also want to erase saved shortcuts, snippets, ignored apps, icons and preferences.

## Why did paste fail in an administrator app?

Windows can block a normal application from simulating input into a higher-privilege process.

## Does WinSidebar send telemetry or start automatically?

No. Version 2.0 does neither.