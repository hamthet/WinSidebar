# Runtime string coverage checklist — executable, not README

Mark a component PASS only after a Windows runtime check. No UI locale is PASS yet.

| Component | Application-owned surface | Evidence required |
| --- | --- | --- |
| Startup and singleton | duplicate instance, fatal startup, shortcut storage load warning | launch two instances and invalid shortcuts |
| Sidebar header and tab | tab tip, close/move/resize accessible names and tooltips | mouse, keyboard and accessibility inspection |
| Window list | monitor root labels, window count, error, no windows, hotkey conflict | empty/multimonitor/hotkey-collision |
| Window management | rename and reset name, ignored process confirmation/errors, manager, buttons, inaccessible identity notice | five open calculators and ignore/unignore scenario |
| Shortcuts | heading, configure/edit mode, four buttons, target errors, save and restore dialogs, settings status | valid/invalid paths, save failure, reset rollback |
| Shortcut editor | every field, type/icon choices, filters, browser options, validation | folder/site/custom icon and invalid input |
| Tray and context menus | monitor selection, manage ignored, exit and five-language selector | actual locale switch and restart |
| Localized default names | new defaults and retained user custom names | first run; rename; switch; restart |
| Package | embedded catalogs, single EXE, end-user instructions in five languages | exact ZIP manifest and build artifact hash |

Nonlocalized by design: names of external application windows, user-provided window aliases, custom shortcut names/paths/URLs, process exe identities, `folder`/`website` and icon serialization tokens, Windows-provided common dialog text. Stable-key catalog parity and placeholders must be verified automatically. The owner will perform five-language user-interface acceptance; developer owns automated and regression checks. Cross-version migration tests are out of scope per owner instruction.
