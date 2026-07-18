param(
    [Parameter(Mandatory)] [string]$SourceRoot,
    [string]$ManifestPath = "tools/reuse-import/reuse-manifest.json",
    [string[]]$Group
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot/ReuseImport.Common.ps1"

$sourceRootFull = (Resolve-Path -LiteralPath $SourceRoot).Path
$targetRoot = (Resolve-Path -LiteralPath ".").Path
$entries = Get-ReuseImportEntries -ManifestPath $ManifestPath
if ($Group) { $entries = $entries | Where-Object { $_.group -in $Group } }

$report = foreach ($entry in $entries) {
    Assert-ReuseDestinationAllowed -Destination $entry.destination
    $source = Resolve-ReusePath -Root $sourceRootFull -RelativePath $entry.source -Label "Source path"
    $target = Resolve-ReusePath -Root $targetRoot -RelativePath $entry.destination -Label "Destination path"
    $sourceHash = Get-ReuseHash -Path $source
    $targetHash = Get-ReuseHash -Path $target
    [pscustomobject]@{
        Group = $entry.group
        Action = $entry.action
        SourceExists = $null -ne $sourceHash
        TargetExists = $null -ne $targetHash
        SameContent = $sourceHash -and $targetHash -and $sourceHash -eq $targetHash
        Destination = $entry.destination
        Reason = $entry.reason
    }
}

$report | Format-Table -AutoSize
