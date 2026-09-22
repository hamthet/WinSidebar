# Development-only one-shot patch for Windows PowerShell 5.1.
# Run from C:\git\WinSidebar. No GitHub Actions, FILEBRIDGE, admin or CMD.
[CmdletBinding()]
param([switch]$Push)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$branchName = 'feature/functional-dialog-placement'
$expectedBlob = '97c46b181bb1fa420dcc88d43be5a381fbd5254c'
$root = (git rev-parse --show-toplevel).Trim().Replace('/', '\').TrimEnd('\')
if ($LASTEXITCODE -ne 0 -or -not [string]::Equals($root, (Get-Location).Path.TrimEnd('\'), [StringComparison]::OrdinalIgnoreCase)) { throw 'Run from the WinSidebar repository root.' }
$branch = (git branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $branch -ne $branchName) { throw "Wrong branch: $branch" }
$blob = (git rev-parse 'HEAD:src/WinSidebar.cs').Trim()
if ($LASTEXITCODE -ne 0 -or $blob -ne $expectedBlob) { throw "UI source differs from reviewed baseline ($blob). Stop and inspect; do not reapply." }
$changes = @(git status --porcelain)
if ($LASTEXITCODE -ne 0 -or $changes.Count -ne 0) { throw 'Working tree not clean. Preserve local changes first.' }

$dotnetRoot = Join-Path $env:USERPROFILE 'Desktop\HAMILTON_NAODELETAR\dotnet'
$dotnet = Join-Path $dotnetRoot 'dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) { throw "Portable .NET SDK not found: $dotnet" }
$env:DOTNET_ROOT = $dotnetRoot
$env:PATH = "$dotnetRoot;$env:PATH"

$path = Join-Path $root 'src\WinSidebar.cs'
$script:code = [IO.File]::ReadAllText($path)
function Replace-Unique([string]$old, [string]$new) {
    $count = [regex]::Matches($script:code, [regex]::Escape($old)).Count
    if ($count -ne 1) { throw "Expected exactly one source anchor, found ${count}: $old" }
    $script:code = $script:code.Replace($old, $new)
}

$oldMouse = @'
        tree.NodeMouseClick += delegate(object sender, TreeNodeMouseClickEventArgs e) {
            if (e.Button == MouseButtons.Right && e.Node != null && e.Node.Tag is IntPtr) {
                tree.SelectedNode = e.Node;
                selectedHandle = (IntPtr)e.Node.Tag;
                windows.ShowWindowMenu(this, tree, selectedHandle, e.Location, delegate { RefreshWindows(true); });
            }
        };
'@
$newMouse = @'
        // Tree rows have their own context route. Otherwise WM_CONTEXTMENU can
        // bubble to content and replace the window actions with the global menu.
        // Do not show a drop-down while the native context message is in flight.
        tree.NodeMouseClick += delegate(object sender, TreeNodeMouseClickEventArgs e) {
            if (e.Button == MouseButtons.Right && e.Node != null && e.Node.Tag is IntPtr) {
                tree.SelectedNode = e.Node;
                selectedHandle = (IntPtr)e.Node.Tag;
            }
        };
        ContextMenuStrip windowContextRouter = new ContextMenuStrip();
        tree.ContextMenuStrip = windowContextRouter;
        windowContextRouter.Opening += delegate(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true; // Router is never itself shown, including on empty rows.
            Point clicked = tree.PointToClient(Cursor.Position);
            TreeNode node = tree.GetNodeAt(clicked);
            if (node == null || !(node.Tag is IntPtr)) return;
            tree.SelectedNode = node;
            IntPtr hwnd = (IntPtr)node.Tag;
            selectedHandle = hwnd;
            tree.BeginInvoke((MethodInvoker)delegate {
                if (IsDisposed || tree.IsDisposed || !Native.IsWindow(hwnd)) return;
                windows.ShowWindowMenu(this, tree, hwnd, clicked, delegate { RefreshWindows(true); });
            });
        };
'@
Replace-Unique $oldMouse.TrimEnd("`r", "`n") $newMouse.TrimEnd("`r", "`n")

$oldKeyboard = @'
                windows.ShowWindowMenu(this, tree, (IntPtr)selected.Tag,
                    new Point(selected.Bounds.Left + 12, selected.Bounds.Bottom), delegate { RefreshWindows(true); });
'@
$newKeyboard = @'
                IntPtr hwnd = (IntPtr)selected.Tag;
                Point position = new Point(selected.Bounds.Left + 12, selected.Bounds.Bottom);
                tree.BeginInvoke((MethodInvoker)delegate {
                    if (IsDisposed || tree.IsDisposed || !Native.IsWindow(hwnd)) return;
                    windows.ShowWindowMenu(this, tree, hwnd, position, delegate { RefreshWindows(true); });
                });
'@
Replace-Unique $oldKeyboard.TrimEnd("`r", "`n") $newKeyboard.TrimEnd("`r", "`n")
Replace-Unique '            tips.Dispose();' "            windowContextRouter.Dispose();`r`n            tips.Dispose();"

[IO.File]::WriteAllText($path, $script:code, (New-Object Text.UTF8Encoding($false)))
git diff --check -- src/WinSidebar.cs
if ($LASTEXITCODE -ne 0) { throw 'Whitespace validation failed. Source remains local; do not rerun the patch.' }
Write-Host 'Tree now has a dedicated, deferred window-context route. Running localization smoke...'
& $dotnet run --project tests/LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Localization smoke failed. Do not rerun the patch.' }
$output = Join-Path $env:TEMP ('WinSidebar-menu-test-' + [guid]::NewGuid().ToString('N'))
& $dotnet publish WinSidebar.csproj -c Release -r win-x64 --self-contained true -o $output
if ($LASTEXITCODE -ne 0) { throw 'Windows publish failed. Do not rerun the patch.' }
$exe = Join-Path $output 'WinSidebar.exe'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) { throw "EXE missing: $exe" }
Write-Host "Local build succeeded; preview: $exe"
if ($Push) {
    gh auth status
    if ($LASTEXITCODE -ne 0) { throw 'GitHub CLI not authenticated. Built source remains local.' }
    git add -- src/WinSidebar.cs
    if ($LASTEXITCODE -ne 0) { throw 'Could not stage source.' }
    git commit -m 'fix(ui): route window right-click to window commands'
    if ($LASTEXITCODE -ne 0) { throw 'Could not commit source.' }
    git push -u origin $branchName
    if ($LASTEXITCODE -ne 0) { throw 'Could not push source.' }
}
if (Get-Process WinSidebar -ErrorAction SilentlyContinue) {
    Write-Warning "An older WinSidebar process is still running. Exit it via the tray, then: Start-Process -FilePath '$exe'"
} else {
    Write-Host "Opening new preview: $exe"
    Start-Process -FilePath $exe
}
