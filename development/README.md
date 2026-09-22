# WinSidebar — development dossier

**Branch:** `develop` · **Repository:** `hamthet/WinSidebar` · **Established:** 2026-09-22 · **Working language:** Brazilian Portuguese (technical IDs remain in English).

This directory is a **private-to-development deliverable in the public `develop` branch**, not customer-facing documentation. It describes the product scope, confirmed repository state, decisions, implementation order, audit, test evidence, and release gates. Anyone taking over the work must be able to continue using this directory plus the repository; no ChatGPT conversation, unpublished plan, or undocumented assumption is required. Do not include this directory in the release ZIP or merge it into the final `main` tree. The branch is public because the repository is public: record no secrets, customer data, private paths, contact lists, or unredacted logs.

## Read in order

1. [`BASELINE.md`](BASELINE.md): source-of-truth snapshot, known facts, legacy work and uncertainties.
2. [`PLAN.md`](PLAN.md): ordered product-first multilingual implementation, architecture, acceptance tests and repository work.
3. [`PRODUCT-REVIEW.md`](PRODUCT-REVIEW.md): product-design assessment and requirements beyond translation; separates blockers, conditional work and optional enhancements.
4. [`VERIFICATION.md`](VERIFICATION.md): reproducible test matrix, traceability, release/package audits and evidence template.
5. [`DECISIONS.md`](DECISIONS.md): architectural decisions and change-control rules.
6. [`LOG.md`](LOG.md): append-only dated development journal, with actual work distinguished from plans.

## Canonical workflow

`develop` is the integration branch; small feature branches may target it. First complete the **product**, including five real UI localizations, persistence/migration, manual usability and regression verification, reliable portable packaging. Only then finalize per-language GitHub READMEs, tutorials and SVG/PNG artwork. Keep the shipped v1.0.0 release as historical Portuguese software and do not relabel its binary. A new release requires its own build, artifact hash, evidence and version.

For every change: record the requirement ID and baseline SHA, the files and reason, before/after behavior, verification command and result (or `NOT RUN`), known risks, and commit/PR link in `LOG.md`. Attach reproducible evidence to `VERIFICATION.md`. Re-evaluate the product gate when scope changes. No green check from a compile-only workflow proves runtime localization or UI usability.

## Separation from final product

**Never merge this directory or any historical planning memo into final `main`.** At release time prepare a clean release branch from the latest approved `main`; transfer *only* selected source code, resources, tests, release workflow, customer-facing docs, images, license and package instructions. Review the changed-file allowlist and the produced ZIP separately. Remove the legacy `docs/i18n/LOCALIZATION-PLAN.md` from the final product tree: it was introduced prematurely on `main`. Preserve developmental history in `develop` and Git/PR history, not in the shipping codebase or binary. The final repo can include ordinary engineering tests/build workflows and necessary user or maintainer documentation, but not internal plans or process journals.

## Status key

`CONFIRMED` = read directly from repository or CI response; `PLANNED` = not implemented; `OPEN` = requires decision/investigation; `PASS` = executed and evidenced; `NOT RUN` = no evidence. No milestone is marked done because a document describing it exists.
