# WinSidebar

**English** · [Português (Brasil)](docs/i18n/README.pt-BR.md) · Español, Русский and 简体中文 (translations planned)

![Concept illustration of WinSidebar on a dual-monitor desktop](assets/i18n/en-US/hero-illustration.svg)

> **Concept artwork, created with AI assistance.** This is not a screenshot or an exact representation of the application controls. The **Windows 98-inspired appearance is intentional**, not an outdated UI in need of modernization.

WinSidebar is a collapsible Windows sidebar for **finding and switching between open windows**, grouped by monitor. It also provides four customizable shortcuts to folders and websites. It is portable, needs no installer or separate .NET download, and is open source under the [MIT license](LICENSE).

**[Download WinSidebar 1.0.0 for Windows 10/11 x64](https://github.com/hamthet/WinSidebar/releases/tag/v1.0.0)** · **[Read the step-by-step tutorial](docs/i18n/TUTORIAL.en-US.md)**

> **Language availability:** The repository's default documentation and concept artwork are in English. **The existing v1.0.0 executable still has a Portuguese interface.** An in-app language selector and translated executable have not been released. This documentation change does not update the published binary.

> **Unsigned application:** This version's executable is **not digitally signed**. Microsoft Defender SmartScreen may show an unrecognized-app warning, and security policies may block it. Download only from the [official release](https://github.com/hamthet/WinSidebar/releases/tag/v1.0.0). If you want to check the download's integrity, compare it with the release's `SHA256SUMS.txt`. Follow your computer's IT policies. **A checksum is not a substitute for a digital signature.**

## Get started in a minute

1. Open the release page and download **`WinSidebar-v1.0.0-win-x64.zip`** under *Assets*.
2. Extract the ZIP into a folder you control.
3. Run **`WinSidebar.exe`**. The included `LEIA-ME.txt` provides essential instructions in Portuguese.

The ZIP contains **only the executable and `LEIA-ME.txt`**. You do not need PowerShell, Git, a compiler, an installer, or a separate .NET download. The executable bundles the .NET 8 runtime.

## Features

- **Switch windows:** click the sidebar tab to expand or collapse it; `Shift+F1` toggles the sidebar, `Shift+F2/F3` navigate windows, and `Shift+F4` activates the selected window.
- **Group windows by monitor:** view windows on the primary monitor and, if available, a secondary display.
- **Customize four shortcuts:** in the **PASTAS / WEB** (Folders / Web) section, click the gear and then a shortcut to set its name, folder or URL, icon, and browser. The current executable's generic defaults are **Pasta local** (Documents folder), **Downloads**, **Acervo** (unconfigured), and **Site** (Google).
- **Adjust the sidebar:** use the header buttons to move it to the opposite edge, decrease or increase its width, and exit with the red X.

**[See the illustrated tutorial →](docs/i18n/TUTORIAL.en-US.md)**

## Privacy and existing installations

Preferences are stored under `%LOCALAPPDATA%\WinSidebar`, independently of other installations. WinSidebar does not replace earlier executables, change your default browser, enable automatic startup, or send telemetry. Close any other edition of the sidebar before launching this one to avoid conflicts between global keyboard shortcuts.

To uninstall, exit with the X and delete `WinSidebar.exe`. Optionally, delete `%LOCALAPPDATA%\WinSidebar` to remove **this edition's preferences**.

## Source, license, and outreach

The C# source is in [`src/`](src/); the Windows build is verified by the [build workflow](.github/workflows/build.yml). The [MIT license](LICENSE) permits use, modification, and redistribution, including commercial use, provided its copyright and license notices are retained. To report issues or suggest improvements, [open an issue](https://github.com/hamthet/WinSidebar/issues/new); remove personal information from any screenshots or logs.

An [English square promotional illustration](assets/i18n/en-US/linkedin-illustration.svg) and [English LinkedIn publishing guidance](docs/i18n/OUTREACH.en-US.md) are available. **Do not present either illustration as an actual application screenshot.**
