$ErrorActionPreference = "Stop"

$expected = @(
    "AGENTS.md",
    ".agents/README.md",
    ".agents/context/project-map.md",
    ".agents/context/task-router.md",
    ".agents/context/codebase-analysis.md",
    ".agents/context/risk-map.md",
    ".agents/context/commands.md",
    ".agents/rules/context-efficiency.md",
    ".agents/skills/backend-api-architecture/SKILL.md",
    ".agents/skills/camera-media/SKILL.md",
    ".agents/skills/Device-system/SKILL.md"
)

$missing = $expected | Where-Object { -not (Test-Path $_) }
if ($missing.Count -gt 0) {
    Write-Error "Missing guidance files: $($missing -join ', ')"
}

Write-Output "Checking stale .github/skills references..."
$stale = rg -n "\.github/skills|\.github\\skills" AGENTS.md .agents
if ($LASTEXITCODE -eq 0) {
    Write-Output $stale
    Write-Output "Only intentional non-canonical warnings should remain."
} else {
    Write-Output "No stale .github/skills references found."
}

Write-Output "Checking .agents references..."
rg -n "\.agents/" AGENTS.md .agents | Out-Null

Write-Output "Guidance verification completed."
