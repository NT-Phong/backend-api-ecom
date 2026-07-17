$ErrorActionPreference = "Stop"

$requiredFiles = @(
    "AGENTS.md",
    ".agents/README.md",
    ".agents/context/project-map.md",
    ".agents/context/task-router.md",
    ".agents/context/codebase-analysis.md",
    ".agents/context/risk-map.md",
    ".agents/rules/agent-directives.md",
    ".agents/rules/optimized-workflow.md",
    ".agents/rules/safety-constraints.md",
    ".agents/skills/backend-api-architecture/SKILL.md",
    ".agents/skills/backend-api-architecture/references/execution-workflow.md",
    ".agents/skills/backend-api-architecture/templates/request-intake.md",
    ".agents/skills/backend-api-architecture/templates/skill-report-update.md"
)

$errors = New-Object System.Collections.Generic.List[string]

foreach ($file in $requiredFiles) {
    if (-not (Test-Path -LiteralPath $file)) {
        $errors.Add("Missing required guidance file: $file")
    }
}

Write-Output "Checking SKILL.md frontmatter..."
$skillDirectories = Get-ChildItem -LiteralPath ".agents/skills" -Directory
foreach ($skillDirectory in $skillDirectories) {
    $files = Get-ChildItem -LiteralPath $skillDirectory.FullName -File
    $exactEntry = $files | Where-Object { $_.Name -ceq "SKILL.md" }
    $caseVariantEntry = $files | Where-Object { $_.Name -ieq "SKILL.md" -and $_.Name -cne "SKILL.md" }

    if ($caseVariantEntry) {
        $errors.Add("Skill entrypoint must be exactly SKILL.md: $($caseVariantEntry.FullName)")
    }

    $hasSkillDocsOnlyMarker = $files | Where-Object { $_.Name -ceq ".doc-only" }
    if (-not $exactEntry -and -not $hasSkillDocsOnlyMarker) {
        $errors.Add("Missing SKILL.md in skill directory: $($skillDirectory.FullName)")
    }
}

$skillFiles = Get-ChildItem -LiteralPath ".agents/skills" -Recurse -File | Where-Object { $_.Name -ceq "SKILL.md" }
foreach ($skill in $skillFiles) {
    $content = Get-Content -LiteralPath $skill.FullName -Raw
    if ($content -notmatch '(?s)^---\s*\r?\n.*?\r?\n---') {
        $errors.Add("Missing YAML frontmatter: $($skill.FullName)")
        continue
    }

    $frontmatter = [regex]::Match($content, '(?s)^---\s*\r?\n(.*?)\r?\n---').Groups[1].Value
    if ($frontmatter -notmatch '(?m)^name:\s*\S+') {
        $errors.Add("Missing frontmatter name: $($skill.FullName)")
    }
    if ($frontmatter -notmatch '(?m)^description:\s*\S+') {
        $errors.Add("Missing frontmatter description: $($skill.FullName)")
    }
}

Write-Output "Checking markdown links inside .agents..."
$markdownFiles = Get-ChildItem -LiteralPath ".agents" -Recurse -File -Filter "*.md"
foreach ($md in $markdownFiles) {
    $text = Get-Content -LiteralPath $md.FullName -Raw
    $matches = [regex]::Matches($text, '\[[^\]]+\]\(([^)]+)\)')

    foreach ($match in $matches) {
        $target = $match.Groups[1].Value.Trim()
        if ($target -eq "" -or $target.StartsWith("#") -or $target -match '^[a-zA-Z][a-zA-Z0-9+.-]*:') {
            continue
        }

        $targetWithoutAnchor = $target.Split("#")[0]
        if ($targetWithoutAnchor -eq "") {
            continue
        }

        $baseDir = Split-Path -Parent $md.FullName
        $resolved = Join-Path $baseDir $targetWithoutAnchor
        if (-not (Test-Path -LiteralPath $resolved)) {
            $errors.Add("Broken markdown link in $($md.FullName): $target")
        }
    }
}

Write-Output "Checking stale .github/skills references..."
$stale = rg -n "\.github/skills|\.github\\skills" AGENTS.md .agents
if ($LASTEXITCODE -eq 0) {
    Write-Output $stale
    Write-Output "Only intentional canonical-memory warnings should remain."
} else {
    Write-Output "No stale .github/skills references found."
}

if ($errors.Count -gt 0) {
    $errors | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Output "Skill docs verification completed."
