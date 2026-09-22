# BUG — right-click on window shows the wrong menu (2026-09-22)

## Actual owner observations

1. Initial functional preview: right-click on a window row caused unhandled `System.ObjectDisposedException`, object `System.Windows.Forms.ContextMenuStrip`, with `ToolStripDropDown.Show` / `Control.WmContextMenu` in the stack. Owner supplied screenshot and dump in the conversation; they are not republished in the public repo.
2. After the menu-lifetime mitigation, owner supplied a screenshot of a **selected window row** with the *global sidebar* menu visible (primary/secondary monitor, manage ignored applications, language, exit), **without** per-window Rename, Reset Name or Ignore Application. This is a separate, observed GUI failure. The previous assistant incorrectly told the owner the actions should already be available; the screenshot establishes otherwise.

## Source diagnosis and pending fix

`src/WinSidebar.cs` shows a window popup directly from `tree.NodeMouseClick`, but the tree itself does not own a `ContextMenuStrip`. Its parent `content` does own the global/tray menu. The native `WM_CONTEXTMENU` processing can therefore also show the inherited global menu, displacing the per-window popup. This diagnosis is based on source and screenshot; no instrumented WinForms trace is available. The earlier change deferred popup disposal in `WindowManagement.cs`, but did not address this routing conflict.

On `feature/functional-dialog-placement`, the development-only `development/fix-window-context-routing-20260922.ps1` stages an anchored patch to `src/WinSidebar.cs`: explicitly attach a dedicated, non-displayed router menu to the TreeView, cancel its native Opening, defer the window popup via UI `BeginInvoke` after the context message, and handle keyboard context activation similarly. Blank/root clicks should not open window actions. The script checks exact branch/blob/clean tree, runs five-language smoke and Windows publish using the owner's portable .NET SDK, optionally commits/pushes, and starts the new executable automatically only after the older instance exits. Source patch, build and GUI fix are **NOT YET VERIFIED**. Never rerun the prior lifetime/placement scripts.

## Retest and release gate

Exit current preview through tray; run the new script once on a clean functional branch; confirm a window item opens Rename / Reset Name / Ignore Application, while blank space and tray do not expose window actions. Repeat right-click and keyboard Shift+F10/Apps and test Rename, Ignore and undo; check no disposal exception, duplicate popup, or clipping on both screen edges. Preserve existing settings and other repositories. Record genuine Windows PASS/FAIL separately from a .NET compile. No GitHub Actions, FILEBRIDGE, merge, or official release. `main`/v1.0.0 unchanged.
