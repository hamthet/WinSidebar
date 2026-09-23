# Development-only runner for the text-snippet paste spike.
# Windows PowerShell 5.1; no administrator, CMD, GitHub Actions or FILEBRIDGE.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$expectedBranch = 'feature/text-snippets'
$expectedWinSidebarBlob = '1b9403fdd49f5cfe915d1176c1baee1c63a5f6df'
$expectedInjectionBlob = 'a6fd374dac8a50c382974ca337b57b8ac526ed4c'
$expectedProjectBlob = '54e8b0fafd513bdf8a878fc45903a751665bb06b'

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

$actualWinSidebarBlob = (git rev-parse 'HEAD:src/WinSidebar.cs').Trim()
$actualInjectionBlob = (git rev-parse 'HEAD:src/TextInjection.cs').Trim()
$actualProjectBlob = (git rev-parse 'HEAD:WinSidebar.csproj').Trim()
if ($actualWinSidebarBlob -ne $expectedWinSidebarBlob -or
    $actualInjectionBlob -ne $expectedInjectionBlob -or
    $actualProjectBlob -ne $expectedProjectBlob) {
    throw 'The probe source differs from the reviewed baseline. Pull/inspect before running.'
}

$dotnetRoot = Join-Path $env:USERPROFILE 'Desktop\HAMILTON_NAODELETAR\dotnet'
$dotnet = Join-Path $dotnetRoot 'dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) {
    throw "Portable .NET SDK not found: $dotnet"
}
$env:DOTNET_ROOT = $dotnetRoot
$env:PATH = "$dotnetRoot;$env:PATH"

if (Get-Process WinSidebar -ErrorAction SilentlyContinue) {
    throw 'A WinSidebar instance is already running. Exit it from the tray, then run this script again.'
}

Write-Host 'Running five-language localization smoke...'
& $dotnet run --project .\tests\LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Localization smoke failed.' }

Write-Host 'Running isolated snippet-store smoke...'
& $dotnet run --project .\tests\SnippetStoreSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Snippet-store smoke failed.' }

$output = Join-Path $env:TEMP ('WinSidebar-snippet-probe-' + [Guid]::NewGuid().ToString('N'))
Write-Host "Publishing probe to: $output"
& $dotnet publish .\WinSidebar.csproj -c Release -r win-x64 --self-contained true -o $output
if ($LASTEXITCODE -ne 0) { throw 'Windows publish failed.' }

$exe = Join-Path $output 'WinSidebar.exe'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Published WinSidebar.exe not found: $exe"
}

Write-Host ''
Write-Host 'Probe ready.'
Write-Host '1. Put a known value on the clipboard, e.g.: Set-Clipboard -Value "CLIPBOARD-ANTES"'
Write-Host '2. Focus a normal text field (ChatGPT/browser or Notepad).'
Write-Host '3. Press Ctrl+Shift+F1 and RELEASE the keys.'
Write-Host '4. Expected paste: five diagnostic lines (Portuguese, Russian, Chinese, emoji).'
Write-Host '5. After roughly 350 ms, Get-Clipboard should still return your previous clipboard value.'
Write-Host ''
Write-Host "Opening: $exe"
Start-Process -FilePath $exe
