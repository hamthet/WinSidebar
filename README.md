# WinSidebar

**English** · [Português (Brasil)](docs/i18n/README.pt-BR.md) · Español, Русский and 简体中文 (translations planned)

![Concept illustration of WinSidebar on a dual-monitor desktop](assets/i18n/en-US/hero-illustration.svg)

> **Concept artwork, created with AI assistance.** This is not a screenshot or an exact representation of the application controls. The **Windows 98-inspired appearance is intentional**.

WinSidebar is a collapsible Windows sidebar for **finding and switching between open windows**, grouped by monitor, with four customizable shortcuts to folders and websites. It is portable, needs no installer or separate .NET download, and is open source under the [MIT license](LICENSE).

**[Download the existing WinSidebar 1.0.0 for Windows 10/11 x64](https://github.com/hamthet/WinSidebar/releases/tag/v1.0.0)** · **[English quick-start guide](docs/i18n/QUICK-START.en-US.md)** · **[English illustrated tutorial](docs/i18n/TUTORIAL.en-US.md)**

> **Release language status:** The currently published **v1.0.0 executable and the `LEIA-ME.txt` packaged with it are in Portuguese**. The English guides linked here provide complete instructions for that release. An English application build is being prepared separately; do not treat v1.0.0 as an English release.

> **Unsigned application:** The released executable is **not digitally signed**. Microsoft Defender SmartScreen or organizational security policies may warn or block it. Download only from the [official release](https://github.com/hamthet/WinSidebar/releases/tag/v1.0.0), verify its `SHA256SUMS.txt` if desired, and follow your organization's policies. **A checksum does not replace a digital signature.**

## Getting started

1. Download **`WinSidebar-v1.0.0-win-x64.zip`** under the official release's *Assets* section.
2. Extract it to a folder you control and run **`WinSidebar.exe`**.
3. Follow the **[English quick-start guide](docs/i18n/QUICK-START.en-US.md)**. It translates the relevant labels of the Portuguese v1.0.0 interface and covers installation, configuration, and removal.

The current release archive contains only `WinSidebar.exe` and `LEIA-ME.txt`; no installer, PowerShell, Git, compiler, or separate .NET download is needed. The executable bundles .NET 8.

## Features

- **Switch windows:** the sidebar tab expands or collapses the panel; `Shift+F1` toggles it, `Shift+F2/F3` navigate windows, and `Shift+F4` activates the selected window.
- **Group by monitor:** see windows on the primary and, when present, secondary monitor.
- **Configure four shortcuts:** click the gear in **PASTAS / WEB** (Folders / Web) and choose a folder or URL, display name, icon, and browser. The current defaults are **Pasta local** (Documents), **Downloads**, **Acervo** (unconfigured), and **Site** (Google).
- **Adjust the panel:** use the header buttons to move it to the opposite edge, reduce or increase its width, and exit with the red X.

## Privacy and removal

Preferences are kept under `%LOCALAPPDATA%\WinSidebar`. WinSidebar does not replace other executables, change your default browser, enable automatic startup, or send telemetry. Close other sidebar editions before running this one to avoid global-hotkey conflicts.

To remove the program, quit using the X and delete `WinSidebar.exe`. Optionally delete `%LOCALAPPDATA%\WinSidebar` to remove this edition's preferences.

## Source, license and outreach

C# source: [`src/`](src/). Windows build: [GitHub Actions](.github/workflows/build.yml). The [MIT license](LICENSE) permits use, modification, and redistribution, including commercial use, provided copyright and license notices are retained. [Report issues](https://github.com/hamthet/WinSidebar/issues/new) without exposing personal information in screenshots or logs.

[English promotional illustration](assets/i18n/en-US/linkedin-illustration.svg) · [English publishing guidance](docs/i18n/OUTREACH.en-US.md). **Illustrations are not actual screenshots.**