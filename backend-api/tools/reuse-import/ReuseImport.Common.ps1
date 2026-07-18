function Get-ReuseImportEntries {
    param([string]$ManifestPath)

    if (-not (Test-Path -LiteralPath $ManifestPath -PathType Leaf)) {
        throw "Manifest not found: $ManifestPath"
    }

    $manifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
    if (-not $manifest.entries -or $manifest.entries.Count -eq 0) {
        throw "Manifest has no entries: $ManifestPath"
    }

    return $manifest.entries
}

function Resolve-ReusePath {
    param([string]$Root, [string]$RelativePath, [string]$Label)

    if ([IO.Path]::IsPathRooted($RelativePath)) {
        throw "$Label must be relative: $RelativePath"
    }

    $rootFull = [IO.Path]::GetFullPath($Root)
    $candidate = [IO.Path]::GetFullPath((Join-Path $rootFull $RelativePath))
    $prefix = $rootFull.TrimEnd('\', '/') + [IO.Path]::DirectorySeparatorChar
    if (-not $candidate.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "$Label escapes its root: $RelativePath"
    }

    return $candidate
}

function Assert-ReuseDestinationAllowed {
    param([string]$Destination)

    $normalized = $Destination.Replace('\', '/')
    $blocked = @(
        '(^|/)appsettings[^/]*\.json$',
        '(^|/)ApplicationDbContext\.cs$',
        '(^|/)Migrations/',
        '(^|/)Entities/Commerce/',
        '(^|/)Configurations/Commerce/',
        '(^|/)Features/Auth/',
        '(^|/)Security/CurrentUser\.cs$'
    )

    foreach ($pattern in $blocked) {
        if ($normalized -match $pattern) {
            throw "Protected destination is blocked from reuse import: $Destination"
        }
    }
}

function Get-ReuseHash {
    param([string]$Path)
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) { return $null }
    return (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash
}
