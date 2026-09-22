# Development-only local integration. Remove from shipping branch after application.
# Run in the root of hamthet/WinSidebar on branch feature/runtime-i18n-ru.
# This does not use GitHub Actions, FILEBRIDGE, or any paid service.
[CmdletBinding()]
param([switch]$Push)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$expectedBranch = 'feature/runtime-i18n-ru'
$expectedBlob = 'e613588dc597c778010c0332904a2d18d0225034'
$branch = (git branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $branch -ne $expectedBranch) {
    throw "Required branch is $expectedBranch; current branch is $branch"
}
$blob = (git rev-parse 'HEAD:src/WinSidebar.cs').Trim()
if ($LASTEXITCODE -ne 0 -or $blob -ne $expectedBlob) {
    throw "WinSidebar.cs baseline changed ($blob). Review diff; do not apply this script blindly."
}
$dirty = @(git status --porcelain -- 'src/WinSidebar.cs')
if ($LASTEXITCODE -ne 0 -or $dirty.Count -ne 0) {
    throw 'WinSidebar.cs contains local changes. Preserve them before applying this patch.'
}
$path = Join-Path (Get-Location) 'src/WinSidebar.cs'
$script:text = [IO.File]::ReadAllText($path)
function Replace-Unique([string]$old, [string]$new) {
    $count = [Text.RegularExpressions.Regex]::Matches($script:text, [Text.RegularExpressions.Regex]::Escape($old)).Count
    if ($count -ne 1) { throw "Expected exactly one occurrence ($count): $old" }
    $script:text = $script:text.Replace($old, $new)
}

Replace-Unique '    private ToolStripMenuItem spanishItem;' @'
    private ToolStripMenuItem spanishItem;
    private ToolStripMenuItem russianItem;
'@.TrimEnd("`r", "`n")
Replace-Unique '        spanishItem = new ToolStripMenuItem("Español");' @'
        spanishItem = new ToolStripMenuItem("Español");
        russianItem = new ToolStripMenuItem("Русский");
'@.TrimEnd("`r", "`n")
Replace-Unique '        spanishItem.Click += delegate { ChangeLanguage("es-ES"); };' @'
        spanishItem.Click += delegate { ChangeLanguage("es-ES"); };
        russianItem.Click += delegate { ChangeLanguage("ru-RU"); };
'@.TrimEnd("`r", "`n")
Replace-Unique '        languageMenu.DropDownItems.Add(spanishItem);' @'
        languageMenu.DropDownItems.Add(spanishItem);
        languageMenu.DropDownItems.Add(russianItem);
'@.TrimEnd("`r", "`n")
Replace-Unique '        spanishItem.Checked = Localization.Current == "es-ES";' @'
        spanishItem.Checked = Localization.Current == "es-ES";
        russianItem.Checked = Localization.Current == "ru-RU";
'@.TrimEnd("`r", "`n")

[IO.File]::WriteAllText($path, $script:text, [Text.UTF8Encoding]::new($false))
git diff --check -- src/WinSidebar.cs
if ($LASTEXITCODE -ne 0) { throw 'Whitespace errors in Russian selector patch.' }
Write-Host 'Applied anchored Russian language menu patch. Running actual local tests.'
dotnet run --project tests/LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Russian localization smoke tests failed. Changes were not committed or pushed.' }
dotnet build WinSidebar.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'WinSidebar Windows compilation failed. Changes were not committed or pushed.' }
Write-Host 'Local tests and Windows compile passed. Interactive WinForms behavior is still NOT TESTED.'

if ($Push) {
    gh auth status
    if ($LASTEXITCODE -ne 0) { throw 'GitHub CLI not authenticated; no push performed.' }
    git add -- src/WinSidebar.cs
    if ($LASTEXITCODE -ne 0) { throw 'Could not stage Russian selector.' }
    git commit -m 'feat(i18n): expose Russian in sidebar and tray language menu'
    if ($LASTEXITCODE -ne 0) { throw 'Could not commit Russian selector.' }
    git push -u origin $expectedBranch
    if ($LASTEXITCODE -ne 0) { throw 'Could not push Russian selector.' }
    Write-Host 'Source pushed to WinSidebar feature branch. No FILEBRIDGE upload performed.'
}
