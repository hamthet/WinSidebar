# Product-design review — WinSidebar

**Review date:** 2026-09-22. **Method:** source, README, tutorial, package workflow and prior CI result review; **no fresh interactive Windows user test, accessibility audit, sustained reliability test or installation study was performed**. This is a scoped design assessment, not a certification. Use `VERIFICATION.md` to turn findings into evidence.

## Product contract and intended user journey

The proposed finished product is a small, portable Windows 10/11 x64 utility for people who frequently switch between windows: start without an installer or separate .NET download; discover the collapsible sidebar; find an open window grouped by monitor; switch via mouse or Shift+F1…F4; set four folder/site shortcuts; optionally pick a browser; select one of five application languages; move/resize sidebar; retain choices across launches; quit/uninstall without hidden persistence. Windows 98-inspired visuals are deliberate. One executable plus localized plain-text instructions are acceptable; automatic updates, an installer and a cloud account are not inherent requirements. The current v1.0.0 shipped interface is Portuguese, **not** a complete five-language product.

## Summary of product status

- **Core utility appears implemented in source**: window traversal/activation, monitor grouping, four editable shortcuts, browser selection, tray interaction, global hotkeys and portable build workflow. Presence in code is not proof of usability or bug-free behavior.
- **Internationalization is not implemented**: app strings are hard-coded Portuguese, no language selector or saved locale; current ZIP includes Portuguese `LEIA-ME.txt`. English marketing does not close this gap.
- **A completed translation project alone cannot establish a finished product**: the release claim must match the *actual* window inclusion policy, a person must reliably find and use essential controls, settings must survive failures, interface layout/keyboard navigation must remain usable across target languages and DPI, and the released artifact must be verifiably the tested one.
- **No claim that the source is unsafe or broken** follows from absence of tests. Record unverified scenarios as `OPEN` / `NOT RUN`; mark a confirmed defect only after reproduction.

## Release blockers (P0: require demonstrable pass or an explicit, accurate scope correction)

| ID | Risk/user consequence | Release condition | Current evidence |
| --- | --- | --- | --- |
| UX-01 | User chooses English but sees Portuguese app text and/or Portuguese instructions | All application-owned strings, settings/dialog paths and shipped quick-start content available in each locale; first run, switching, restart pass | Incomplete; direct source + ZIP workflow |
| UX-02 | Promise 'find open windows' conflicts with intentionally excluded Calculator/Settings windows | Decide inclusion rule through user scenarios; either include commonly expected windows safely **or** state limited scope in interface/README, with locale-independent implementation | Open: title/process filters in `Snapshot()` |
| UX-03 | Existing users lose preference/shortcut names during locale migration | Existing `settings.ini` and `shortcuts.xml` migrate non-destructively; no blind translation of custom names; backup and failure cases tested | Not tested; source shows persisted names and no locale setting |
| UX-04 | A language makes essential controls unreadable or unreachable | For all five locales verify UI at 100/150/200% scale, default/compact width, Windows 10/11, keyboard and assisted access; no blocking clip or missing glyph | Not tested; fixed pixel sizes in editor/sidebar |
| UX-05 | Sidebar becomes unusable after display/hotkey/lifecycle changes | One/two monitor flows, unplug/replug, changed primary/work area, minimize/restore, Shift hotkey conflict and mouse fallback verified | Not tested; fallback and refresh paths exist |
| UX-06 | Save/read failure hides data loss or strands user | Test permission denied, corrupt config, backup recovery, bad icon/path/browser; preserve original and give actionable feedback, no hidden overwrite | Not tested; some settings exceptions are swallowed; shortcut errors surface warning |
| UX-07 | User cannot trust that downloaded product matches documentation | New release and exact artifact built after source change, version tagged, ZIP content per locale, checksum and MIT/license + unsigned warning verified; no claim old v1.0.0 changed | Old release remains Portuguese; current build tests only single EXE/old ZIP structure |
| UX-08 | Core interaction breaks during new localization infrastructure | Regression matrix of window list, four shortcuts, browser choice, icon creation, settings, tray, confirmation, hotkeys and single instance is PASS | Existing build success alone insufficient |

## Minimum additional product decisions (P1: decide explicitly, implement if required for the promised workflow)

1. **Discoverability / first run:** four icon-only shortcuts and the gear/configure mode are not self-explanatory at a glance. Test whether a first-time user can switch a window and configure a shortcut without reading source code. A concise localized Help/About item or visible hint is a low-impact option; don't redesign merely for fashion.
2. **Scope of display support:** code groups `Screen.AllScreens` but the targeting menu selects primary or the first nonprimary screen. Decide whether 3+ monitors are supported fully or explicitly limit positioning to primary/secondary. Ensure actual marketing does not promise arbitrary monitor selection.
3. **Keyboard/accessibility:** ensure language selection, context menus, editor fields and activation work without mouse; tooltips and accessible names change with locale; inspect screen reader announcements and contrast. No Windows 98 visual redesign is required if interaction remains accessible.
4. **Portable lifecycle:** a first-run guide, close-to-tray semantics (if any), exit and uninstall behavior must be clear. Distinguish 'delete EXE' from optional deletion of saved preferences; define manual update/rollback and whether two builds may conflict on global hotkeys.
5. **Security and trust:** public release remains unsigned. Keep direct official download and checksum instructions, do not tell people to bypass SmartScreen or corporate policy; signing is optional unless the release channel/audience requires it. Verify privacy wording against actual source/package.
6. **Feedback route:** GitHub issues already appear in README, but capture a reproducible issue template containing version, OS build, locale, monitor/DPI setup and sanitized reproduction steps; optional for a tiny personal utility unless support load warrants it.

## Enhancements (P2; not completion prerequisites)

Automatic updater, installer, onboarding wizard, themes beyond the intentional retro style, drag-and-drop shortcuts, cloud sync, usage analytics, commercial support or more operating systems are separate product proposals. Do not require them merely to award a 'finished' label for the defined portable tool. Their absence should be accurately disclosed where relevant.

## Practical acceptance study

Recruit at least a few people unfamiliar with the code, ideally using different target languages and a Windows 10/11 machine. Give only the public download and ask them to (a) launch, (b) locate/activate a window, (c) configure a folder and a URL shortcut, (d) choose a language, (e) restart, (f) quit/uninstall. Record task completion, friction/error points, unintended data changes, observed labels and environment. Do not state a success rate without running the study. User-study findings should convert to P0/P1 issues based on blocked core tasks, not aesthetic preference alone.

## Decision rule

**Today:** cannot claim the multilingual product is complete. **After the planned five-language implementation:** it is *eligible* to be called a completed **scoped portable utility** only if every P0 gate passes on the actual release artifact and essential P1 scope decisions are resolved. Product completeness is relative to the truthful promise above, not to an unlimited wishlist. Until there is actual runtime/manual evidence, status remains `NOT VERIFIED`, even if source and CI compilation look plausible.
