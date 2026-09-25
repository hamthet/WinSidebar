# Release procedure

Public product version: 2.0
Intended tag: v2.0

Technical Windows assembly/file versions may use four numeric fields such as 2.0.0.0. That technical format is not a separate public product version.

## Preconditions

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

## Integration

1. Review the release PR against main.
2. Record actual tests accurately.
3. Obtain explicit owner approval.
4. Merge the PR.
5. Confirm main contains the intended state.
6. Create tag v2.0 from the approved main commit.
7. Publish the GitHub Release and attach the verified ZIP/checksum artifacts.

Merge approval and release publication are separate decisions.