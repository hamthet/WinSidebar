# Self-contained distribution gate — 2026-09-24

**Producer:** `hamthet/WinSidebar`  
**Product branch under test:** `feature/text-snippets`  
**Status:** CONFIGURATION VERIFIED IN SOURCE / LOCAL RELEASE GATE PENDING OWNER EXECUTION  
**Official `main` and v1.0.0:** unchanged.

## Requirement

The WinSidebar end-user package must run on supported Windows x64 systems **without requiring a separate .NET runtime, SDK, installer, PowerShell, Git or other development dependency**.

Development dependencies are permitted only for building/testing the product.

## Current source configuration

`WinSidebar.csproj` already defines:

- `TargetFramework = net8.0-windows`
- `RuntimeIdentifier = win-x64`
- `SelfContained = true`
- `PublishSingleFile = true`
- native libraries included in the single-file publish
- trimming disabled
- debug symbols disabled

The repository's manual `.github/workflows/build.yml` also verifies that publish output is exactly one `WinSidebar.exe` and packages that executable. GitHub Actions remain disabled by owner decision for current development work; this is source evidence only, not a newly executed workflow.

## Local authoritative gate

Use:

`development/build-self-contained-release-20260924.ps1`

Local conventions:

- repository: `C:\Git\WinSidebar`
- .NET 8 SDK used **only for build**: `C:\dotnet8\dotnet.exe`
- immutable release-candidate artifacts: `C:\H\filebridge\WinSidebar\release`
- stable local executable for desktop shortcuts: `C:\H\files\WinSidebar\current\WinSidebar.exe`
- previous stable executable preserved for recovery: `C:\H\files\WinSidebar\previous\WinSidebar.exe`

The script:

1. refuses an unexpected branch or dirty working tree;
2. verifies that the SDK is .NET 8;
3. verifies the project self-contained/single-file properties;
4. runs the five-language localization smoke test;
5. runs the isolated snippet-store smoke test;
6. publishes `win-x64` with `--self-contained true` and `PublishSingleFile=true`;
7. rejects the result unless publish contains **exactly one file named `WinSidebar.exe`**;
8. rejects an implausibly small binary;
9. creates a ZIP whose sole payload is `WinSidebar.exe`;
10. writes SHA-256 hashes for the EXE and ZIP;
11. only after all checks pass, promotes the verified EXE to the stable `current` path and preserves the prior stable EXE under `previous`.

The gate does not merge, tag, publish a GitHub Release or modify `main`.

## Interpretation

A PASS demonstrates that the candidate artifact is the repository's intended .NET 8 **self-contained single-file** Windows x64 build. The .NET SDK remains a development dependency only and is not shipped or required by the user.

A clean-machine execution on a Windows system without a separately installed .NET runtime would be an additional deployment smoke test, but it is not required to establish the publish mode: self-contained publish embeds the required .NET runtime into the deliverable.

## Next project stage after PASS

By owner decision, successful completion of this gate closes the current functional/product-design stage. The next stage is repository-side total five-language conversion for:

- public README/tutorial/help material;
- release/in-ZIP instructions;
- public images/captions/text assets where applicable;
- current product descriptions and feature/hotkey documentation;
- language parity review across English, Brazilian Portuguese, Spanish, Russian and Simplified Chinese.

Do not change product behavior opportunistically during the translation stage. Functional changes discovered later require their own scoped task.

## Preservation

Current user profile remains at `%LOCALAPPDATA%\WinSidebar` and is not part of release packaging. Internal `development/**` files never enter the shipping ZIP. No remote FILEBRIDGE transfer is authorized by this gate.

## Stable desktop shortcut target

The dated `candidate-<timestamp>-<commit>` directory is intentionally versioned and changes every successful build. Desktop shortcuts must **not** target it. Use the stable path:

`C:\H\files\WinSidebar\current\WinSidebar.exe`

The release gate updates this stable executable only after the candidate passes all self-contained checks. This keeps the desktop shortcut valid while preserving immutable candidate evidence separately.
