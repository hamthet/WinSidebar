# WinSidebar — development dossier

**Branch:** `develop` · **Repository:** `hamthet/WinSidebar` · **Established:** 2026-09-22 · **Dossier language:** English (the product targets five interface locales).

This directory is **development-only documentation** on the public `develop` branch, not customer-facing material. It describes the product scope, confirmed repository state, decisions, implementation order, design audit, verification evidence and release gates. A new contributor should be able to continue using this directory and the repository without relying on a ChatGPT conversation or an unpublished plan. Do not include this directory in the release ZIP or merge it into final `main`. **The development branch is public**, not confidential: record no secrets, customer data, private paths, contact lists or unredacted logs.

## Read in order

1. [`BASELINE.md`](BASELINE.md): source-of-truth snapshot, known facts, legacy work and uncertainties.
2. [`PLAN.md`](PLAN.md): product-first multilingual implementation, architecture, acceptance tests and subsequent repository work.
3. [`PRODUCT-REVIEW.md`](PRODUCT-REVIEW.md): product-design assessment and requirements beyond translation; separates blockers, conditional work and optional enhancements.
4. [`VERIFICATION.md`](VERIFICATION.md): reproducible test matrix, traceability, release/package audits and evidence template.
5. [`DECISIONS.md`](DECISIONS.md): architectural decisions and change-control rules.
6. [`LOG.md`](LOG.md): dated, append-only engineering journal, distinguishing observed work from plans.

## Canonical workflow

`develop` is the integration branch; small feature branches may target it. First complete the **product**, including five real UI localizations, persistence/migration, manual usability and regression verification, and reliable portable packaging. Only then finalize per-language GitHub READMEs, tutorials and SVG/PNG artwork. The shipped v1.0.0 release remains historical Portuguese software and must not be relabeled. A new release requires its own build, artifact hash, evidence and version.

For every substantive change record the requirement ID and baseline SHA, files and reason, before/after behavior, verification command and result (or `NOT RUN`), known risks, and commit/PR link in `LOG.md`. Attach reproducible evidence to `VERIFICATION.md`. Re-evaluate the product gate when scope changes. A successful compile does not prove runtime localization or UI usability.

## Separation from final product

**Never merge this directory or a historical planning memo into final `main`.** At release prepare a clean release branch from latest approved `main`; selectively transfer only production source, resources, tests, release workflow, user-facing docs, images, license and package instructions. Audit both the final Git tree and ZIP manifest. The prematurely published `docs/i18n/LOCALIZATION-PLAN.md` was already removed from `main` and `develop` on 2026-09-22; check again before release. Preserve development history in `develop` and Git/PR history, not in the product source tree or binary. Ordinary production tests/build workflows and necessary maintainer documentation may remain in final `main`, but not internal plans and journals.

## Status key

`CONFIRMED` = read directly from repository or CI response; `PLANNED` = not implemented; `OPEN` = decision/investigation needed; `PASS` = executed and evidenced; `NOT RUN` = no evidence. No milestone is finished merely because a document describing it exists.
