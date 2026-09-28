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
    'README.md', 'START-HERE.txt', 'SUPPORT.md', 'PROJECT.md', 'project.json', 'CONTRIBUTING.md',
    'docs\README.md', 'docs\FAQ.md', 'docs\TUTORIAL.md', 'docs\RELEASE-NOTES.md', 'docs\OUTREACH.md',
    'docs\ARCHITECTURE.md', 'docs\DATA-FORMATS.md', 'docs\DESIGN-DECISIONS.md',
    'docs\KNOWN-LIMITATIONS.md', 'docs\DEVELOPMENT.md', 'docs\RELEASE.md',
    'docs\i18n\START-HERE.pt-BR.txt', 'docs\i18n\START-HERE.es-ES.txt',
    'docs\i18n\START-HERE.ru-RU.txt', 'docs\i18n\START-HERE.zh-CN.txt',
    'docs\i18n\FAQ.pt-BR.md', 'docs\i18n\FAQ.es-ES.md',
    'docs\i18n\FAQ.ru-RU.md', 'docs\i18n\FAQ.zh-CN.md',
    'docs\i18n\SUPPORT.pt-BR.md', 'docs\i18n\SUPPORT.es-ES.md',
    'docs\i18n\SUPPORT.ru-RU.md', 'docs\i18n\SUPPORT.zh-CN.md',
    'docs\i18n\README.pt-BR.md', 'docs\i18n\README.es-ES.md',
    'docs\i18n\README.ru-RU.md', 'docs\i18n\README.zh-CN.md',
    'docs\i18n\TUTORIAL.pt-BR.md', 'docs\i18n\TUTORIAL.es-ES.md',
    'docs\i18n\TUTORIAL.ru-RU.md', 'docs\i18n\TUTORIAL.zh-CN.md',
    'docs\i18n\RELEASE-NOTES.pt-BR.md', 'docs\i18n\RELEASE-NOTES.es-ES.md',
    'docs\i18n\RELEASE-NOTES.ru-RU.md', 'docs\i18n\RELEASE-NOTES.zh-CN.md',
    'docs\i18n\OUTREACH.pt-BR.md', 'docs\i18n\OUTREACH.es-ES.md',
    'docs\i18n\OUTREACH.ru-RU.md', 'docs\i18n\OUTREACH.zh-CN.md',
    'assets\hero-illustration.svg', 'assets\linkedin-illustration.svg',
    'assets\i18n\pt-BR\hero-illustration.svg', 'assets\i18n\pt-BR\linkedin-illustration.svg',
    'assets\i18n\es-ES\hero-illustration.svg', 'assets\i18n\es-ES\linkedin-illustration.svg',
    'assets\i18n\ru-RU\hero-illustration.svg', 'assets\i18n\ru-RU\linkedin-illustration.svg',
    'assets\i18n\zh-CN\hero-illustration.svg', 'assets\i18n\zh-CN\linkedin-illustration.svg',
    '.github\ISSUE_TEMPLATE\bug_report.yml', '.github\ISSUE_TEMPLATE\bug_report.pt-BR.yml',
    '.github\ISSUE_TEMPLATE\bug_report.es-ES.yml', '.github\ISSUE_TEMPLATE\bug_report.ru-RU.yml',
    '.github\ISSUE_TEMPLATE\bug_report.zh-CN.yml'
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

Write-Host 'Auditing public repository hygiene...'
$publicTextFiles = @(
    (Join-Path $root 'README.md'),
    (Join-Path $root 'START-HERE.txt'),
    (Join-Path $root 'PROJECT.md'),
    (Join-Path $root 'CONTRIBUTING.md')
)
$publicTextFiles += @(Get-ChildItem -LiteralPath (Join-Path $root 'docs') -File -Recurse |
    Where-Object { $_.Extension -eq '.md' -or $_.Extension -eq '.txt' } |
    ForEach-Object FullName)
