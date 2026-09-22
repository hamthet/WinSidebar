# Development-only, one-shot patch for UI feedback. Run locally in Windows PowerShell.
# No GitHub Actions, FILEBRIDGE, administrator rights, CMD, or global PATH changes.
[CmdletBinding()]
param([switch]$Push)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$branchName = 'feature/runtime-i18n-ru'
$expectedSourceBlob = '8dde9112a7187427595c2671ca5b07f16155bcca'
$dotnetRoot = Join-Path $env:USERPROFILE 'Desktop\HAMILTON_NAODELETAR\dotnet'
$dotnetExe = Join-Path $dotnetRoot 'dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnetExe)) { throw "Portable .NET SDK missing: $dotnetExe" }
$env:DOTNET_ROOT = $dotnetRoot
$env:PATH = "$dotnetRoot;$env:PATH"

$branch = (git branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $branch -ne $branchName) { throw "Expected branch $branchName; got $branch" }
$blob = (git rev-parse 'HEAD:src/WinSidebar.cs').Trim()
if ($LASTEXITCODE -ne 0 -or $blob -ne $expectedSourceBlob) {
    throw "WinSidebar.cs baseline differs ($blob). Do not reapply; inspect the branch before changing code."
}
$changes = @(git status --porcelain)
if ($LASTEXITCODE -ne 0 -or $changes.Count -ne 0) {
    throw 'Working tree is not clean. Preserve or commit existing changes before running this one-shot patch.'
}
$path = Join-Path (Get-Location) 'src\WinSidebar.cs'
if (-not (Test-Path -LiteralPath $path)) { throw 'Run this script from the WinSidebar repository root.' }
$script:source = [IO.File]::ReadAllText($path)
function Replace-Unique([string]$old, [string]$replacement) {
    $count = [regex]::Matches($script:source, [regex]::Escape($old)).Count
    if ($count -ne 1) { throw "Expected one exact anchor; found $count : $old" }
    $script:source = $script:source.Replace($old, $replacement)
}

# The shortcut editor already has its own necessary Save/Cancel confirmation.
# All successfully confirmed settings are saved by their corresponding actions.
Replace-Unique '    private readonly Button savePreferencesButton = new Button();' '    private readonly Button languageButton = new Button();'
Replace-Unique '        ConfigureShortcutHeaderButton(savePreferencesButton, "S", Localization.Text("sidebar.save_preferences"), delegate { SavePreferences(); });' @'
        ConfigureShortcutHeaderButton(languageButton, "\U0001F310", Localization.Text("sidebar.language"),
            delegate { languageMenu.DropDown.Show(languageButton, new Point(0, languageButton.Height)); });
'@.TrimEnd("`r", "`n")
Replace-Unique '        savePreferencesButton.AccessibleName = Localization.Text("sidebar.save_preferences");' '        languageButton.AccessibleName = Localization.Text("sidebar.language");'
Replace-Unique '        tips.SetToolTip(savePreferencesButton, Localization.Text("sidebar.save_preferences"));' '        tips.SetToolTip(languageButton, Localization.Text("sidebar.language"));'

# Remove only the unused, redundant whole-form save handler, not the shortcut editor's Save.
$removeSave = '(?m)^    private void SavePreferences\(\)\r?\n    \{\r?\n.*?^    \}\r?\n(?=\r?\n    private void RestoreDefaults\(\))'
$matches = [regex]::Matches($script:source, $removeSave, [Text.RegularExpressions.RegexOptions]::Singleline)
if ($matches.Count -ne 1) { throw "Expected exactly one redundant SavePreferences method, found $($matches.Count)." }
$script:source = [regex]::Replace($script:source, $removeSave, '', [Text.RegularExpressions.RegexOptions]::Singleline)

# Sidebar is TopMost and sits at the screen edge; CenterParent can place the
# modal under the sidebar and partially off screen. Locate it within the
# monitor working area, beside the sidebar when space permits, and above it.
Replace-Unique '            if (editor.ShowDialog(this) != DialogResult.OK) return;' @'
            Rectangle workArea = Screen.FromControl(this).WorkingArea;
            int preferredX = leftSide ? Right + 12 : Left - editor.Width - 12;
            int editorX = Math.Max(workArea.Left, Math.Min(preferredX, Math.Max(workArea.Left, workArea.Right - editor.Width)));
            int preferredY = Top + (Height - editor.Height) / 2;
            int editorY = Math.Max(workArea.Top, Math.Min(preferredY, Math.Max(workArea.Top, workArea.Bottom - editor.Height)));
            editor.StartPosition = FormStartPosition.Manual;
            editor.Location = new Point(editorX, editorY);
            editor.TopMost = true;
            if (editor.ShowDialog(this) != DialogResult.OK) return;
'@.TrimEnd("`r", "`n")

if ($script:source.Contains('savePreferencesButton') -or $script:source.Contains('SavePreferences()')) {
    throw 'Unexpected remaining reference to removed button or handler.'
}
if (-not $script:source.Contains('languageMenu.DropDown.Show(languageButton') -or
    -not $script:source.Contains('editor.TopMost = true;')) {
    throw 'Expected UI changes missing from patch.'
}
[IO.File]::WriteAllText($path, $script:source, [Text.UTF8Encoding]::new($false))
git diff --check -- src/WinSidebar.cs
if ($LASTEXITCODE -ne 0) { throw 'Whitespace/conflict issue; no commit or push performed.' }
Write-Host 'Applied language button, removed redundant Save, repositioned/topmost shortcut editor.'
Write-Host 'Running executable language checks and full Windows compilation on this machine...'
& $dotnetExe run --project tests/LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Localization smoke checks failed; no commit or push performed.' }
& $dotnetExe build WinSidebar.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Windows build failed; no commit or push performed.' }
Write-Host 'Local smoke checks and build passed. GUI stacking and language-button behavior require your manual Windows test.'

if ($Push) {
    gh auth status
    if ($LASTEXITCODE -ne 0) { throw 'GitHub CLI not authenticated; changes remain local.' }
    git add -- src/WinSidebar.cs
    if ($LASTEXITCODE -ne 0) { throw 'Staging failed.' }
    git commit -m 'fix(ui): expose language picker and keep shortcut editor above sidebar'
    if ($LASTEXITCODE -ne 0) { throw 'Commit failed.' }
    git push origin $branchName
    if ($LASTEXITCODE -ne 0) { throw 'Push failed.' }
    Write-Host 'Source pushed to WinSidebar only; FILEBRIDGE not used.'
}
