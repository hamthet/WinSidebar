#!/usr/bin/env python3
"""One-shot Spanish UI integration; abort if English stage source has drifted."""
from pathlib import Path
import json

root = Path(__file__).resolve().parents[1]
source = root / 'src/WinSidebar.cs'
text = source.read_text(encoding='utf-8')
catalog = json.loads((root / 'i18n/catalog.json').read_text(encoding='utf-8'))
spanish = json.loads((root / 'i18n/es-ES.json').read_text(encoding='utf-8'))
if set(catalog) != set(spanish) or len(catalog) < 100:
    raise SystemExit('Spanish resource does not cover every existing UI key exactly.')
for key, value in spanish.items():
    if not isinstance(value, str) or not value.strip():
        raise SystemExit(f'Invalid Spanish translation: {key}')

def replace_once(old, new):
    global text
    count = text.count(old)
    if count != 1:
        raise SystemExit(f'Expected exactly one anchored source occurrence; found {count}: {old[:100]!r}')
    text = text.replace(old, new, 1)

replace_once('    private ToolStripMenuItem portugueseItem;\n',
             '    private ToolStripMenuItem portugueseItem;\n    private ToolStripMenuItem spanishItem;\n')
replace_once('        portugueseItem = new ToolStripMenuItem("Português (Brasil)");\n',
             '        portugueseItem = new ToolStripMenuItem("Português (Brasil)");\n        spanishItem = new ToolStripMenuItem("Español");\n')
replace_once('        portugueseItem.Click += delegate { ChangeLanguage("pt-BR"); };\n',
             '        portugueseItem.Click += delegate { ChangeLanguage("pt-BR"); };\n        spanishItem.Click += delegate { ChangeLanguage("es-ES"); };\n')
replace_once('        languageMenu.DropDownItems.Add(portugueseItem);\n',
             '        languageMenu.DropDownItems.Add(portugueseItem);\n        languageMenu.DropDownItems.Add(spanishItem);\n')
replace_once('        portugueseItem.Checked = Localization.Current == "pt-BR";\n',
             '        portugueseItem.Checked = Localization.Current == "pt-BR";\n        spanishItem.Checked = Localization.Current == "es-ES";\n')
source.write_text(text, encoding='utf-8')
print(f'PASS: Spanish menu integrated; {len(spanish)} Spanish resource keys present.')
