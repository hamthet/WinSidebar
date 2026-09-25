# WinSidebar 2.0 local release verifier.
# Windows PowerShell 5.1 compatible and intentionally ASCII-only.
[CmdletBinding()]
param(
    [string]$Dotnet = 'dotnet',
    [string]$OutputRoot = 'artifacts\release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$rootFromGit = (git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Unable to resolve Git repository root.' }
$root = (Resolve-Path -LiteralPath $rootFromGit).ProviderPath
Set-Location -LiteralPath $root

if (@(git status --porcelain).Count -ne 0) {
    throw 'Working tree is not clean. Preserve local changes before release verification.'
}

if (-not (Get-Command $Dotnet -ErrorAction SilentlyContinue) -and -not (Test-Path -LiteralPath $Dotnet -PathType Leaf)) {
    throw "dotnet executable not found: $Dotnet"
}
$sdkVersion = (& $Dotnet --version).Trim()
if ($LASTEXITCODE -ne 0 -or -not $sdkVersion.StartsWith('8.')) {
    throw "WinSidebar 2.0 release verification requires a .NET 8 SDK. Found: $sdkVersion"
}

[xml]$project = [IO.File]::ReadAllText((Join-Path $root 'WinSidebar.csproj'))
$p = $project.Project.PropertyGroup | Select-Object -First 1
if ($p.Version -ne '2.0') { throw "Unexpected public version: $($p.Version)" }
if ($p.AssemblyVersion -ne '2.0.0.0' -or $p.FileVersion -ne '2.0.0.0') {
    throw 'Unexpected technical Windows assembly/file version.'
}
if ($p.NeutralLanguage -ne 'en-US') { throw 'English must remain the neutral/default language.' }
if ($p.RuntimeIdentifier -ne 'win-x64' -or $p.SelfContained -ne 'true' -or $p.PublishSingleFile -ne 'true') {
    throw 'Project is not configured as win-x64 self-contained single-file.'
}
if (Test-Path -LiteralPath (Join-Path $root 'development')) {
    throw 'Internal development dossier must not be present on the shipping branch.'
}

$required = @(
    'README.md', 'START-HERE.txt', 'PROJECT.md', 'project.json', 'CONTRIBUTING.md',
    'docs\README.md', 'docs\FAQ.md', 'docs\TUTORIAL.md', 'docs\RELEASE-NOTES.md',
    'docs\ARCHITECTURE.md', 'docs\DATA-FORMATS.md', 'docs\DEVELOPMENT.md', 'docs\RELEASE.md',
    'docs\i18n\START-HERE.pt-BR.txt', 'docs\i18n\START-HERE.es-ES.txt',
    'docs\i18n\START-HERE.ru-RU.txt', 'docs\i18n\START-HERE.zh-CN.txt',
    'docs\i18n\FAQ.pt-BR.md', 'docs\i18n\FAQ.es-ES.md',
    'docs\i18n\FAQ.ru-RU.md', 'docs\i18n\FAQ.zh-CN.md'
)
foreach ($path in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $root $path) -PathType Leaf)) {
        throw "Missing required release file: $path"
    }
}

$utf8 = [Text.UTF8Encoding]::new($false)
$manifest = [IO.File]::ReadAllText((Join-Path $root 'project.json'), $utf8) | ConvertFrom-Json
if ($manifest.publicVersion -ne '2.0') { throw 'project.json publicVersion mismatch.' }
if ($manifest.localization.default -ne 'en-US') { throw 'project.json default language mismatch.' }
if ((@($manifest.localization.supported) -join '|') -ne 'en-US|pt-BR|es-ES|ru-RU|zh-CN') {
    throw 'project.json supported-language contract mismatch.'
}
if ($manifest.distribution.executable -ne 'WinSidebar.exe' -or
    $manifest.distribution.archive -ne 'WinSidebar-v2.0-win-x64.zip' -or
    -not $manifest.distribution.selfContained -or -not $manifest.distribution.singleFile) {
    throw 'project.json distribution contract mismatch.'
}

