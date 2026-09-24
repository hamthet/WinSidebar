# WinSidebar 2.0.0 — publishing notes

[README](../README.md) · [Português](i18n/OUTREACH.pt-BR.md) · [Español](i18n/OUTREACH.es-ES.md) · [Русский](i18n/OUTREACH.ru-RU.md) · [简体中文](i18n/OUTREACH.zh-CN.md)

## Suggested short announcement

WinSidebar 2.0.0 is a portable Windows 10/11 x64 sidebar for switching among open windows, launching up to 12 shortcuts and pasting up to 8 reusable text snippets. It is self-contained in one executable, supports English, Brazilian Portuguese, Spanish, Russian and Simplified Chinese, and defaults to English on a new installation.

Key controls: AltGr+Y toggles the sidebar, F1–F4 open shortcuts 1–4, and Shift+F1–F4 are the default snippet hotkeys.

## Visual assets

Use assets/linkedin-illustration.svg for social publishing and assets/hero-illustration.svg for repository/web presentation.

Both are concept illustrations, not application screenshots. Preserve that disclosure when exporting, cropping or reposting them. Do not describe the artwork as an exact representation of the current controls.

Localized SVG artwork is under assets/i18n/{locale}/.

## Claims to keep accurate

- Portable and self-contained does not mean digitally signed.
- The .NET 8 runtime is bundled; end users do not download .NET separately.
- Five runtime languages are supported; English is the default for a new profile.
- Text snippets store and paste literal text; they do not execute scripts.
- Window discovery is heuristic and should not be described as exact Alt+Tab parity.
- WinSidebar does not send telemetry, change the default browser or enable automatic startup.

Official source: https://github.com/hamthet/WinSidebar