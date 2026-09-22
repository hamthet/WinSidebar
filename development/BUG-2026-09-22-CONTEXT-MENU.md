# BUG — window versus global context menus (2026-09-22)

## Owner-observed sequence

1. Initial right-click on a window row caused an unhandled `System.ObjectDisposedException` for `System.Windows.Forms.ContextMenuStrip`. Screenshot and dump remain in the conversation, not the public repository.
2. After deferring menu disposal, a selected window row instead showed the *global* sidebar menu, with no window-specific Rename / Reset Name / Ignore Application. The owner's screenshot confirmed the failure.
3. After the owner ran the dedicated TreeView context-routing patch, the **window-specific popup now works**, and Rename and Reset Name were both explicitly tested successfully. This is a limited GUI PASS only for those interactions; Ignore, undo, keyboard invocation, other layouts and the original crash's recurrence were not separately confirmed.
4. **New observed regression:** clicking outside a window item does not show the formerly available global sidebar menu. The shared TreeView router cancels the native context menu for *all* tree clicks, then immediately returns for blank areas or monitor headings. The global/tray menu is still attached to the parent content and tray, but the tree no longer inherits it. Thus the user cannot access its general commands from the list's empty space.

## Narrow patch staged; not yet validated

`feature/functional-dialog-placement` contains the development-only one-shot script [`development/fix-empty-space-context-menu-20260922.ps1`](https://github.com/hamthet/WinSidebar/blob/feature/functional-dialog-placement/development/fix-empty-space-context-menu-20260922.ps1). It changes only the blank/root branch of the TreeView's `Opening` handler: after canceling the native event, post `content.ContextMenuStrip.Show(tree, clicked)` to the UI message queue. The currently working window-item route is unchanged. The script guards the exact source blob/branch and clean checkout, runs local five-language smoke and a fresh WinForms self-contained publish with the portable SDK, optionally commits and pushes, then auto-opens the new preview if the old instance is closed. Its existence is NOT source-fix execution or a GUI PASS; do not rerun previous patch scripts.

## Required retest

- Right-click a real window row: window-specific Rename / Reset Name / Ignore Application; rename/reset remain correct.
- Right-click **blank list space** and a **monitor heading**: shared global menu (monitor controls, ignored-app management, language, exit); no window commands.
- Right-click other non-item sidebar surfaces and tray: shared global menu. Repeat clicks and test Shift+F10/Apps, Ignore and undo, without duplicates/exceptions.
- Record exact failures separately. Preserve all preferences and local changes; do not publish screenshots with personal window names. No GitHub Actions, FILEBRIDGE, merge or release. `main`/v1.0.0 remains unchanged.
