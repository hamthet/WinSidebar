# Runtime localization contract — 2026-09-24

**Producer:** `hamthet/WinSidebar`  
**Runtime branch under closure:** `feature/text-snippets`  
**Supported application languages:** English (`en-US`), Brazilian Portuguese (`pt-BR`), Spanish (`es-ES`), Russian (`ru-RU`), Simplified Chinese (`zh-CN`)  
**Product default:** **English (`en-US`)**

## Current runtime contract

1. A brand-new installation with no `%LOCALAPPDATA%\WinSidebar\settings.ini` starts with English selected.
2. The first-run language chooser lists, in this order:
   - English
   - Português (Brasil)
   - Español
   - Русский
   - 简体中文
3. English is preselected on first use. The Windows UI culture does not silently override the product default.
4. After the user chooses a language, the selected code is persisted as `language=<code>` in `settings.ini` and restored on later launches.
5. The general WinSidebar context menu allows changing among the same five languages at runtime.
6. Existing legacy profiles whose `settings.ini` predates the `language=` field retain Portuguese. This is a compatibility preservation rule, not the default for new installations.
7. User-authored data is never translated: shortcut names/targets, snippet names/content, external window titles, process identities and paths remain literal.

## Catalog structure

`i18n/catalog.json` is the base catalog and contains English and Brazilian Portuguese for every application-owned localization key.

Additional complete locale catalogs:

- `i18n/es-ES.json`
- `i18n/ru-RU.json`
- `i18n/zh-CN.json`

All are embedded in the single WinSidebar executable by `WinSidebar.csproj`. Missing resources, missing keys and unsupported language codes fail closed rather than silently falling back to an incomplete locale.

## Verification contract

`tests/LocalizationSmoke.cs` must verify:

- base English/Portuguese catalog completeness;
- exact key-count parity for Spanish, Russian and Simplified Chinese;
- every supported language can resolve every catalog key;
- English is the in-memory product default;
- a missing settings file selects English regardless of representative Windows UI cultures;
- a legacy existing profile without `language=` retains Portuguese;
- explicit saved selections restore all five supported languages;
- first-run selection persists;
- unsupported Traditional Chinese selection is rejected;
- missing localization keys are rejected.

The self-contained release gate runs this smoke test before promoting a candidate executable.

## First-run UI

The language chooser itself uses English as the neutral/default interface before the user has selected another language:

- title: `WinSidebar — Language`
- prompt: `Choose your language`
- language names are displayed in their native forms.

## Public documentation boundary

This document defines the **software/runtime** language contract. Public repository documentation is a separate next stage and must be converted consistently into the same five languages, with English as the repository/default entry point. Public translation must not opportunistically change product behavior.

## Closure state

The owner reported completion of the self-contained local release gate on the pre-English-default checkpoint `checkpoint/self-contained-owner-release-20260924` at `790ad23d6214728cbbe8e0cd1ea340cbab9bf86b`.

The English-default source/test closure was applied after that checkpoint, so the local self-contained release gate must be rerun on the new feature head before this runtime localization contract is considered build-verified.
