# Development

## Prerequisites

- Windows for real WinForms execution.
- .NET 8 SDK to build and test.
- Git for source control.

End users do not need the SDK. The release executable is self-contained. Do not hard-code personal checkout or SDK paths in product source or public docs.

## Build

From repository root:

    dotnet build .\WinSidebar.csproj -c Release

## Smoke tests

    dotnet run --project .\tests\LocalizationSmoke.csproj -c Release
    dotnet run --project .\tests\SnippetStoreSmoke.csproj -c Release

## Self-contained publish

    dotnet publish .\WinSidebar.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o .\publish

Expected product publish payload: publish\WinSidebar.exe only.

## Real Windows checks

- start and exit normally;
- AltGr+Y toggles open/closed;
- F1-F4 open shortcuts 1-4;
- at least one shortcut opens;
- at least one snippet pastes into a normal external app;
- language changes and persists;
- current profile is not unexpectedly reset;
- window click activates a listed window;
- right-click window and shortcut menus still work.

## Localization changes

When an application-owned string changes, update all five languages and run LocalizationSmoke. Never translate user-authored names, paths, snippet text or external window titles.

## Persistence changes

A change to settings.ini, shortcuts.xml, snippets.json or ignored-apps.json is a user-data contract change. Document and test it explicitly.