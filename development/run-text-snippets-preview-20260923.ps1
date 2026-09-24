# Owner-run preview for expandable shortcuts and text snippets.
# Windows PowerShell 5.1; no administrator, CMD, GitHub Actions or FILEBRIDGE.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$expectedBranch = 'feature/text-snippets'
$expected = @{
    'src/WinSidebar.cs'            = 'fa845dfd7b425e06209abc3cd5eecb1acbd6099f'
    'src/Localization.cs'          = 'bb7f64977fc4dcc4791512ebc5bbd77ffcd5bb69'
    'src/FirstRunLanguageDialog.cs'= 'a582010162cdd07f55f32ef4ff9b6ebb5ef957e7'
    'src/ShortcutConfig.cs'        = 'a4db7728b8aeec6d48b2f4909524443e809a7a4c'
    'src/SnippetStore.cs'          = '6d4835d4ad3ff36e66a134a8afcb32e4a5798a66'
    'src/SnippetEditor.cs'         = '65cd78d0e0caa47776d6217d0a72f202e93ced55'
    'src/TextInjection.cs'         = '4b5e893363a4e4f9827dd003552252c2f84b526d'
    'WinSidebar.csproj'            = 'bb7d1acac365a5771e9e17534a2ed3ebbb5df17a'
    'tests/LocalizationSmoke.cs'   = 'e28b60f87dd5555f9a7ab3dcc9f0c2282b17cfed'
    'tests/SnippetStoreSmoke.cs'   = 'b144c31fd904677d00e1217d3f0f78133cf10283'
}

$rootFromGit = (git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Unable to resolve the Git repository root.' }
$root = (Resolve-Path -LiteralPath $rootFromGit).ProviderPath.TrimEnd('\')
$current = (Resolve-Path -LiteralPath '.').ProviderPath.TrimEnd('\')
if (-not [string]::Equals($root, $current, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Run this script from the WinSidebar repository root. Git root: $root ; current: $current"
}
$branch = (git branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $branch -ne $expectedBranch) {
    throw "Unexpected branch: $branch"
}
$changes = @(git status --porcelain)
if ($LASTEXITCODE -ne 0 -or $changes.Count -ne 0) {
    throw 'Working tree is not clean. Preserve local changes before testing.'
}
foreach ($path in $expected.Keys) {
    $blob = (git rev-parse "HEAD:$path").Trim()
    if ($LASTEXITCODE -ne 0 -or $blob -ne $expected[$path]) {
        throw "Reviewed source mismatch: $path ($blob). Pull and inspect before running."
    }
}

$dotnetRoot = 'C:\dotnet8'
$dotnet = Join-Path $dotnetRoot 'dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) {
    throw "Portable .NET 8 SDK not found: $dotnet"
}
$version = (& $dotnet --version).Trim()
if ($LASTEXITCODE -ne 0 -or -not $version.StartsWith('8.')) {
    throw "WinSidebar requires the .NET 8 SDK for development. Found: $version"
}
$env:DOTNET_ROOT = $dotnetRoot
$env:PATH = "$dotnetRoot;$env:PATH"

if (Get-Process WinSidebar -ErrorAction SilentlyContinue) {
    throw 'A WinSidebar instance is already running. Exit it from the tray, then run this preview again.'
}

Write-Host 'Running five-language localization smoke...'
& $dotnet run --project .\tests\LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Localization smoke failed.' }

Write-Host 'Running isolated snippet-store smoke...'
& $dotnet run --project .\tests\SnippetStoreSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Snippet-store smoke failed.' }

$previewRoot = 'C:\H\filebridge\WinSidebar\preview'
New-Item -ItemType Directory -Path $previewRoot -Force | Out-Null
$output = Join-Path $previewRoot ('WinSidebar-snippets-preview-' + [Guid]::NewGuid().ToString('N'))
Write-Host "Publishing preview to: $output"
& $dotnet publish .\WinSidebar.csproj -c Release -r win-x64 --self-contained true -o $output
if ($LASTEXITCODE -ne 0) { throw 'Windows publish failed.' }

$exe = Join-Path $output 'WinSidebar.exe'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Published WinSidebar.exe not found: $exe"
}

Write-Host ''
Write-Host 'Preview ready. This runner does NOT delete or reset %LOCALAPPDATA%\WinSidebar; the existing profile is intentionally preserved.'
Write-Host 'Test plan:'
Write-Host '  A. Keep the current profile. Do NOT delete or reset %LOCALAPPDATA%\WinSidebar.'
Write-Host '  B. Collapse the sidebar, then press Alt+" (Alt+Shift+the quote/apostrophe key). It must open the sidebar.'
Write-Host '  C. Press Alt+" again while already open: no second action, popup or duplicate effect.'
Write-Host '  D. F1..F4 still open shortcuts 1..4; configured Shift+F hotkeys still paste their scripts.'
Write-Host '  E. Script gear hotkey editing, auto-fit vertical sizing and right-click shortcut editing must remain unchanged.'
Write-Host '  F. Close and reopen with the same profile; report if the previous shortcut-load error returns.'
Write-Host ''
Write-Host "Opening: $exe"
Start-Process -FilePath $exe
