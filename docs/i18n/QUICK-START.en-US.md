# WinSidebar — English quick-start guide

**Important:** The existing [WinSidebar v1.0.0 release](https://github.com/hamthet/WinSidebar/releases/tag/v1.0.0) has a Portuguese-language interface and a Portuguese `LEIA-ME.txt` inside its ZIP. This English guide explains how to use that existing release; it does **not** mean the released executable has been translated. See the [English project README](../../README.md) for current project status.

## Download and start

1. Download `WinSidebar-v1.0.0-win-x64.zip` from the official release's **Assets** section.
2. Extract the ZIP to a folder you control.
3. Double-click `WinSidebar.exe`. No installer or separate .NET download is required. Windows 10 or 11 (64-bit) is required.

The executable is **not digitally signed**. Windows Defender SmartScreen or your organization's security policy may warn or block execution. Only use the official download, check `SHA256SUMS.txt` if desired, and respect your IT policies. A SHA-256 checksum does not replace a digital signature.

## Use the sidebar

- Click the narrow tab at the screen edge to expand or collapse the sidebar. `Shift+F1` does the same.
- `Shift+F2` and `Shift+F3` move between windows; `Shift+F4` activates the selected window. Double-clicking a window also activates it.
- Use the header buttons to move the sidebar to the opposite edge, reduce or increase its width, or quit using the red **X** (confirmation required).
- Open the sidebar/tab context menu to choose the primary or secondary display when one is available.

## Configure your four shortcuts

The section labeled **PASTAS / WEB** means *Folders / Web*. Click the gear icon to enter configuration mode. Click one of the four shortcut icons to edit its display name, folder or URL, icon, and browser. Click **Salvar** (*Save*) to store the change, then click the gear again to exit configuration mode. **Cancelar** means *Cancel*. The default shortcuts are **Pasta local** (*Documents*), **Downloads**, **Acervo** (*unconfigured archive*), and **Site** (*Google*). The browser setting for website shortcuts does not change the Windows default browser.

## Privacy and removal

Settings and shortcut data are stored in `%LOCALAPPDATA%\WinSidebar`. The program does not install automatic startup or send telemetry. Close other sidebar editions first to avoid global-hotkey conflicts. To remove the application, quit and delete `WinSidebar.exe`. Optionally remove `%LOCALAPPDATA%\WinSidebar` to delete this edition's settings and custom shortcuts.

For further instructions, see the [English illustrated tutorial](TUTORIAL.en-US.md). To report a problem, [open an issue](https://github.com/hamthet/WinSidebar/issues/new) without exposing private window titles, paths, or URLs.