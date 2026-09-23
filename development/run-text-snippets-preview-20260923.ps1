# Owner-run preview for expandable shortcuts and text snippets.
# Windows PowerShell 5.1; no administrator, CMD, GitHub Actions or FILEBRIDGE.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$expectedBranch = 'feature/text-snippets'
$expected = @{
    'src/WinSidebar.cs'    = 'a13e32e8c65f94d7213dc3cbfe35de51ea1feb74'
    'src/ShortcutConfig.cs'= 'ff96339e5830f60e9bb119d2154b1e60389e38bc'
    'src/SnippetStore.cs'  = 'f4f9d08f2399b64e6f8bfdd4460d10d523214892'
    'src/SnippetEditor.cs' = '7477fb7eacafee41cb63b66cb4941ff9306621a7'
    'src/TextInjection.cs' = '4b5e893363a4e4f9827dd003552252c2f84b526d'
    'WinSidebar.csproj'    = 'a7c5f14eaccefd2fb33d93166e51ab7bf445318c'
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
Write-Host '  A. ATALHOS no longer has the original gear/edit mode.'
Write-Host '  B. Left-click an existing shortcut: it opens normally. Right-click it: its editor opens directly.'
Write-Host '  C. Click + in ATALHOS: a new row of four empty shortcuts appears and can be right-click edited.'
Write-Host '  D. Click + in SCRIPTS: Script 5 appears; its gear edits it and its row click pastes it.'
Write-Host '  E. Ctrl+Shift+F1..F4 still control only Scripts 1..4; extra scripts have no invented global hotkeys.'
Write-Host '  F. The ⇔ and ⇕ size cycles, mouse-click paste focus restoration and existing context menus still work.'
Write-Host '  G. Restart and verify extra rows, names, contents and selected width/height persist.'
Write-Host ''
Write-Host "Opening: $exe"
Start-Process -FilePath $exe
