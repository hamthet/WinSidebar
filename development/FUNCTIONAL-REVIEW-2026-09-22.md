# Functional review — 2026-09-22 (source audit, not Windows acceptance)

## Evidence / scope

Reviewed `src/WinSidebar.cs`, `src/WindowManagement.cs`, the accepted change request and verification matrix at Chinese feature commit `29f88af`. The owner approved the five-language preview; that is **not** a PASS for window-management or failure-handling scenarios. No Windows application was run by this review and no GitHub Actions was requested. The standalone local checkout is `C:\git\WinSidebar`. Preserve `main` and official v1.0.0.

## Directly observable issues

- **Dialog placement — source defect:** `SidebarWindow` is a topmost edge bar. `EditShortcut` already positions its modal beside the bar and makes it topmost; `WindowManagement.Rename` and `ManageIgnored` still use `FormStartPosition.CenterParent`, with no corresponding placement/topmost adjustment. On some display sizes they may overlap the sidebar or appear beneath it. Correct these two forms independently, then manually retest on the actual Windows desktop (both edges, primary/secondary screens, compact width); source inspection alone is not a GUI PASS.
- **Automatic settings persistence — error-reporting defect:** `SaveSettings()` writes `settings.ini` directly and silently catches `IOException` and `UnauthorizedAccessException`; call sites update the UI before attempting the write. A denied or interrupted save could leave changed UI with older on-disk settings without notifying the owner, particularly now that the separate Save Preferences control has been removed. Do not claim durable auto-save on failed writes. Fix in a separate, atomic/rollback-aware change and verify failure paths; do not replace silent catches with an unhandled exception in a UI event.
- **Alt+Tab fidelity — still an empirical gate:** `Snapshot()` filters `EnumWindows` entries by visible/noncloaked, tool/owned-window flags and nonempty titles. This is a heuristic and not identical to Windows' shell Alt+Tab policy by construction. Verify actual Calculator/Settings visible/closed cases before changing the filter; never reintroduce unconditional process/title exclusions to hide phantom entries.
- **Per-window aliases:** the dictionary is keyed by HWND and guarded by PID and process start, so aliases are not intentionally process-wide. Whether a recycled HWND from the *same* surviving process can inherit an alias is not ruled out by that guard alone. Validate close/recreate cases before asserting full handle-reuse safety.

## Execution order (no Actions, no FILEBRIDGE)

1. Isolate the two dialog-placement changes on a branch after `feature/runtime-i18n-zh`; run `dotnet run --project tests/LocalizationSmoke.csproj -c Release` and `dotnet build WinSidebar.csproj -c Release` locally with the portable SDK. Retest the two modal forms manually; do not merge or release on source inspection alone.
2. Add a focused persistence/failure-handling change with tests for an unwritable settings directory and interruption/rollback. Preserve existing saved files and language selection; cross-version migration is OUT OF SCOPE.
3. Owner-run Windows functional scenarios WM-01..WM-06 and CORE-02..CORE-06 in `CHANGE-REQUEST-2026-09-22.md` / `VERIFICATION.md`. Report exact Windows build, display setup, expected/actual and PASS/FAIL; do not upload raw window titles, installed-app paths or screenshots containing personal data to the public repo.
4. Only after these gates, reconcile PR dependencies, internal records and public five-language docs; create a clean shipping branch excluding `development/**` and one-shot scripts. Existing v1.0.0 remains intact.
