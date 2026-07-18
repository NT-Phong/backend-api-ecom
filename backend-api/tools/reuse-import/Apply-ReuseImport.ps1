param(
    [switch]$Apply,
    [string]$ManifestPath = "tools/reuse-import/reuse-manifest.json",
    [string]$StageRoot = ".reuse-staging",
    [string[]]$Group
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/ReuseImport.Common.ps1"

if (-not $Apply) {
    throw "Refusing to modify source. Re-run with -Apply after reviewing staged files and updating approved entries to copy_if_missing."
}

$targetRoot = (Resolve-Path -LiteralPath ".").Path
$stageRootFull = [IO.Path]::GetFullPath((Join-Path $targetRoot $StageRoot))
$entries = Get-ReuseImportEntries -ManifestPath $ManifestPath
if ($Group) { $entries = $entries | Where-Object { $_.group -in $Group } }

$applied = 0
foreach ($entry in $entries) {
    if ($entry.action -ne "copy_if_missing") { continue }
    Assert-ReuseDestinationAllowed -Destination $entry.destination
    $stageFile = Resolve-ReusePath -Root $stageRootFull -RelativePath (Join-Path "files" $entry.destination) -Label "Stage path"
    $target = Resolve-ReusePath -Root $targetRoot -RelativePath $entry.destination -Label "Destination path"
    if (-not (Test-Path -LiteralPath $stageFile -PathType Leaf)) { throw "Staged file missing: $stageFile" }
    if (Test-Path -LiteralPath $target) { throw "Target exists; overwrite is forbidden: $target" }
    New-Item -ItemType Directory -Path (Split-Path -Parent $target) -Force | Out-Null
    Copy-Item -LiteralPath $stageFile -Destination $target
    $applied++
    Write-Output "APPLIED [$($entry.group)] $($entry.destination)"
}

Write-Output "Applied $applied file(s). Review git diff and run the narrowest relevant build/test."
