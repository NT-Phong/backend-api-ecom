$ErrorActionPreference = "Stop"

$skills = Get-ChildItem -LiteralPath ".agents/skills" -Directory
foreach ($skill in $skills) {
    $entry = Join-Path $skill.FullName "SKILL.md"
    $metadata = Join-Path $skill.FullName "agents/openai.yaml"
    if (-not (Test-Path -LiteralPath $entry)) { throw "Missing SKILL.md: $($skill.Name)" }
    if (-not (Test-Path -LiteralPath $metadata)) { throw "Missing agents/openai.yaml: $($skill.Name)" }

    $content = Get-Content -LiteralPath $entry -Raw
    if ($content -notmatch '(?s)^---\s*\r?\nname:\s*([^\r\n]+)\r?\ndescription:\s*([^\r\n]+)\r?\n---') {
        throw "SKILL.md frontmatter must contain only name then description: $($skill.Name)"
    }
    $declaredName = $Matches[1].Trim()
    if ($declaredName -ne $skill.Name) { throw "Skill folder/name mismatch: $($skill.Name) != $declaredName" }

    $lineCount = (Get-Content -LiteralPath $entry).Count
    if ($lineCount -gt 100) { throw "SKILL.md exceeds 100-line project budget: $($skill.Name) ($lineCount)" }

    $ui = Get-Content -LiteralPath $metadata -Raw
    foreach ($field in @('display_name:', 'short_description:', 'default_prompt:')) {
        if ($ui -notmatch [regex]::Escape($field)) { throw "Missing $field in $metadata" }
    }
    if ($ui -notmatch [regex]::Escape("`$$declaredName")) { throw "default_prompt must mention `$$declaredName" }
}

$markdown = Get-ChildItem -LiteralPath ".agents/skills" -Recurse -File -Filter "*.md"
foreach ($file in $markdown) {
    $text = Get-Content -LiteralPath $file.FullName -Raw
    foreach ($match in [regex]::Matches($text, '\[[^\]]+\]\(([^)]+)\)')) {
        $target = $match.Groups[1].Value.Trim()
        if ($target -eq '' -or $target.StartsWith('#') -or $target -match '^[a-zA-Z][a-zA-Z0-9+.-]*:') { continue }
        $resolved = Join-Path (Split-Path -Parent $file.FullName) $target.Split('#')[0]
        if (-not (Test-Path -LiteralPath $resolved)) { throw "Broken skill link in $($file.FullName): $target" }
    }
}

Write-Output "Skill docs verification completed."
