# Design decisions

This file records current WinSidebar 2.0 product decisions that are easy to mistake for implementation accidents. It is descriptive technical documentation, not repository governance.

## Portable, self-contained distribution

WinSidebar is distributed as a portable Windows x64 application. The end-user executable contains the required .NET 8 runtime. There is no installer and the product does not require users to install .NET separately.

Source contract: `WinSidebar.csproj`.

## English is the new-profile default

The supported runtime languages are English, Brazilian Portuguese, Spanish, Russian and Simplified Chinese. A brand-new profile starts in English regardless of the Windows display language. The explicit user choice is then persisted.

Legacy profiles created before language persistence can retain Portuguese when no `language=` value exists. This preserves existing user state rather than silently migrating it.

Sources: `src/Localization.cs`, `src/FirstRunLanguageDialog.cs`, `tests/LocalizationSmoke.cs`.

## User content is never translated

Application-owned labels are localized. User-authored shortcut names, paths, snippet text, external window titles and application identities remain literal.

This prevents localization from modifying personal data or changing the meaning of user content.

## Snippets are literal text, not executable scripts

The UI section is historically labeled Scripts in the product, but its stored content is literal text. WinSidebar stages that text on the Windows clipboard, pastes with simulated Ctrl+V and attempts to restore the previous clipboard state.

Do not reinterpret the feature as arbitrary code/script execution without a separate product decision and security review.

Sources: `src/SnippetStore.cs`, `src/SnippetEditor.cs`, `src/TextInjection.cs`.

## Keyboard contract

- AltGr+Y toggles the sidebar.
- F1-F4 open shortcuts 1-4.
- Snippet hotkeys are None or Shift+F1 through Shift+F12.
- Snippets 1-4 default to Shift+F1-F4.
- The older Shift+F window-list navigation is intentionally absent.

Source: `src/WinSidebar.cs`.

## Window list is heuristic

WinSidebar enumerates eligible top-level Windows windows using metadata and heuristics. It does not claim exact parity with the operating system's Alt+Tab implementation.

Temporary window aliases belong to the current live window/process lifetime. Ignored-application rules are persistent.

Sources: `src/WinSidebar.cs`, `src/WindowManagement.cs`.

## Persistence is separate from the executable

User state lives under `%LOCALAPPDATA%\WinSidebar`, not beside WinSidebar.exe. Deleting or replacing the executable does not automatically remove the profile.

Profile formats are validated and some stores use atomic replacement with `.bak` recovery files. Cross-version preference migration is outside the 2.0 validation contract.

See `docs/DATA-FORMATS.md`.

## Public version versus Windows file version

The public product/release name is **2.0** and the published tag is `v2.0`. Windows AssemblyVersion/FileVersion use the required four-field form `2.0.0.0`. That technical representation is not a second public version.

The `v2.0` tag is the immutable source baseline for the published 2.0 artifacts. Later commits on `main` do not retroactively redefine that release.

## Release verification

`tools/verify-release.ps1` is the canonical local build/test/package gate. The manual build workflow delegates to that same script so the repository does not maintain two independent release procedures.

A successful technical gate is evidence; it is not merge approval or release-publication approval.
