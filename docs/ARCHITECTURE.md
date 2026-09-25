# Architecture

WinSidebar is a single-process Windows Forms application. There is no service, database, web backend or installer.

## Runtime flow

1. Resolve %LOCALAPPDATA%\WinSidebar.
2. Initialize localization from settings.ini; a new profile starts in English.
3. Show the first-run language chooser when appropriate.
4. Load shortcuts, snippets and ignored-application rules.
5. Enumerate eligible top-level windows and group them by monitor.
6. Register global hotkeys.
7. Handle window activation, shortcuts, snippets and context-menu commands.
8. Persist user settings under the profile root.

## Source map

- src/WinSidebar.cs — main form, layout, window refresh, language menu, global hotkeys and UI orchestration.
- src/Localization.cs — locale selection and string lookup.
- src/FirstRunLanguageDialog.cs — first-run five-language chooser.
- src/ShortcutConfig.cs — shortcut model, XML persistence, defaults, validation and editor.
- src/SnippetStore.cs — versioned JSON persistence for literal-text snippets.
- src/SnippetEditor.cs — snippet name/content/hotkey editor.
- src/TextInjection.cs — foreground tracking plus clipboard/input injection.
- src/WindowManagement.cs — temporary live-window aliases and persistent ignored-application rules.

## Runtime localization

- i18n/catalog.json contains en-US and pt-BR for every application-owned key.
- i18n/es-ES.json, i18n/ru-RU.json and i18n/zh-CN.json must have exact key parity.
- Missing keys and unsupported language codes fail closed instead of inventing fallback text.

## Tests

- tests/LocalizationSmoke.cs checks embedded catalogs, five-language parity, English default behavior and persistence.
- tests/SnippetStoreSmoke.cs checks defaults, Unicode round-trip, limits, backups, version handling and invalid data rejection.
- GUI/hotkey/focus/window behavior still requires real Windows testing.