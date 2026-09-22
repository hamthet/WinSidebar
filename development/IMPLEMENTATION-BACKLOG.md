# Ordered implementation backlog — CR-2026-09-22

This is a task breakdown, not an implementation report. Apply on development/feature branches only. The owner-approved behavior in `CHANGE-REQUEST-2026-09-22.md` is authoritative.

- [ ] WM-A: independently reproduce genuine versus phantom Calculator/Settings entries and record Windows version / Alt+Tab comparison. Remove hard-coded app exclusions only with an Alt+Tab-aligned visibility rule; avoid process-wide exclusions of UWP host processes. Add deterministic pure filter tests where possible.
- [ ] WM-B: attach a context menu to actual window nodes, enable keyboard equivalent, and add per-live-window aliases/undo without modifying external applications. Verify 5 same-process windows do not share aliases and HWND reuse cannot leak names.
- [ ] WM-C: persist bounded ignored-application identities, add right-click ignore and an always-reachable tray manager with per-rule restore. Handle app-identity failures and duplicated names conservatively. Test persistence and reversible behavior.
- [ ] SP-A: rename folder heading to localized `Atalhos`/`Shortcuts`/etc.; fit gear + Save + Restore in same heading across widths/DPI; provide accessible names/tooltips and real keyboard focus.
- [ ] SP-B: add explicit Save preferences with real verified persistence and accurate error reporting, preserving established auto-save; add confirmed Restore defaults with rollback/partial failure disclosure, while preserving selected language provisionally.
- [ ] I18N-A: implement centralized locale catalogs and include every new control, modal, error and accessibility label alongside existing strings, 5 catalogs and parity checks.
- [ ] TEST: owner UI acceptance in all five languages; CI build/catalog/tests and documented manual WM-01..06/SP-01..03/Alt+Tab tests; correct labels on actual release artifact.
- [ ] RELEASE: only after product verification, finish per-language README/tutorial/artwork/ZIP; keep process documents out of final `main` and the ZIP.

Mark tasks checked only with a linked code commit plus appropriate executed test evidence in journal. Existing preference reliability is OWNER-APPROVED; migration tests explicitly OUT OF SCOPE.