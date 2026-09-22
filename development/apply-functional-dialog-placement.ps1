# Development-only one-shot patch. Execute in Windows PowerShell from C:\git\WinSidebar.
# No administrator, CMD, GitHub Actions, FILEBRIDGE or global PATH changes.
[CmdletBinding()]
param([switch]$Push)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$expectedBranch = 'feature/functional-dialog-placement'
$expectedSourceBlob = '83f706aa3eff304d9fc9268c651e928eb31a2825'
$repo = (git rev-parse --show-toplevel).Trim().Replace('/', '\').TrimEnd('\')
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repo)) { throw 'Not inside a Git repository.' }
if (-not [string]::Equals($repo, (Get-Location).Path.TrimEnd('\'), [StringComparison]::OrdinalIgnoreCase)) {
    throw "Run from repository root: $repo"
}
$branch = (git branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $branch -ne $expectedBranch) { throw "Expected $expectedBranch; found $branch" }
$blob = (git rev-parse 'HEAD:src/WindowManagement.cs').Trim()
if ($LASTEXITCODE -ne 0 -or $blob -ne $expectedSourceBlob) {
    throw "WindowManagement.cs baseline changed ($blob); do not reapply."
}
$changes = @(git status --porcelain)
if ($LASTEXITCODE -ne 0 -or $changes.Count -ne 0) { throw 'Working tree is not clean; preserve changes.' }

$sdkRoot = Join-Path $env:USERPROFILE 'Desktop\HAMILTON_NAODELETAR\dotnet'
$dotnet = Join-Path $sdkRoot 'dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) { throw "Portable SDK not found: $dotnet" }
$env:DOTNET_ROOT = $sdkRoot
$env:PATH = "$sdkRoot;$env:PATH"
$path = Join-Path $repo 'src\WindowManagement.cs'
$script:source = [IO.File]::ReadAllText($path)
function Replace-Unique([string]$old, [string]$replacement) {
    $count = [regex]::Matches($script:source, [regex]::Escape($old)).Count
    if ($count -ne 1) { throw "Expected one anchor, found ${count}: $old" }
    $script:source = $script:source.Replace($old, $replacement)
}

# Window-management modal dialogs still use CenterParent even though the
# sidebar is TopMost and positioned on the edge of the monitor.
$helper = @'
    // Keep dialogs next to the sidebar, inside its monitor's working area.
    // Modal ownership is preserved; never move the user's other windows.
    private static void PlaceModalBesideSidebar(Form owner, Form dialog)
    {
        Rectangle work = Screen.FromControl(owner).WorkingArea;
        bool barOnLeft = owner.Left + owner.Width / 2 <= work.Left + work.Width / 2;
        int preferredX = barOnLeft ? owner.Right + 12 : owner.Left - dialog.Width - 12;
        int maxX = Math.Max(work.Left, work.Right - dialog.Width);
        int preferredY = owner.Top + (owner.Height - dialog.Height) / 2;
        int maxY = Math.Max(work.Top, work.Bottom - dialog.Height);
        dialog.StartPosition = FormStartPosition.Manual;
        dialog.Location = new Point(Math.Max(work.Left, Math.Min(preferredX, maxX)),
            Math.Max(work.Top, Math.Min(preferredY, maxY)));
        dialog.TopMost = owner.TopMost;
    }

'@
Replace-Unique '    private void Rename(Form owner, IntPtr hwnd, Action changed)' ($helper + '    private void Rename(Form owner, IntPtr hwnd, Action changed)')
Replace-Unique '            if (dialog.ShowDialog(owner) != DialogResult.OK) return;' @'
            PlaceModalBesideSidebar(owner, dialog);
            if (dialog.ShowDialog(owner) != DialogResult.OK) return;
'@.TrimEnd("`r", "`n")
Replace-Unique '            dialog.ShowDialog(owner);' @'
            PlaceModalBesideSidebar(owner, dialog);
            dialog.ShowDialog(owner);
'@.TrimEnd("`r", "`n")
if ([regex]::Matches($script:source, 'PlaceModalBesideSidebar\(owner, dialog\);').Count -ne 2) {
    throw 'Expected both rename and ignored-list dialogs to be positioned.'
}
[IO.File]::WriteAllText($path, $script:source, [Text.UTF8Encoding]::new($false))
git diff --check -- src/WindowManagement.cs
if ($LASTEXITCODE -ne 0) { throw 'Whitespace/conflict issue; no commit or push.' }
Write-Host 'Patched both window-management dialogs. Running localization smoke and Windows build...'
& $dotnet run --project tests/LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Localization smoke failed; patch remains local. Do not rerun script.' }
& $dotnet build WinSidebar.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Windows build failed; patch remains local. Do not rerun script.' }
Write-Host 'Static smoke/build completed. Foreground and screen placement still require a live GUI check.'

if ($Push) {
    gh auth status
    if ($LASTEXITCODE -ne 0) { throw 'GitHub CLI not authenticated; source remains local.' }
    git add -- src/WindowManagement.cs
    if ($LASTEXITCODE -ne 0) { throw 'Staging failed.' }
    git commit -m 'fix(ui): keep rename and ignored-app dialogs above sidebar'
    if ($LASTEXITCODE -ne 0) { throw 'Commit failed.' }
    git push -u origin $expectedBranch
    if ($LASTEXITCODE -ne 0) { throw 'Push failed.' }
    Write-Host 'Pushed source to WinSidebar; no Actions or FILEBRIDGE used.'
}