$publicText = ($publicTextFiles | ForEach-Object { [IO.File]::ReadAllText($_, $utf8) }) -join [Environment]::NewLine

if ($publicText -match '(?<![0-9.])v?2[.]0[.]0(?![0-9.])') {
    throw 'Obsolete public version label 2.0.0 remains in user/repository documentation.'
}
foreach ($pattern in @(
    'C:\\Users\\',
    'C:\\Git\\',
    'C:\\H\\',
    'HAMILTON_NAODELETAR',
    'C:\\dotnet8',
    'feature/text-snippets',
    'chore/release-2.0.0'
)) {
    if ($publicText -match $pattern) { throw "Personal/obsolete repository reference remains in public docs: $pattern" }
}

Write-Host 'Auditing localized product-document parity...'
$localizedProductDocs = @(
    'README.md',
    'docs\i18n\README.pt-BR.md', 'docs\i18n\README.es-ES.md',
    'docs\i18n\README.ru-RU.md', 'docs\i18n\README.zh-CN.md',
    'docs\TUTORIAL.md',
    'docs\i18n\TUTORIAL.pt-BR.md', 'docs\i18n\TUTORIAL.es-ES.md',
    'docs\i18n\TUTORIAL.ru-RU.md', 'docs\i18n\TUTORIAL.zh-CN.md',
    'docs\RELEASE-NOTES.md',
    'docs\i18n\RELEASE-NOTES.pt-BR.md', 'docs\i18n\RELEASE-NOTES.es-ES.md',
    'docs\i18n\RELEASE-NOTES.ru-RU.md', 'docs\i18n\RELEASE-NOTES.zh-CN.md'
)
foreach ($path in $localizedProductDocs) {
    $document = [IO.File]::ReadAllText((Join-Path $root $path), $utf8)
    foreach ($token in @('WinSidebar 2.0', 'AltGr+Y', 'F1', 'F12', '12', '8')) {
        if (-not $document.Contains($token)) {
            throw "Localized product documentation is missing a current 2.0 contract token: $path / $token"
        }
    }
}

$localizedStartDocs = @(
    'START-HERE.txt',
    'docs\i18n\START-HERE.pt-BR.txt', 'docs\i18n\START-HERE.es-ES.txt',
    'docs\i18n\START-HERE.ru-RU.txt', 'docs\i18n\START-HERE.zh-CN.txt'
)
foreach ($path in $localizedStartDocs) {
    $document = [IO.File]::ReadAllText((Join-Path $root $path), $utf8)
    foreach ($token in @('WinSidebar 2.0', 'Windows 10/11 x64', '.NET 8', 'AltGr+Y', 'F1-F4', 'Shift+F1-F4', '%LOCALAPPDATA%\WinSidebar', 'WinSidebar.exe')) {
        if (-not $document.Contains($token)) {
            throw "Localized start-here documentation is missing a current 2.0 contract token: $path / $token"
        }
    }
}

$localizedSupportDocs = @(
    'SUPPORT.md',
    'docs\i18n\SUPPORT.pt-BR.md', 'docs\i18n\SUPPORT.es-ES.md',
    'docs\i18n\SUPPORT.ru-RU.md', 'docs\i18n\SUPPORT.zh-CN.md'
)
foreach ($path in $localizedSupportDocs) {
    $document = [IO.File]::ReadAllText((Join-Path $root $path), $utf8)
    foreach ($token in @('WinSidebar 2.0', 'Windows 10', 'Windows 11', '%LOCALAPPDATA%\WinSidebar', 'shortcuts.xml', 'snippets.json', 'ignored-apps.json', 'icons/')) {
        if (-not $document.Contains($token)) {
            throw "Localized support documentation is missing a current 2.0 contract token: $path / $token"
        }
    }
}

