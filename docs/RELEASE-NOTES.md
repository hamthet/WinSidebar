# WinSidebar 2.0.0 release notes

[Português](i18n/RELEASE-NOTES.pt-BR.md) · [Español](i18n/RELEASE-NOTES.es-ES.md) · [Русский](i18n/RELEASE-NOTES.ru-RU.md) · [简体中文](i18n/RELEASE-NOTES.zh-CN.md)

Version 2.0.0 is a major product revision.

## Highlights

- Complete five-language runtime: English, Brazilian Portuguese, Spanish, Russian and Simplified Chinese.
- English is the default for new installations; language choice persists.
- Self-contained .NET 8 Windows x64 single executable.
- 4–12 configurable folder/website shortcuts with independent add/remove/restore controls.
- 4–8 reusable literal-text snippets with independent add/remove/restore controls.
- Per-snippet hotkeys: None or Shift+F1 through Shift+F12; snippets 1–4 default to Shift+F1–F4.
- F1–F4 open shortcuts 1–4.
- AltGr+Y toggles the sidebar.
- Right-click shortcut editing.
- Window context commands for temporary rename/reset and persistent application-ignore rules.
- Persisted width/height cycles and side selection.
- Five-language public README, tutorial, quick-start text, publishing guidance and concept artwork.

## Compatibility notes

The former Shift+F window-navigation contract is removed. Existing legacy profiles without a saved language may retain Portuguese until the user selects another language. Cross-version preference migration is outside the 2.0.0 validation scope.

## Security and distribution

The executable is portable and unsigned. Windows SmartScreen or organizational policy may display a warning. WinSidebar does not enable automatic startup, change the default browser or send telemetry.

Text snippets use the Windows clipboard temporarily for paste injection and attempt to restore previous clipboard content afterward.