Write-Host 'Auditing five-language catalog parity...'
$base = [IO.File]::ReadAllText((Join-Path $root 'i18n\catalog.json'), $utf8) | ConvertFrom-Json
$baseKeys = @($base.PSObject.Properties.Name)
if ($baseKeys.Count -lt 100) { throw 'Localization base catalog is unexpectedly small.' }
foreach ($key in $baseKeys) {
    $entry = $base.$key
    $entryKeys = @($entry.PSObject.Properties.Name | Sort-Object)
    if (($entryKeys -join '|') -ne 'en-US|pt-BR') { throw "Base locale parity failure: $key" }
    if ([string]::IsNullOrWhiteSpace($entry.'en-US') -or [string]::IsNullOrWhiteSpace($entry.'pt-BR')) {
        throw "Blank base translation: $key"
    }
}
foreach ($code in @('es-ES','ru-RU','zh-CN')) {
    $locale = [IO.File]::ReadAllText((Join-Path $root ('i18n\' + $code + '.json')), $utf8) | ConvertFrom-Json
    $keys = @($locale.PSObject.Properties.Name)
    if ($keys.Count -ne $baseKeys.Count) { throw "$code catalog count mismatch." }
    foreach ($key in $baseKeys) {
        if (-not ($locale.PSObject.Properties.Name -contains $key) -or [string]::IsNullOrWhiteSpace($locale.$key)) {
            throw "$code catalog mismatch: $key"
        }
    }
}

$sourceText = (Get-ChildItem -LiteralPath (Join-Path $root 'src') -Filter '*.cs' -File | ForEach-Object {
    [IO.File]::ReadAllText($_.FullName, $utf8)
}) -join [Environment]::NewLine
$refs = [regex]::Matches($sourceText, 'Localization\.Text\("([^"]+)"\)') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique
foreach ($key in $refs) {
    if (-not ($baseKeys -contains $key)) { throw "Undefined localization key referenced by source: $key" }
}

Write-Host 'Running localization smoke...'
& $Dotnet run --project .\tests\LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Localization smoke failed.' }

Write-Host 'Running snippet-store smoke...'
& $Dotnet run --project .\tests\SnippetStoreSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Snippet-store smoke failed.' }

$output = Join-Path $root $OutputRoot
$publish = Join-Path $output 'publish'
$staging = Join-Path $output 'staging'
$dist = Join-Path $output 'dist'
foreach ($dir in @($publish,$staging,$dist)) {
    if (Test-Path -LiteralPath $dir) { Remove-Item -LiteralPath $dir -Recurse -Force }
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
}

Write-Host 'Publishing self-contained single EXE...'
& $Dotnet publish .\WinSidebar.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o $publish
if ($LASTEXITCODE -ne 0) { throw 'Self-contained publish failed.' }
$published = @(Get-ChildItem -LiteralPath $publish -File)
if ($published.Count -ne 1 -or $published[0].Name -ne 'WinSidebar.exe' -or $published[0].Length -lt 10000000) {
    throw 'Expected exactly one self-contained WinSidebar.exe.'
}

Copy-Item -LiteralPath $published[0].FullName -Destination (Join-Path $staging 'WinSidebar.exe')
Copy-Item -LiteralPath (Join-Path $root 'LICENSE') -Destination (Join-Path $staging 'LICENSE')
Copy-Item -LiteralPath (Join-Path $root 'START-HERE.txt') -Destination (Join-Path $staging 'START-HERE.txt')
foreach ($code in @('pt-BR','es-ES','ru-RU','zh-CN')) {
    Copy-Item -LiteralPath (Join-Path $root ('docs\i18n\START-HERE.' + $code + '.txt')) -Destination (Join-Path $staging ('START-HERE.' + $code + '.txt'))
}

$zip = Join-Path $dist 'WinSidebar-v2.0-win-x64.zip'
Compress-Archive -Path (Join-Path $staging '*') -DestinationPath $zip -CompressionLevel Optimal

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($zip)
try {
    $actual = @($archive.Entries | ForEach-Object FullName | Sort-Object)
    $expected = @('LICENSE','START-HERE.es-ES.txt','START-HERE.pt-BR.txt','START-HERE.ru-RU.txt','START-HERE.txt','START-HERE.zh-CN.txt','WinSidebar.exe')
    if (($actual -join '|') -ne ($expected -join '|')) { throw "Unexpected ZIP contents: $($actual -join ', ')" }
}
finally { $archive.Dispose() }

$exeHash = (Get-FileHash -LiteralPath $published[0].FullName -Algorithm SHA256).Hash.ToLowerInvariant()
$zipHash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()
$sum = Join-Path $dist 'SHA256SUMS.txt'
[IO.File]::WriteAllLines($sum, @(
    "$exeHash  WinSidebar.exe",
    "$zipHash  WinSidebar-v2.0-win-x64.zip"
), $utf8)

Write-Host ''
Write-Host 'WINSIDEBAR 2.0 RELEASE VERIFICATION: PASS'
Write-Host "SDK: $sdkVersion"
Write-Host "EXE: $($published[0].FullName)"
Write-Host "ZIP: $zip"
Write-Host "SHA256: $sum"
