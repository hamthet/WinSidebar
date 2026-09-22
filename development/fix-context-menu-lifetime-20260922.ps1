# Development-only one-shot fix for a WinForms ContextMenuStrip disposed during WM_CONTEXTMENU.
# Run from the WinSidebar clone on Windows PowerShell; no Actions or FILEBRIDGE.
[CmdletBinding()]
param([switch]$Push)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$branch = 'feature/functional-dialog-placement'
$expectedBlob = 'da1eca6a8acf49f5611867b41ffbec72f36ca021'
$repo = (git rev-parse --show-toplevel).Trim().Replace('/', '\').TrimEnd('\')
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repo)) { throw 'Not in a Git repository.' }
if (-not [string]::Equals($repo, (Get-Location).Path.TrimEnd('\'), [StringComparison]::OrdinalIgnoreCase)) { throw "Run from the repository root: $repo" }
$actualBranch = (git branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $actualBranch -ne $branch) { throw "Expected branch $branch; found $actualBranch" }
$actualBlob = (git rev-parse 'HEAD:src/WindowManagement.cs').Trim()
if ($LASTEXITCODE -ne 0 -or $actualBlob -ne $expectedBlob) { throw "Source changed ($actualBlob); inspect before patching. Do NOT rerun." }
$changes = @(git status --porcelain)
if ($LASTEXITCODE -ne 0 -or $changes.Count -ne 0) { throw 'Working tree not clean; preserve local changes.' }

$sdk = Join-Path $env:USERPROFILE 'Desktop\HAMILTON_NAODELETAR\dotnet'
$dotnet = Join-Path $sdk 'dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) { throw "Portable .NET SDK missing: $dotnet" }
$env:DOTNET_ROOT = $sdk
$env:PATH = "$sdk;$env:PATH"
$path = Join-Path $repo 'src\WindowManagement.cs'
$source = [IO.File]::ReadAllText($path)
$old = '        menu.Closed += delegate { menu.Dispose(); };'
$new = @'
        // Closed fires while WinForms may still be unwinding WM_CONTEXTMENU.
        // Never dispose an active drop-down synchronously from its Closed handler.
        menu.Closed += delegate {
            if (owner.IsDisposed || !owner.IsHandleCreated) return;
            try {
                owner.BeginInvoke((MethodInvoker)delegate {
                    if (!menu.IsDisposed) menu.Dispose();
                });
            }
            catch (InvalidOperationException) { /* Form is shutting down. */ }
        };
'@
if ([regex]::Matches($source, [regex]::Escape($old)).Count -ne 1) { throw 'Expected one unsafe Closed handler; no source changed.' }
$source = $source.Replace($old, $new.TrimEnd("`r", "`n"))
[IO.File]::WriteAllText($path, $source, [Text.UTF8Encoding]::new($false))
git diff --check -- src/WindowManagement.cs
if ($LASTEXITCODE -ne 0) { throw 'Source diff failed; no commit/push.' }

Write-Host 'Running five-language smoke tests...'
& $dotnet run --project tests/LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Smoke tests failed; patch remains local. Do NOT rerun.' }

# Publishing builds the complete WinForms app and creates a fresh unique preview.
$preview = Join-Path $env:TEMP ('WinSidebar-context-menu-test-' + [guid]::NewGuid().ToString('N'))
Write-Host "Building preview at: $preview"
& $dotnet publish WinSidebar.csproj -c Release -r win-x64 --self-contained true -o $preview
if ($LASTEXITCODE -ne 0) { throw 'Windows publish failed; patch remains local. Do NOT rerun.' }
$exe = Join-Path $preview 'WinSidebar.exe'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) { throw "Published EXE not found: $exe" }
Write-Host 'Smoke and publish completed locally; right-click GUI regression is NOT verified yet.'

if ($Push) {
    gh auth status
    if ($LASTEXITCODE -ne 0) { throw 'GitHub CLI not authenticated; patch remains local.' }
    git add -- src/WindowManagement.cs
    if ($LASTEXITCODE -ne 0) { throw 'git add failed.' }
    git commit -m 'fix(ui): defer disposing per-window context menu until event processing ends'
    if ($LASTEXITCODE -ne 0) { throw 'git commit failed.' }
    git push -u origin $branch
    if ($LASTEXITCODE -ne 0) { throw 'git push failed; commit is local.' }
}

# Do not kill any existing WinSidebar instance or change user preferences.
if (Get-Process WinSidebar -ErrorAction SilentlyContinue) {
    Write-Host "Close the previous WinSidebar through its tray icon, then run: Start-Process -FilePath '$exe'"
    throw 'Preview built but not opened because another WinSidebar process is still running.'
}
Write-Host "Opening the newly published executable: $exe"
Start-Process -FilePath $exe
Write-Host 'Test right-click on multiple window rows, Rename, Ignore and the tray language menu.'
