# Development-only one-shot source integration on Windows PowerShell without admin.
# WinSidebar repository only. No GitHub Actions and no FILEBRIDGE transfer.
# Must be run from C:\git\WinSidebar with portable .NET 8 SDK installed.
[CmdletBinding()]
param([switch]$Push)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$expectedBranch = 'feature/runtime-i18n-zh'
$expectedBlob = 'f278ea40c1aab18382e01fb3fdca27265672fa9d'
$repo = (git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repo)) { throw 'Not inside a Git repository.' }
$repo = $repo.Replace('/', '\').TrimEnd('\')
$here = (Get-Location).Path.TrimEnd('\')
if (-not [string]::Equals($repo, $here, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Run from the repository root: $repo"
}
$branch = (git branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $branch -ne $expectedBranch) { throw "Expected branch $expectedBranch; found $branch" }
$blob = (git rev-parse 'HEAD:src/WinSidebar.cs').Trim()
if ($LASTEXITCODE -ne 0 -or $blob -ne $expectedBlob) {
    throw "Main UI baseline changed ($blob). Inspect the changes; do not reapply the patch."
}
$changes = @(git status --porcelain)
if ($LASTEXITCODE -ne 0 -or $changes.Count -ne 0) {
    throw 'Working tree is not clean. Preserve your local changes and inspect them first.'
}

$dotnetRoot = Join-Path $env:USERPROFILE 'Desktop\HAMILTON_NAODELETAR\dotnet'
$dotnet = Join-Path $dotnetRoot 'dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) {
    throw "Portable .NET 8 SDK not found: $dotnet"
}
$env:DOTNET_ROOT = $dotnetRoot
$env:PATH = "$dotnetRoot;$env:PATH"

# Windows PowerShell 5.1 compatible parity validation; no Python installation.
$base = Get-Content -LiteralPath 'i18n/catalog.json' -Raw -Encoding UTF8 | ConvertFrom-Json
$expected = @($base.PSObject.Properties.Name)
if ($expected.Count -lt 100) { throw "Unexpected base catalog size: $($expected.Count)" }
foreach ($code in @('es-ES','ru-RU','zh-CN')) {
    $catalog = Get-Content -LiteralPath "i18n/$code.json" -Raw -Encoding UTF8 | ConvertFrom-Json
    $actual = @($catalog.PSObject.Properties.Name)
    if ($actual.Count -ne $expected.Count) { throw "Wrong key count in $code" }
    foreach ($key in $expected) {
        if ($actual -cnotcontains $key) { throw "Missing key in ${code}: $key" }
        $text = $catalog.PSObject.Properties[$key].Value
        if (-not ($text -is [string]) -or [string]::IsNullOrWhiteSpace($text)) {
            throw "Empty/invalid translation in ${code}: $key"
        }
    }
}
Write-Host "Catalog keys verified: $($expected.Count) in all five languages (static parity)."

$path = Join-Path $repo 'src\WinSidebar.cs'
$script:source = [IO.File]::ReadAllText($path)
function Replace-Unique([string]$old, [string]$replacement) {
    $count = [regex]::Matches($script:source, [regex]::Escape($old)).Count
    if ($count -ne 1) { throw "Expected exactly one anchor; found ${count}: $old" }
    $script:source = $script:source.Replace($old, $replacement)
}

# Preserve the existing visible language button, tray submenu, shortcut editor,
# and persisted shortcut/icon/process IDs. Add only the fifth language option.
Replace-Unique '    private ToolStripMenuItem russianItem;' @'
    private ToolStripMenuItem russianItem;
    private ToolStripMenuItem chineseItem;
'@.TrimEnd("`r", "`n")
Replace-Unique '        russianItem = new ToolStripMenuItem("\u0420\u0443\u0441\u0441\u043A\u0438\u0439");' @'
        russianItem = new ToolStripMenuItem("\u0420\u0443\u0441\u0441\u043A\u0438\u0439");
        chineseItem = new ToolStripMenuItem("简体中文");
'@.TrimEnd("`r", "`n")
Replace-Unique '        russianItem.Click += delegate { ChangeLanguage("ru-RU"); };' @'
        russianItem.Click += delegate { ChangeLanguage("ru-RU"); };
        chineseItem.Click += delegate { ChangeLanguage("zh-CN"); };
'@.TrimEnd("`r", "`n")
Replace-Unique '        languageMenu.DropDownItems.Add(russianItem);' @'
        languageMenu.DropDownItems.Add(russianItem);
        languageMenu.DropDownItems.Add(chineseItem);
'@.TrimEnd("`r", "`n")
Replace-Unique '        russianItem.Checked = Localization.Current == "ru-RU";' @'
        russianItem.Checked = Localization.Current == "ru-RU";
        chineseItem.Checked = Localization.Current == "zh-CN";
'@.TrimEnd("`r", "`n")

[IO.File]::WriteAllText($path, $script:source, [Text.UTF8Encoding]::new($false))
git diff --check -- src/WinSidebar.cs
if ($LASTEXITCODE -ne 0) { throw 'Whitespace issue in the UI patch. No commit or push performed.' }
Write-Host 'Chinese menu option added locally; now running the real .NET smoke tests and build.'
& $dotnet run --project tests/LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Five-language smoke tests failed; source remains local and is NOT committed.' }
& $dotnet build WinSidebar.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Windows build failed; source remains local and is NOT committed.' }
Write-Host 'Local smoke tests and Windows build passed. Live GUI, font rendering and persistence still require owner testing.'

if ($Push) {
    gh auth status
    if ($LASTEXITCODE -ne 0) { throw 'GitHub CLI not authenticated. Source remains local.' }
    git add -- src/WinSidebar.cs
    if ($LASTEXITCODE -ne 0) { throw 'Could not stage the Chinese selector.' }
    git commit -m 'feat(i18n): expose Simplified Chinese in language menu'
    if ($LASTEXITCODE -ne 0) { throw 'Could not commit the Chinese selector.' }
    git push -u origin $expectedBranch
    if ($LASTEXITCODE -ne 0) { throw 'Could not push the Chinese selector.' }
    Write-Host 'Chinese selector pushed to WinSidebar only; no Actions or FILEBRIDGE used.'
}
