# WinSidebar project map

This is the technical entry point for maintainers, reviewers, automation and AI assistants. User-facing instructions start in README.md and START-HERE.txt.

## Product identity

- Public release line: 2.0
- Platform: Windows 10/11 x64
- Runtime: .NET 8, published self-contained as one WinSidebar.exe
- New-profile default language: English (en-US)
- Runtime languages: en-US, pt-BR, es-ES, ru-RU, zh-CN
- User profile root: %LOCALAPPDATA%\WinSidebar
- Installer: none; the application is portable
- Telemetry: none

## Reading order for maintainers and AI

1. PROJECT.md — stable product map and invariants.
2. docs/ARCHITECTURE.md — component boundaries and runtime flow.
3. docs/DATA-FORMATS.md — persisted user data and compatibility rules.
4. docs/DEVELOPMENT.md — build and test commands.
5. docs/RELEASE.md — release procedure and gates.
6. tests/ — executable behavioral contracts.
7. src/ and i18n/ — implementation and runtime language resources.

Historical branches and old Pull Requests are development evidence, not the current product contract.

## Sources of truth

| Concern | Source of truth |
| --- | --- |
| Version and publish mode | WinSidebar.csproj |
| Runtime behavior | src/*.cs |
| Runtime translations | i18n/*.json |
| Localization behavior | src/Localization.cs + tests/LocalizationSmoke.cs |
| Shortcuts | src/ShortcutConfig.cs |
| Snippets | src/SnippetStore.cs |
| Window aliases / ignored apps | src/WindowManagement.cs |
| Text paste | src/TextInjection.cs |
| User instructions | README.md, START-HERE.txt, docs/TUTORIAL.md, docs/FAQ.md |
| Release checks | tools/verify-release.ps1 + docs/RELEASE.md; .github/workflows/build.yml delegates to the same script |

## Product invariants

- New profiles start in English; Windows display language does not silently select another language.
- Exactly five runtime languages are supported: English, Brazilian Portuguese, Spanish, Russian and Simplified Chinese.
- Legacy profiles without a saved language may retain Portuguese for compatibility.
- F1-F4 open shortcuts 1-4.
- AltGr+Y toggles the sidebar.
- Snippet hotkeys are None or Shift+F1 through Shift+F12; snippets 1-4 default to Shift+F1-F4.
- Shortcut count: 4 to 12, added/removed in rows of four.
- Snippet count: 4 to 8.
- Snippets are literal text, not executable scripts.
- Shortcut restore and snippet restore are independent.
- User-authored names, paths, text and external window titles are never translated.
- Shipping output is a self-contained Windows x64 single executable.
- No automatic startup, default-browser changes or telemetry.
- Cross-version preference migration is not part of the 2.0 validation contract.

## Repository layout

    .github/workflows/   Manual verification workflows
    assets/              English-default concept artwork
    assets/i18n/         Localized concept artwork
    docs/                User and maintainer documentation
    docs/i18n/           Localized user documentation
    i18n/                Runtime localization catalogs
    src/                 WinForms implementation
    tests/               Smoke tests
    WinSidebar.csproj    Product/build contract

## Change rule

Before changing behavior, identify the owning source file and the test that should prove the new contract. Do not infer current behavior from old branches when current source and tests are available.