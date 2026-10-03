# Resume Prompt — WinSidebar emergency handoff

Continue the frozen WinSidebar project from the GitHub repository `hamthet/WinSidebar`.

Before changing anything:

1. Fetch the repository and check out `handoff/emergency-checkpoint-20261002`.
2. Read `HANDOFF.md` completely before making any proposal or edit.
3. Verify the branch HEAD and inspect Git history.
4. Verify that the frozen source/work-state commit `352f4c560e6d14db7117c5d450f55790abc7cc72` exists and is the base immediately before the handoff-documentation commit(s).
5. Inspect `main`; the handoff expected `main` to be `790cd352b11f67625f121a8fb9dcd4d9022ed6e0`. If it differs, report the divergence before doing work.
6. Inspect original work branch `docs/localization-art-audit-2.0` and PR #9. Do not assume its state is unchanged merely because HANDOFF.md says so.
7. Compare PR #9 branch against current `main`; report ahead/behind, changed files, PR state, unresolved review threads and whether any runtime files changed.
8. Do not presume incomplete work functions. Do not merge PR #9 merely because it is mergeable.
9. Respect all product decisions, restrictions, test claims and unknowns in HANDOFF.md.
10. Do not redo already-completed localization/artwork corrections without evidence that they regressed.
11. If HANDOFF.md and the repository disagree, stop the affected action and report the exact divergence.

The project was explicitly frozen on 2026-10-02. Do not restart development unless the owner's new message explicitly resumes it.

When the owner resumes, the immediate next technical objective is:

**Run the modified canonical verifier from the exact frozen PR #9 branch under Windows PowerShell 5.1 with .NET 8, without changing files first.**

Preferred sequence:

    git fetch origin --prune --tags
    git switch docs/localization-art-audit-2.0
    git reset --hard origin/docs/localization-art-audit-2.0
    git status --short
    git rev-parse HEAD

Expected frozen work-state SHA before any new work:

    352f4c560e6d14db7117c5d450f55790abc7cc72

If the branch still matches that SHA and the working tree is clean, run:

    .\tools\verify-release.ps1 -Dotnet dotnet -OutputRoot 'artifacts\release'

Record the complete final result accurately.

Success condition:
- actual observed `WINSIDEBAR 2.0 RELEASE VERIFICATION: PASS`;
- SDK is 8.x;
- no uncommitted source changes were required to obtain PASS.

Failure condition:
- preserve the exact error/output;
- do not claim PASS;
- diagnose only the failing gate;
- do not change runtime code unless the owner explicitly authorizes a runtime change.

After a PASS:
- re-audit PR #9 scope against current `main`;
- record test evidence in the PR if appropriate;
- obtain/confirm explicit owner approval before merge;
- do not alter tag `v2.0` or published assets as part of this PR.

Environment convention from the owner:
- no prefix = corporate/native mode;
- message beginning with `em casa` = home mode with admin/CMD/broader tooling;
- home work must remain Git-reproducible and must not create dependencies that break return to the corporate environment;
- canonical 2.0 validation uses .NET 8;
- GitHub Actions currently have no usable credits, so do not rely on hosted Actions.

Do not delete historical branches, cleanup refs, edit the published Release, or perform unrelated housekeeping while resuming PR #9 unless the owner explicitly reopens those tasks.
