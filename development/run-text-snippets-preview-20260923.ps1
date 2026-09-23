# Owner-run preview for expandable shortcuts and text snippets.
# Windows PowerShell 5.1; no administrator, CMD, GitHub Actions or FILEBRIDGE.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$expectedBranch = 'feature/text-snippets'
$expected = @{
    'src/WinSidebar.cs'            = '396fb3ff8eae93c5911bdc4382f1eb40ddd7b89a'
    'src/Localization.cs'          = 'bb7f64977fc4dcc4791512ebc5bbd77ffcd5bb69'
    'src/FirstRunLanguageDialog.cs'= 'a582010162cdd07f55f32ef4ff9b6ebb5ef957e7'
    'src/ShortcutConfig.cs'        = 'a4db7728b8aeec6d48b2f4909524443e809a7a4c'
    'src/SnippetStore.cs'          = 'bd2e8da43d2dbdf9b042ab046584f275637bd216'
    'src/SnippetEditor.cs'         = '7477fb7eacafee41cb63b66cb4941ff9306621a7'
    'src/TextInjection.cs'         = '4b5e893363a4e4f9827dd003552252c2f84b526d'
    'WinSidebar.csproj'            = 'bb7d1acac365a5771e9e17534a2ed3ebbb5df17a'
    'tests/LocalizationSmoke.cs'   = 'e28b60f87dd5555f9a7ab3dcc9f0c2282b17cfed'
    'tests/SnippetStoreSmoke.cs'   = '3ec159d8b2234a4b156cd3c606131fac5432501a'
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

$dotnetRoot = Join-Path $env:USERPROFILE 'Desktop\HAMILTON_NAODELETAR\dotnet'
$dotnet = Join-Path $dotnetRoot 'dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) {
    throw "Portable .NET SDK not found: $dotnet"
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

$output = Join-Path $env:TEMP ('WinSidebar-snippets-preview-' + [Guid]::NewGuid().ToString('N'))
Write-Host "Publishing preview to: $output"
& $dotnet publish .\WinSidebar.csproj -c Release -r win-x64 --self-contained true -o $output
if ($LASTEXITCODE -ne 0) { throw 'Windows publish failed.' }

$exe = Join-Path $output 'WinSidebar.exe'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Published WinSidebar.exe not found: $exe"
}

Write-Host ''
Write-Host 'Preview ready. No source or profile data was changed by this runner.'
Write-Host 'Test plan:'
Write-Host '  A. ATALHOS must show ↺, -, + and no globe. SCRIPTS must also show ↺, -, +.'
Write-Host '  B. Limits: ATALHOS max 12 (3x4); SCRIPTS max 8. + disables at the limit.'
Write-Host '  C. - removes only the last shortcut row (4) or last script (1), never below four.'
Write-Host '  D. Restore in ATALHOS asks confirmation and resets only shortcuts. Restore in SCRIPTS asks confirmation and resets only scripts.'
Write-Host '  E. Left-click opens every configured shortcut; right-click edits every shortcut, including folders.'
Write-Host '  F. Global F1..F4 open shortcuts 1..4. Shift+F1..F4 paste Scripts 1..4.'
Write-Host '  G. Extra shortcuts/scripts remain configurable and clickable but have no default global hotkey.'
Write-Host '  H. Script mouse-click paste, ⇔/⇕ size cycles, language in the right-click menu and window context menus must still work.'
Write-Host '  I. Restart and verify rows/content/size persist.'
Write-Host '  J. First-run language chooser will NOT appear on this existing profile; localization smoke verifies its persistence path separately.'
Write-Host ''
Write-Host "Opening: $exe"
Start-Process -FilePath $exe
