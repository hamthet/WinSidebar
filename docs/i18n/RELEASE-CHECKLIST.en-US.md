# English release acceptance checklist

An English README alone does not constitute an English release. Before publishing an English-language executable:

- [ ] All application controls, context menus, accessibility names, tooltips, confirmation dialogs, error messages, and default shortcut labels are in English.
- [ ] Existing settings (`settings.ini`, `shortcuts.xml`) continue to load without silently overwriting user-defined shortcut names or destinations.
- [ ] All end-user files inside the ZIP are in English, including `README.txt`; do not include a Portuguese-only `LEIA-ME.txt` as the sole user guide.
- [ ] The English README, quick-start guide, illustrated tutorial, promotional copy, and all embedded artwork text agree with the packaged application's language and features.
- [ ] The `win-x64` self-contained single-file build passes; the ZIP manifest and SHA-256 checksum are verified.
- [ ] Windows 10/11 GUI smoke tests confirm no clipped strings, especially in the shortcut editor and sidebar at minimum width.
- [ ] Do not relabel or replace the historical v1.0.0 assets: publish a distinct version/tag only after the checks pass.
- [ ] Only then remove the existing-release language warning from the English README.

This checklist is a release gate, not a statement that these items have passed.