# Local release gate for a self-contained WinSidebar candidate.
# Windows PowerShell 5.1; no administrator, CMD, GitHub Actions or remote FILEBRIDGE.
[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$expectedBranch = 'feature/text-snippets'
$dotnetRoot = 'C:\dotnet8'
$dotnet = Join-Path $dotnetRoot 'dotnet.exe'
$releaseRoot = 'C:\H\filebridge\WinSidebar\release'
$stableRoot = 'C:\H\files\WinSidebar\current'
$previousRoot = 'C:\H\files\WinSidebar\previous'

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
if (@(git status --porcelain).Count -ne 0) {
    throw 'Working tree is not clean. Preserve local changes before building the release candidate.'
}
if (Get-Process WinSidebar -ErrorAction SilentlyContinue) {
    throw 'Exit WinSidebar before the release gate so the stable executable can be updated safely.'
}

if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) {
    throw "Portable .NET 8 SDK not found: $dotnet"
}
$version = (& $dotnet --version).Trim()
if ($LASTEXITCODE -ne 0 -or -not $version.StartsWith('8.')) {
    throw "WinSidebar requires the .NET 8 SDK for development. Found: $version"
}
$env:DOTNET_ROOT = $dotnetRoot
$env:PATH = "$dotnetRoot;$env:PATH"

[xml]$project = Get-Content -LiteralPath '.\WinSidebar.csproj' -Raw
$properties = $project.Project.PropertyGroup | Select-Object -First 1
if ($properties.TargetFramework -ne 'net8.0-windows') {
    throw "Unexpected target framework: $($properties.TargetFramework)"
}
if ($properties.NeutralLanguage -ne 'en-US') {
    throw "Unexpected neutral/default language: $($properties.NeutralLanguage)"
}
if ($properties.RuntimeIdentifier -ne 'win-x64' -or
    $properties.SelfContained -ne 'true' -or
    $properties.PublishSingleFile -ne 'true') {
    throw 'Project is not configured as win-x64 self-contained single-file.'
}

Write-Host 'Running five-language localization smoke...'
& $dotnet run --project .\tests\LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Localization smoke failed.' }

Write-Host 'Running isolated snippet-store smoke...'
& $dotnet run --project .\tests\SnippetStoreSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Snippet-store smoke failed.' }

New-Item -ItemType Directory -Path $releaseRoot -Force | Out-Null
$short = (git rev-parse --short=10 HEAD).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Unable to resolve Git commit.' }
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$work = Join-Path $releaseRoot ("candidate-$stamp-$short")
$publish = Join-Path $work 'publish'
$dist = Join-Path $work 'dist'
New-Item -ItemType Directory -Path $publish,$dist -Force | Out-Null

Write-Host "Publishing self-contained single EXE to: $publish"
& $dotnet publish .\WinSidebar.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $publish
if ($LASTEXITCODE -ne 0) { throw 'Self-contained publish failed.' }

$publishedFiles = @(Get-ChildItem -LiteralPath $publish -File -Recurse)
if ($publishedFiles.Count -ne 1) {
    $names = ($publishedFiles | ForEach-Object FullName) -join [Environment]::NewLine
    throw ("Expected exactly one published file. Found " + $publishedFiles.Count + ":" +
        [Environment]::NewLine + $names)
}

$exe = $publishedFiles[0]
if ($exe.Name -ne 'WinSidebar.exe') {
    throw "Expected WinSidebar.exe, found: $($exe.Name)"
}
if ($exe.Length -lt 10000000) {
    throw "WinSidebar.exe is unexpectedly small for a self-contained .NET 8 build: $($exe.Length) bytes"
}

$distExe = Join-Path $dist 'WinSidebar.exe'
Copy-Item -LiteralPath $exe.FullName -Destination $distExe

$zipName = "WinSidebar-$short-win-x64-self-contained.zip"
$zip = Join-Path $dist $zipName
Compress-Archive -LiteralPath $distExe -DestinationPath $zip -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($zip)
try {
    $entries = @($archive.Entries | ForEach-Object FullName)
    if ($entries.Count -ne 1 -or $entries[0] -ne 'WinSidebar.exe') {
        throw "Unexpected ZIP contents: $($entries -join ', ')"
    }
}
finally { $archive.Dispose() }

$exeHash = (Get-FileHash -LiteralPath $distExe -Algorithm SHA256).Hash.ToLowerInvariant()
$zipHash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()
$sumPath = Join-Path $dist 'SHA256SUMS.txt'
$content = $exeHash + '  WinSidebar.exe' + [Environment]::NewLine +
    $zipHash + '  ' + $zipName + [Environment]::NewLine
[IO.File]::WriteAllText($sumPath, $content, [Text.UTF8Encoding]::new($false))

# Promote only a fully verified candidate to a stable local path. The dated
# candidate remains immutable for evidence; desktop shortcuts should target stableRoot.
New-Item -ItemType Directory -Path $stableRoot,$previousRoot -Force | Out-Null
$stableExe = Join-Path $stableRoot 'WinSidebar.exe'
$previousExe = Join-Path $previousRoot 'WinSidebar.exe'
$stagedStable = Join-Path $stableRoot 'WinSidebar.exe.new'
Copy-Item -LiteralPath $distExe -Destination $stagedStable -Force
if (Test-Path -LiteralPath $stableExe) {
    Copy-Item -LiteralPath $stableExe -Destination $previousExe -Force
    Remove-Item -LiteralPath $stableExe -Force
}
Move-Item -LiteralPath $stagedStable -Destination $stableExe
$stableHash = (Get-FileHash -LiteralPath $stableExe -Algorithm SHA256).Hash.ToLowerInvariant()
if ($stableHash -ne $exeHash) {
    throw 'Stable executable hash does not match the verified release candidate.'
}

Write-Host ''
Write-Host 'SELF-CONTAINED RELEASE GATE: PASS'
Write-Host "Git commit: $short"
Write-Host "SDK used only for build: $version at $dotnet"
Write-Host "Published payload: one file -> $distExe"
Write-Host "ZIP payload: WinSidebar.exe only -> $zip"
Write-Host "Checksums: $sumPath"
Write-Host "Stable executable for desktop shortcut: $stableExe"
if (Test-Path -LiteralPath $previousExe) {
    Write-Host "Previous stable executable preserved at: $previousExe"
}
Write-Host ''
Write-Host 'The end-user executable contains the .NET runtime; no separate .NET installation is required.'
Write-Host 'This gate does not publish a GitHub Release or modify main.'
