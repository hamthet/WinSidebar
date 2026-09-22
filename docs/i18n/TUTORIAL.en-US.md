# WinSidebar tutorial

**[English README](../../README.md)** · [Português (Brasil)](../TUTORIAL.md)

> **About the images:** The artwork on this page is an AI-assisted **concept illustration**, not a screenshot or a pixel-accurate representation of the actual interface. The Windows 98-inspired design is intentional. The available v1.0.0 application's interface is still in Portuguese.

![Concept artwork showing the sidebar beside a generic dual-monitor desktop](../../assets/i18n/en-US/hero-illustration.svg)

## 1. Download and run — no separate dependencies

1. Open the [official v1.0.0 release](https://github.com/hamthet/WinSidebar/releases/tag/v1.0.0).
2. Under **Assets**, download `WinSidebar-v1.0.0-win-x64.zip`; do not select the automatically generated *Source code* archives if you only want to run the program.
3. Extract the ZIP into a folder you control, such as Documents or Downloads. It contains only `WinSidebar.exe` and `LEIA-ME.txt` (the latter is in Portuguese).
4. Double-click `WinSidebar.exe`. No installer, PowerShell, Git, Visual Studio, or separate .NET installation is necessary.

WinSidebar is intended for **64-bit Windows 10 and Windows 11**. The executable is not digitally signed, so Windows or your organization's policies may warn about or prevent execution. Verify that your download comes from the official repository. On a managed computer, follow your IT policies; do not bypass security controls.

## 2. Expand or collapse the sidebar

Click the small **tab at the side of your monitor** to expand or collapse the window list. Open windows are grouped by monitor. Double-click a window in the list to activate it, or use the activation shortcut.

| Shortcut | Action |
| --- | --- |
| `Shift + F1` | Expand or collapse the sidebar |
| `Shift + F2` | Select the previous window |
| `Shift + F3` | Select the next window |
| `Shift + F4` | Activate the selected window |

If another application has already reserved a shortcut, use the mouse or close the other tool. Do not run multiple editions of the sidebar at the same time; they may compete for global keyboard shortcuts.

## 3. Adjust position and width

The **header** includes buttons to decrease or increase the width, move the sidebar to the opposite screen edge, and exit via the red **X**. To select the main or secondary monitor, right-click the sidebar/tab and choose **Usar monitor principal** (use primary monitor) or **Usar monitor secundário** (use secondary monitor). A secondary monitor is only available when Windows detects one.

## 4. Configure your four shortcuts

In the **PASTAS / WEB** (Folders / Web) area, click the **gear icon** to enter configuration mode. Clicking a shortcut icon in this mode opens its editor **instead of opening the folder or website**. Edit the name, choose a destination type, pick a folder or enter a URL, select the icon, and optionally choose which browser opens websites. Save and click the gear again to leave configuration mode.

Generic defaults in the current release are **Pasta local** (your Documents folder), **Downloads**, **Acervo** (no destination configured), and **Site** (Google). These are generic examples, not personal paths, usernames, or private websites. Choosing a browser here does not change your Windows default browser.

## 5. Where are preferences stored?

Settings are saved under `%LOCALAPPDATA%\WinSidebar`, separately from other editions' preferences. The executable does not configure automatic startup or replace another program.

To uninstall, exit with the X and delete `WinSidebar.exe`. To remove **this edition's** shortcuts and settings as well, optionally delete `%LOCALAPPDATA%\WinSidebar`. Do not delete folders belonging to other editions.

## 6. Troubleshooting and feedback

If something does not work, [open an issue](https://github.com/hamthet/WinSidebar/issues/new) with your Windows version, steps to reproduce, and the error message. Before attaching screenshots, conceal personal names, paths, window titles, URLs, and other sensitive information.

**Official download:** [WinSidebar 1.0.0](https://github.com/hamthet/WinSidebar/releases/tag/v1.0.0). **The v1.0.0 application itself remains in Portuguese.**
