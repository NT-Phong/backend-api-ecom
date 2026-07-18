param(
    [Parameter(Mandatory)] [string]$SourceRoot,
    [string]$ManifestPath = "tools/reuse-import/reuse-manifest.json",
    [string]$StageRoot = ".reuse-staging",
    [string[]]$Group
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/ReuseImport.Common.ps1"

$sourceRootFull = (Resolve-Path -LiteralPath $SourceRoot).Path
$targetRoot = (Resolve-Path -LiteralPath ".").Path
$stageRootFull = [IO.Path]::GetFullPath((Join-Path $targetRoot $StageRoot))
$entries = Get-ReuseImportEntries -ManifestPath $ManifestPath
if ($Group) { $entries = $entries | Where-Object { $_.group -in $Group } }

$staged = 0
foreach ($entry in $entries) {
    if ($entry.action -eq "reference_only") { continue }
    Assert-ReuseDestinationAllowed -Destination $entry.destination
    $source = Resolve-ReusePath -Root $sourceRootFull -RelativePath $entry.source -Label "Source path"
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) {
        Write-Warning "Source missing, skipped: $($entry.source)"
        continue
    }
    $stageFile = Resolve-ReusePath -Root $stageRootFull -RelativePath (Join-Path "files" $entry.destination) -Label "Stage path"
    New-Item -ItemType Directory -Path (Split-Path -Parent $stageFile) -Force | Out-Null
    Copy-Item -LiteralPath $source -Destination $stageFile -Force
    $staged++
    Write-Output "STAGED [$($entry.group)] $($entry.destination)"
}

Write-Output "Staged $staged file(s) under $stageRootFull"
