# Release procedure and current 2.0 publication

Public product version: 2.0  
Published tag: `v2.0`  
Published release: https://github.com/hamthet/WinSidebar/releases/tag/v2.0  
Published source commit: `2597fe5ddda8906fd4e2a31ab8591ed4e7f6a2a4`

Technical Windows assembly/file versions may use four numeric fields such as 2.0.0.0. That technical format is not a separate public product version.

## Current 2.0 publication state

WinSidebar 2.0 was published on 2026-09-25. The GitHub Release is public, non-draft and non-prerelease.

Published assets:

    WinSidebar-v2.0-win-x64.zip
    SHA256SUMS.txt

The annotated tag `v2.0` points to the published source commit above. Treat that tag as the immutable source baseline for the shipped 2.0 artifacts. The `main` branch can advance after publication with documentation or later maintenance work without changing the already-published release.

## Preconditions

For a release build:

- clean working tree;
- WinSidebar.csproj has Version=2.0;
- NeutralLanguage=en-US;
- RuntimeIdentifier=win-x64;
- SelfContained=true;
- PublishSingleFile=true;
- five locale catalogs have exact key parity;
- no internal development dossier or personal machine paths in the shipping tree.

## Canonical gate

Run from the repository root:

    .\tools\verify-release.ps1

If dotnet is not on PATH, use -Dotnet with the .NET 8 SDK executable. The script checks release metadata, repository structure, five-language parity, source localization references, smoke tests, self-contained publish, ZIP layout and SHA-256 hashes.

A PASS from this script is build/test evidence only; it does not authorize merge or publication.

## Final user-facing release folder

After a successful canonical gate, the selected OutputRoot is flat. Temporary build folders are removed. The final directory contains:

    WinSidebar.exe
    WinSidebar-v2.0-win-x64.zip
    SHA256SUMS.txt
    START-HERE.txt
    START-HERE.pt-BR.txt
    START-HERE.es-ES.txt
    START-HERE.ru-RU.txt
    START-HERE.zh-CN.txt
    LICENSE

This flat directory is for humans. Internal publish/staging/dist folders are build implementation details and must not remain after a PASS. The verifier rejects unrelated pre-existing content and checks the exact final file set, so a PASS cannot leave stale files or technical subdirectories in the selected OutputRoot.

## End-user ZIP

Name: WinSidebar-v2.0-win-x64.zip

Archive root:

    WinSidebar.exe
    START-HERE.txt
    START-HERE.pt-BR.txt
    START-HERE.es-ES.txt
    START-HERE.ru-RU.txt
    START-HERE.zh-CN.txt
    LICENSE

Do not bury the executable under bin/, publish/ or candidate folders inside the ZIP.

## 2.0 publication sequence

The following sequence is complete for the published `v2.0` release:

1. Review the release PR against main.
2. Record actual tests accurately.
3. Obtain explicit owner approval.
4. Merge the PR.
5. Confirm main contains the intended state.
6. Create tag v2.0 from the approved main commit.
7. Publish the GitHub Release and attach the verified ZIP/checksum artifacts.

Merge approval and release publication are separate decisions.

For a later public version, update all version-specific metadata, artifact names, verifier expectations and documentation before reusing this procedure. Do not move or repurpose the published `v2.0` tag.
