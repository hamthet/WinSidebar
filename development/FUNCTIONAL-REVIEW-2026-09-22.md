# Functional review — 2026-09-22 (source audit and owner GUI failures)

## Evidence / scope

Reviewed `src/WinSidebar.cs`, `src/WindowManagement.cs`, accepted change request and verification matrix at Chinese feature commit `29f88af`. Owner approved the five-language preview, not all window-management behaviors. All code and documentation belong to `hamthet/WinSidebar`; local standalone clone `C:\git\WinSidebar`; no GitHub Actions or FILEBRIDGE for routine testing. `main` and official v1.0.0 remain untouched.

## Source and Windows findings

- **Window dialog placement:** topmost sidebar; Rename and Manage Ignored originally used `CenterParent`. The owner has since pushed a source patch (`f28e0d1`) to place them beside the bar. Live display/monitor retest remains pending.
- **Window menu: OBSERVED FAIL, BLOCKER.** Owner's first right-click produced an unhandled `ObjectDisposedException` on `ContextMenuStrip`; after deferring disposal, a screenshot clearly shows the global/sidebar menu (monitors, ignored-list manager, language, exit) on a selected window row, with no Rename/Ignore actions. The parent `content` owns that global menu, while `tree.NodeMouseClick` opens a separate popup and tree lacked its own native context-menu route. This is likely a `WM_CONTEXTMENU` routing conflict; see [`BUG-2026-09-22-CONTEXT-MENU.md`](BUG-2026-09-22-CONTEXT-MENU.md). A development-only, anchored fix script is staged on the functional branch but not yet run. No PASS may be recorded before owner confirmation.
- **Automatic settings persistence:** `SaveSettings()` writes `settings.ini` directly, silently catches `IOException` and `UnauthorizedAccessException` and updates UI before save. A denied/interrupted write may leave on-screen changes with older disk values without warning. Fix atomically with explicit error handling and rollback-aware UI in a separate change; no unhandled exception in an event handler.
- **Alt+Tab fidelity:** `Snapshot()` uses `EnumWindows` plus visibility/cloaking, tool/owned-window and title heuristics. Actual Alt+Tab parity is environment-dependent; empirically verify Calculator/Settings open/closed and no phantom entries before changing filtering.
- **Per-window aliases:** dictionary keyed by HWND and guarded by PID + process start. A recycled HWND in a surviving process could still reuse an alias. Test close/recreate/reuse; never claim the guard rules this out absolutely.

## Sequence

1. On functional branch, apply the staged dedicated TreeView context-routing fix once on a clean checkout; perform local five-language smoke and Windows publish; verify window menu versus global menu, repeated mouse/keyboard opening, no disposed exception, Rename/Ignore/undo and dialog placement. Keep PR draft until GUI PASS.
2. Independently harden settings persistence and failure handling, with tests for denial/interruption. Cross-version migration is OUT OF SCOPE.
3. Owner-run WM-01..WM-06 / CORE-02..CORE-06 on actual Windows, record expected/actual and limitations, avoid public uploads of personal titles, paths, screenshots or logs.
4. After acceptance, reconcile stacked PRs, internal records and public five-language docs; prepare a clean shipping tree excluding `development/**`. Never bulk-merge development plans into `main`.
