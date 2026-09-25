# Contributing to WinSidebar

WinSidebar favors small, reviewable changes that preserve the portable single-file product.

## Before changing code

Read PROJECT.md, the relevant docs under docs/, the owning source file and the applicable smoke test.

## Scope

Keep one coherent task per branch/PR. Do not bundle unrelated refactoring, dependency changes or style modernization into a bug fix or documentation task.

Do not publish personal machine paths, private screenshots, credentials or secrets.

## Tests

Run the applicable commands in docs/DEVELOPMENT.md. Changes to UI, hotkeys, focus, window management, clipboard/input or persistence require real Windows validation.

State what was actually tested. Source review is not functional approval.

## Localization

English is the default repository/product language. Runtime UI also supports Brazilian Portuguese, Spanish, Russian and Simplified Chinese. Application-owned UI changes require five-language parity. User-authored content is never translated.

## User data

Treat settings.ini, shortcuts.xml, snippets.json and ignored-apps.json as user-data contracts. Preserve originals during diagnosis and do not introduce migration silently.

## Public documentation

Write for non-technical users first: say what to download, tell them to extract the ZIP, keep WinSidebar.exe at the archive root, explain SmartScreen plainly, and explain uninstall/profile behavior without build jargon.