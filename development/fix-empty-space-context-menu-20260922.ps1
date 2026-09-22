# Development-only Windows PowerShell 5.1 patch; run once from C:\git\WinSidebar.
# No administrator, GitHub Actions, FILEBRIDGE or global SDK installation.
[CmdletBinding()]
param([switch]$Push)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$expectedBranch = 'feature/functional-dialog-placement'
$expectedBlob = 'b2cdd8952fb2f9e2ec0257022bd0e5af21fdead7'
$root = (git rev-parse --show-toplevel).Trim().Replace('/', '\').TrimEnd('\')
if ($LASTEXITCODE -ne 0 -or -not [string]::Equals($root, (Get-Location).Path.TrimEnd('\'), [StringComparison]::OrdinalIgnoreCase)) { throw 'Run from the WinSidebar repository root.' }
$branch = (git branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $branch -ne $expectedBranch) { throw "Unexpected branch: $branch" }
$blob = (git rev-parse 'HEAD:src/WinSidebar.cs').Trim()
if ($LASTEXITCODE -ne 0 -or $blob -ne $expectedBlob) { throw "Source baseline differs ($blob); do not reapply." }
$changes = @(git status --porcelain)
if ($LASTEXITCODE -ne 0 -or $changes.Count -ne 0) { throw 'Working tree not clean; preserve changes.' }
$dotnetRoot = Join-Path $env:USERPROFILE 'Desktop\HAMILTON_NAODELETAR\dotnet'
$dotnet = Join-Path $dotnetRoot 'dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) { throw "Portable .NET SDK missing: $dotnet" }
$env:DOTNET_ROOT = $dotnetRoot
$env:PATH = "$dotnetRoot;$env:PATH"
$path = Join-Path $root 'src\WinSidebar.cs'
$source = [IO.File]::ReadAllText($path)
$lineEnding = if ($source.Contains("`r`n")) { "`r`n" } else { "`n" }
$old = @'
            TreeNode node = tree.GetNodeAt(clicked);
            if (node == null || !(node.Tag is IntPtr)) return;
            tree.SelectedNode = node;
'@
$new = @'
            TreeNode node = tree.GetNodeAt(clicked);
            if (node == null || !(node.Tag is IntPtr)) {
                // The tree router canceled the native menu. For an empty area or
                // monitor heading, display the existing global menu after WM_CONTEXTMENU.
                tree.BeginInvoke((MethodInvoker)delegate {
                    if (IsDisposed || tree.IsDisposed) return;
                    ContextMenuStrip generalMenu = content.ContextMenuStrip;
                    if (generalMenu != null && !generalMenu.IsDisposed)
                        generalMenu.Show(tree, clicked);
                });
                return;
            }
            tree.SelectedNode = node;
'@
$old = $old.TrimEnd("`r", "`n").Replace("`n", $lineEnding)
$new = $new.TrimEnd("`r", "`n").Replace("`n", $lineEnding)
$count = [regex]::Matches($source, [regex]::Escape($old)).Count
if ($count -ne 1) { throw "Expected one blank-tree router anchor; found $count. Nothing changed." }
$source = $source.Replace($old, $new)
[IO.File]::WriteAllText($path, $source, (New-Object System.Text.UTF8Encoding($false)))
git diff --check -- src/WinSidebar.cs
if ($LASTEXITCODE -ne 0) { throw 'Whitespace verification failed; patch remains local. Do not rerun.' }
Write-Host 'General menu now routed from empty tree space. Testing five locales...'
& $dotnet run --project tests/LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Smoke failed; do not rerun the patch. Send this output.' }
$output = Join-Path $env:TEMP ('WinSidebar-empty-menu-' + [guid]::NewGuid().ToString('N'))
& $dotnet publish WinSidebar.csproj -c Release -r win-x64 --self-contained true -o $output
if ($LASTEXITCODE -ne 0) { throw 'Publish failed; patch remains local. Send this output.' }
$exe = Join-Path $output 'WinSidebar.exe'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) { throw "No preview executable: $exe" }
Write-Host "Preview: $exe"
if ($Push) {
    gh auth status
    if ($LASTEXITCODE -ne 0) { throw 'GitHub CLI is not authenticated; tested source remains local.' }
    git add -- src/WinSidebar.cs
    if ($LASTEXITCODE -ne 0) { throw 'Unable to stage source.' }
    git commit -m 'fix(ui): restore global menu on blank sidebar tree areas'
    if ($LASTEXITCODE -ne 0) { throw 'Unable to commit source.' }
    git push -u origin $expectedBranch
    if ($LASTEXITCODE -ne 0) { throw 'Unable to push source.' }
}
if (Get-Process WinSidebar -ErrorAction SilentlyContinue) {
    Write-Warning "Old WinSidebar instance running. Exit from tray then open preview: Start-Process -FilePath '$exe'"
} else {
    Write-Host "Opening preview: $exe"
    Start-Process -FilePath $exe
}
