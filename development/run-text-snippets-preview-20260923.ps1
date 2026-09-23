# Owner-run preview for the four configurable text snippets.
# Windows PowerShell 5.1; no administrator, CMD, GitHub Actions or FILEBRIDGE.
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$expectedBranch = 'feature/text-snippets'
$expected = @{
    'src/WinSidebar.cs'   = '224ba64b2c85a830bd954685568276df2a97b53e'
    'src/SnippetStore.cs' = '66d38ce07b0555d66fa954eb8867d021ccf8c2b7'
    'src/SnippetEditor.cs'= '7477fb7eacafee41cb63b66cb4941ff9306621a7'
    'src/TextInjection.cs'= '4b5e893363a4e4f9827dd003552252c2f84b526d'
    'WinSidebar.csproj'   = 'a7c5f14eaccefd2fb33d93166e51ab7bf445318c'
}

$rootFromGit = (git rev-parse --show-toplevel).Trim()
if ($LASTEXITCODE -ne 0) { throw 'Unable to resolve the Git repository root.' }
$root = (Resolve-Path -LiteralPath $rootFromGit).ProviderPath.TrimEnd('\')
$current = (Resolve-Path -LiteralPath '.').ProviderPath.TrimEnd('\')
if (-not [string]::Equals($root, $current, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Run this script from the WinSidebar repository root. Git root: $root ; current: $current"
}
$branch = (git branch --show-current).Trim()
if ($LASTEXITCODE -ne 0 -or $branch -ne $expectedBranch) {
    throw "Unexpected branch: $branch"
}
$changes = @(git status --porcelain)
if ($LASTEXITCODE -ne 0 -or $changes.Count -ne 0) {
    throw 'Working tree is not clean. Preserve local changes before testing.'
}
foreach ($path in $expected.Keys) {
    $blob = (git rev-parse "HEAD:$path").Trim()
    if ($LASTEXITCODE -ne 0 -or $blob -ne $expected[$path]) {
        throw "Reviewed source mismatch: $path ($blob). Pull and inspect before running."
    }
}

$dotnetRoot = Join-Path $env:USERPROFILE 'Desktop\HAMILTON_NAODELETAR\dotnet'
$dotnet = Join-Path $dotnetRoot 'dotnet.exe'
if (-not (Test-Path -LiteralPath $dotnet -PathType Leaf)) {
    throw "Portable .NET SDK not found: $dotnet"
}
$env:DOTNET_ROOT = $dotnetRoot
$env:PATH = "$dotnetRoot;$env:PATH"

if (Get-Process WinSidebar -ErrorAction SilentlyContinue) {
    throw 'A WinSidebar instance is already running. Exit it from the tray, then run this preview again.'
}

Write-Host 'Running five-language localization smoke...'
& $dotnet run --project .\tests\LocalizationSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Localization smoke failed.' }

Write-Host 'Running isolated snippet-store smoke...'
& $dotnet run --project .\tests\SnippetStoreSmoke.csproj -c Release
if ($LASTEXITCODE -ne 0) { throw 'Snippet-store smoke failed.' }

$output = Join-Path $env:TEMP ('WinSidebar-snippets-preview-' + [Guid]::NewGuid().ToString('N'))
Write-Host "Publishing preview to: $output"
& $dotnet publish .\WinSidebar.csproj -c Release -r win-x64 --self-contained true -o $output
if ($LASTEXITCODE -ne 0) { throw 'Windows publish failed.' }

$exe = Join-Path $output 'WinSidebar.exe'
if (-not (Test-Path -LiteralPath $exe -PathType Leaf)) {
    throw "Published WinSidebar.exe not found: $exe"
}

Write-Host ''
Write-Host 'Preview ready. No source or profile data was changed by this runner.'
Write-Host 'Test plan:'
Write-Host '  A. Confirm the four SCRIPT rows still render below ATALHOS.'
Write-Host '  B. The ⇔ header button must cycle width: narrow -> medium -> wide -> narrow.'
Write-Host '  C. The ⇕ header button must cycle height: compact -> medium -> tall -> compact.'
Write-Host '  D. Restart later and verify the chosen width/height and saved snippet content persist.'
Write-Host '  E. In ChatGPT/Notepad, Ctrl+Shift+F1 must still paste Script 1.'
Write-Host '  F. With ChatGPT focused, move the pointer to WinSidebar and click Script 1: focus must return to ChatGPT and paste there.'
Write-Host '  G. Rapid repeated hotkey while one paste is active must remain silent.'
Write-Host '  H. Existing Shift+F1..F4 and right-click menus must still work.'
Write-Host ''
Write-Host "Opening: $exe"
Start-Process -FilePath $exe