$localizedOutreachDocs = @(
    'docs\OUTREACH.md',
    'docs\i18n\OUTREACH.pt-BR.md', 'docs\i18n\OUTREACH.es-ES.md',
    'docs\i18n\OUTREACH.ru-RU.md', 'docs\i18n\OUTREACH.zh-CN.md'
)
foreach ($path in $localizedOutreachDocs) {
    $document = [IO.File]::ReadAllText((Join-Path $root $path), $utf8)
    foreach ($token in @('WinSidebar 2.0', 'Windows 10/11 x64', '.NET 8', 'AltGr+Y', 'F1', '12', '8', 'Alt+Tab')) {
        if (-not $document.Contains($token)) {
            throw "Localized outreach documentation is missing a current 2.0 contract token: $path / $token"
        }
    }
}

$outreachAssetPaths = @{
    'docs\OUTREACH.md' = @('../assets/linkedin-illustration.svg', '../assets/hero-illustration.svg')
    'docs\i18n\OUTREACH.pt-BR.md' = @('../../assets/i18n/pt-BR/linkedin-illustration.svg', '../../assets/i18n/pt-BR/hero-illustration.svg')
    'docs\i18n\OUTREACH.es-ES.md' = @('../../assets/i18n/es-ES/linkedin-illustration.svg', '../../assets/i18n/es-ES/hero-illustration.svg')
    'docs\i18n\OUTREACH.ru-RU.md' = @('../../assets/i18n/ru-RU/linkedin-illustration.svg', '../../assets/i18n/ru-RU/hero-illustration.svg')
    'docs\i18n\OUTREACH.zh-CN.md' = @('../../assets/i18n/zh-CN/linkedin-illustration.svg', '../../assets/i18n/zh-CN/hero-illustration.svg')
}
foreach ($path in $outreachAssetPaths.Keys) {
    $document = [IO.File]::ReadAllText((Join-Path $root $path), $utf8)
    foreach ($assetPath in $outreachAssetPaths[$path]) {
        if (-not $document.Contains($assetPath)) {
            throw "Outreach documentation is missing its localized asset path: $path / $assetPath"
        }
    }
}

