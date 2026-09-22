# Localization rollout

English is the default language for the GitHub landing page (`README.md`). The existing Portuguese README remains at `docs/i18n/README.pt-BR.md`. Both have working links to each other. Spanish (`es-ES`), Russian (`ru-RU`), and Simplified Chinese (`zh-CN`) will receive their own README files in separate localization stages; do not add links to untranslated or missing pages.

Concept illustrations are locale-specific. English SVG and PNG artwork is under `assets/i18n/en-US/`, while the original Portuguese artwork remains under `assets/`. Artwork must retain its prominent concept-art/not-a-screenshot disclosure, including when exported to PNG. Update the GitHub Actions asset-rendering workflow for each new locale.

The released v1.0.0 executable still has a Portuguese-language UI, and the bundled `LEIA-ME.txt` remains in Portuguese. English documentation must state this clearly until in-app localization and a new release actually ship. Do not alter v1.0.0 release assets or suggest the public executable has been translated.

The next implementation phase is runtime UI localization: externalize strings and accessibility labels, preserve stable serialized shortcut types and user-defined names, provide language selection and safe persistence, then test the portable build on Windows. Complete the remaining translations one language per stage.
