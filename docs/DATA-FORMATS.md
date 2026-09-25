# Persisted data formats

All user state is rooted at %LOCALAPPDATA%\WinSidebar. Do not put profile files in the release ZIP and do not delete them casually during testing.

## settings.ini

Line-oriented key=value data. Current keys include width, height, left, secondary, browser, browserSystem and language.
New profiles default to en-US. Legacy profiles without language= may retain Portuguese for compatibility.
Strict-save paths use atomic replacement and may create settings.ini.bak.

## shortcuts.xml

Owned by ShortcutStore in src/ShortcutConfig.cs.
- Minimum 4, maximum 12 entries.
- Added/removed in rows of four.
- XML version 1 is the four-entry form.
- XML version 2 supports expanded counts.
- Unknown versions and invalid counts are rejected.
- Custom icon copies live under %LOCALAPPDATA%\WinSidebar\icons.

## snippets.json

Owned by SnippetStore in src/SnippetStore.cs.
- Version 1 is the four-slot form and remains readable.
- Version 2 supports expanded slot counts.
- The writer uses version 1 for exactly four slots, otherwise version 2.
- Minimum 4, maximum 8 items.
- Name maximum 48 characters.
- Content maximum 65,536 characters.
- Hotkey is empty or Shift+F1..Shift+F12.
- Physical file maximum 2 MiB.
- Atomic replacement can create snippets.json.bak.
Snippet content is literal user text and is never localized or executed.

## ignored-apps.json

Owned by WindowManagement.
- JSON object mapping an application identity to a display label.
- Identity keys use path:... or name:... forms.
- Maximum file size 65,536 bytes.
- Maximum 128 rules.
- Identity key maximum 2,048 characters.
- Display value maximum 256 characters.
- Atomic replacement can create ignored-apps.json.bak.
If the store is detected as corrupt, preserve it before reset; do not silently overwrite evidence.

## Temporary window aliases

Window rename aliases are not a persistent file. They are tied to the live window/process lifetime.

## Recovery

If a profile file fails to load, preserve the original and any .bak before manual cleanup.