Write-Host 'Auditing localized concept-art accessibility metadata...'
$artLanguages = @{
    'assets\hero-illustration.svg' = 'en-US'
    'assets\linkedin-illustration.svg' = 'en-US'
    'assets\i18n\pt-BR\hero-illustration.svg' = 'pt-BR'
    'assets\i18n\pt-BR\linkedin-illustration.svg' = 'pt-BR'
    'assets\i18n\es-ES\hero-illustration.svg' = 'es-ES'
    'assets\i18n\es-ES\linkedin-illustration.svg' = 'es-ES'
    'assets\i18n\ru-RU\hero-illustration.svg' = 'ru-RU'
    'assets\i18n\ru-RU\linkedin-illustration.svg' = 'ru-RU'
    'assets\i18n\zh-CN\hero-illustration.svg' = 'zh-CN'
    'assets\i18n\zh-CN\linkedin-illustration.svg' = 'zh-CN'
}
$englishArt = @{}
foreach ($name in @('hero-illustration.svg','linkedin-illustration.svg')) {
    $text = [IO.File]::ReadAllText((Join-Path $root ('assets\' + $name)), $utf8)
    $titleMatch = [regex]::Match($text, '<title id="title">([^<]+)</title>')
    $descMatch = [regex]::Match($text, '<desc id="desc">([^<]+)</desc>')
    if (-not $titleMatch.Success -or -not $descMatch.Success) { throw "Missing English SVG accessibility metadata: $name" }
    $englishArt[$name] = @($titleMatch.Groups[1].Value, $descMatch.Groups[1].Value)
}
foreach ($path in $artLanguages.Keys) {
    $text = [IO.File]::ReadAllText((Join-Path $root $path), $utf8)
    $language = $artLanguages[$path]
    if (-not $text.Contains(('xml:lang="' + $language + '"'))) { throw "SVG language metadata mismatch: $path" }
    if (-not $text.Contains('aria-labelledby="title desc"')) { throw "SVG accessibility relationship missing: $path" }
    if ([regex]::Matches($text, 'CONCEPT_ART_NOT_SCREENSHOT').Count -ne 1) { throw "SVG concept-art marker count mismatch: $path" }
    $titleMatch = [regex]::Match($text, '<title id="title">([^<]+)</title>')
    $descMatch = [regex]::Match($text, '<desc id="desc">([^<]+)</desc>')
    if (-not $titleMatch.Success -or -not $descMatch.Success) { throw "SVG accessibility text missing: $path" }
    if ($language -ne 'en-US') {
        $name = Split-Path -Leaf $path
        if ($titleMatch.Groups[1].Value -eq $englishArt[$name][0] -or $descMatch.Groups[1].Value -eq $englishArt[$name][1]) {
            throw "Localized SVG accessibility metadata still matches English: $path"
        }
    }
}

Write-Host 'Auditing localized GitHub issue forms...'
$issueForms = @(
    '.github\ISSUE_TEMPLATE\bug_report.yml',
    '.github\ISSUE_TEMPLATE\bug_report.pt-BR.yml',
    '.github\ISSUE_TEMPLATE\bug_report.es-ES.yml',
    '.github\ISSUE_TEMPLATE\bug_report.ru-RU.yml',
    '.github\ISSUE_TEMPLATE\bug_report.zh-CN.yml'
)
foreach ($path in $issueForms) {
    $form = [IO.File]::ReadAllText((Join-Path $root $path), $utf8)
    foreach ($token in @('WinSidebar 2.0','Windows 11','Windows 10','id: language','id: privacy','title: "[Bug] "')) {
        if (-not $form.Contains($token)) {
            throw "Localized issue form is missing a required structural/product token: $path / $token"
        }
    }
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

Write-Host 'Auditing concept-art default shortcut labels against runtime catalogs...'
$additionalLocales = @{}
foreach ($code in @('es-ES','ru-RU','zh-CN')) {
    $additionalLocales[$code] = [IO.File]::ReadAllText((Join-Path $root ('i18n\' + $code + '.json')), $utf8) | ConvertFrom-Json
}
function Get-CatalogValue([string]$code, [string]$key) {
    if ($code -eq 'en-US' -or $code -eq 'pt-BR') {
        $entry = $base.PSObject.Properties[$key].Value
        return $entry.PSObject.Properties[$code].Value
    }
    return $additionalLocales[$code].PSObject.Properties[$key].Value
}
$artByLocale = @{
    'en-US' = @('assets\hero-illustration.svg','assets\linkedin-illustration.svg')
    'pt-BR' = @('assets\i18n\pt-BR\hero-illustration.svg','assets\i18n\pt-BR\linkedin-illustration.svg')
    'es-ES' = @('assets\i18n\es-ES\hero-illustration.svg','assets\i18n\es-ES\linkedin-illustration.svg')
    'ru-RU' = @('assets\i18n\ru-RU\hero-illustration.svg','assets\i18n\ru-RU\linkedin-illustration.svg')
    'zh-CN' = @('assets\i18n\zh-CN\hero-illustration.svg','assets\i18n\zh-CN\linkedin-illustration.svg')
}
foreach ($code in $artByLocale.Keys) {
    foreach ($path in $artByLocale[$code]) {
        $svg = [IO.File]::ReadAllText((Join-Path $root $path), $utf8)
        foreach ($key in @('defaults.documents','defaults.downloads','defaults.archive','defaults.website')) {
            $label = Get-CatalogValue $code $key
            if (-not $svg.Contains(('>' + $label + '</text>'))) {
                throw "Concept artwork default shortcut label mismatch: $path / $key"
            }
        }
        $shortcutTitle = (Get-CatalogValue $code 'sidebar.shortcuts').Trim()
        if (-not $svg.Contains(('>' + $shortcutTitle + '<'))) {
            throw "Concept artwork shortcut heading mismatch: $path"
        }
        if (-not $svg.Contains('AltGr+Y')) {
            throw "Concept artwork is missing the current sidebar hotkey: $path"
        }
    }
}

$sourceText = (Get-ChildItem -LiteralPath (Join-Path $root 'src') -Filter '*.cs' -File | ForEach-Object {
    [IO.File]::ReadAllText($_.FullName, $utf8)
}) -join [Environment]::NewLine
foreach ($pattern in @(
    'C:\\Users\\',
    '[A-Z]:\\(?:Git|H)\\',
    'HAMILTON_NAODELETAR',
    'C:\\dotnet8',
    '[\w.+-]+@[\w.-]+\.[A-Za-z]{2,}'
)) {
    if ($sourceText -match $pattern) { throw "Possible personal/local development reference in source: $pattern" }
}
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

if ([IO.Path]::IsPathRooted($OutputRoot)) {
    $output = $OutputRoot
}
else {
    $output = Join-Path $root $OutputRoot
}
$finalExpected = @(
    'LICENSE',
    'SHA256SUMS.txt',
    'START-HERE.es-ES.txt',
    'START-HERE.pt-BR.txt',
    'START-HERE.ru-RU.txt',
    'START-HERE.txt',
    'START-HERE.zh-CN.txt',
    'WinSidebar-v2.0-win-x64.zip',
    'WinSidebar.exe'
)
$transientNames = @('publish','staging','dist')
if (-not (Test-Path -LiteralPath $output)) {
    New-Item -ItemType Directory -Path $output -Force | Out-Null
}
$unexpectedExisting = @(Get-ChildItem -LiteralPath $output -Force | Where-Object {
    $_.Name -notin ($finalExpected + $transientNames)
})
if ($unexpectedExisting.Count -ne 0) {
    throw "OutputRoot contains unrelated content. Use an empty/dedicated release folder: $($unexpectedExisting.Name -join ', ')"
}
foreach ($name in $finalExpected) {
    $old = Join-Path $output $name
    if (Test-Path -LiteralPath $old -PathType Leaf) { Remove-Item -LiteralPath $old -Force }
}
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

# Build folders are implementation details. Flatten the successful result so the
# release directory itself is immediately understandable to a non-technical user.
Get-ChildItem -LiteralPath $staging -File | ForEach-Object {
    Copy-Item -LiteralPath $_.FullName -Destination (Join-Path $output $_.Name) -Force
}
$finalZip = Join-Path $output 'WinSidebar-v2.0-win-x64.zip'
$finalSum = Join-Path $output 'SHA256SUMS.txt'
Copy-Item -LiteralPath $zip -Destination $finalZip -Force
Copy-Item -LiteralPath $sum -Destination $finalSum -Force

foreach ($dir in @($publish,$staging,$dist)) {
    if (Test-Path -LiteralPath $dir) { Remove-Item -LiteralPath $dir -Recurse -Force }
}

$finalEntries = @(Get-ChildItem -LiteralPath $output -Force)
$finalDirectories = @($finalEntries | Where-Object { $_.PSIsContainer })
if ($finalDirectories.Count -ne 0) {
    throw "Final release folder must be flat. Unexpected directories: $($finalDirectories.Name -join ', ')"
}
$finalFiles = @($finalEntries | Where-Object { -not $_.PSIsContainer } |
    ForEach-Object Name | Sort-Object)
$expectedFinalFiles = @($finalExpected | Sort-Object)
if (($finalFiles -join '|') -ne ($expectedFinalFiles -join '|')) {
    throw "Unexpected final release folder contents: $($finalFiles -join ', ')"
}

$finalExe = Join-Path $output 'WinSidebar.exe'

Write-Host ''
Write-Host 'WINSIDEBAR 2.0 RELEASE VERIFICATION: PASS'
Write-Host "SDK: $sdkVersion"
Write-Host "OUTPUT: $output"
Write-Host "EXE: $finalExe"
Write-Host "ZIP: $finalZip"
Write-Host "SHA256: $finalSum"
