# Development-only integration; never include this script in the shipping product.
# Run in the root of hamthet/WinSidebar on feature/runtime-i18n-ru.
# ASCII-only script: compatible with Windows PowerShell 5.1 and PowerShell 7.
# No Actions, FILEBRIDGE or remote build is called.
[CmdletBinding()]
param([switch]$Push)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$expectedBranch = 'feature/runtime-i18n-ru'
$expectedBlob = 'e613588dc597c778010c0332904a2d18d0225034'
$branch = (git branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $branch -ne $expectedBranch) {
    throw "Expected $expectedBranch, found $branch"
}
$blob = (git rev-parse 'HEAD:src/WinSidebar.cs').Trim()
if ($LASTEXITCODE -ne 0 -or $blob -ne $expectedBlob) {
    throw "WinSidebar.cs baseline changed ($blob); review before applying."
}
$dirty = @(git status --porcelain -- 'src/WinSidebar.cs')
if ($LASTEXITCODE -ne 0 -or $dirty.Count -ne 0) {
    throw 'WinSidebar.cs has local changes; preserve them first.'
}
$path = Join-Path (Get-Location) 'src/WinSidebar.cs'
$script:text = [IO.File]::ReadAllText($path)
$nl = if ($script:text.Contains("`r`n")) { "`r`n" } else { "`n" }
function Replace-Unique([string]$old, [string]$new) {
    $count = [Text.RegularExpressions.Regex]::Matches($script:text, [Text.RegularExpressions.Regex]::Escape($old)).Count
    if ($count -ne 1) { throw "Expected exactly one occurrence ($count): $old" }
    $script:text = $script:text.Replace($old, $new)
}

Replace-Unique '    private ToolStripMenuItem spanishItem;' ('    private ToolStripMenuItem spanishItem;' + $nl + '    private ToolStripMenuItem russianItem;')
# In C# a Unicode escape in a string literal produces the native Russian menu label.
Replace-Unique '        englishItem.Click += delegate { ChangeLanguage("en-US"); };' ('        russianItem = new ToolStripMenuItem("\u0420\u0443\u0441\u0441\u043A\u0438\u0439");' + $nl + '        englishItem.Click += delegate { ChangeLanguage("en-US"); };')
Replace-Unique '        spanishItem.Click += delegate { ChangeLanguage("es-ES"); };' ('        spanishItem.Click += delegate { ChangeLanguage("es-ES"); };' + $nl + '        russianItem.Click += delegate { ChangeLanguage("ru-RU"); };')
Replace-Unique '        languageMenu.DropDownItems.Add(spanishItem);' ('        languageMenu.DropDownItems.Add(spanishItem);' + $nl + '        languageMenu.DropDownItems.Add(russianItem);')
Replace-Unique '        spanishItem.Checked = Localization.Current == "es-ES";' ('        spanishItem.Checked = Localization.Current == "es-ES";' + $nl + '        russianItem.Checked = Localization.Current == "ru-RU";')

[IO.File]::WriteAllText($path, $script:text, (New-Object System.Text.UTF8Encoding($false)))
git diff --check -- src/WinSidebar.cs
if ($LASTEXITCODE -ne 0) { throw 'Whitespace errors in Russian selector patch.' }
Write-Host 'Russian menu patch applied; running local tests.'
dotnet run --project tests/LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Language smoke tests failed. No commit/push performed.' }
dotnet build WinSidebar.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Windows compilation failed. No commit/push performed.' }
Write-Host 'Local smoke and Windows compile passed; GUI/Alt+Tab tests remain unverified.'

if ($Push) {
    gh auth status
    if ($LASTEXITCODE -ne 0) { throw 'GitHub CLI is not authenticated.' }
    git add -- src/WinSidebar.cs
    if ($LASTEXITCODE -ne 0) { throw 'Could not stage Russian menu.' }
    git commit -m 'feat(i18n): expose Russian in sidebar and tray language menu'
    if ($LASTEXITCODE -ne 0) { throw 'Could not commit Russian menu.' }
    git push -u origin $expectedBranch
    if ($LASTEXITCODE -ne 0) { throw 'Could not push Russian menu.' }
    Write-Host 'Source pushed to WinSidebar; no FILEBRIDGE upload.'
}
