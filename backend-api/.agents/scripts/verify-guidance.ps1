$ErrorActionPreference = "Stop"

$required = @(
    "AGENTS.md",
    ".agents/README.md",
    ".agents/context/task-router.md",
    ".agents/context/model-routing.md",
    ".agents/context/risk-map.md",
    ".agents/context/commands.md",
    ".agents/templates/task-contract.md",
    ".agents/templates/code-review.md",
    ".agents/skills/backend-api-architecture/SKILL.md",
    ".agents/skills/commerce-system/SKILL.md",
    ".agents/evals/cases.json",
    ".agents/evals/eval-rubric.md",
    ".agents/evals/baseline-2026-07-18.md",
    ".agents/evals/after-2026-07-18.md",
    ".agents/evals/forward-test-2026-07-18.md"
)

$missing = $required | Where-Object { -not (Test-Path -LiteralPath $_) }
if ($missing) { throw "Missing guidance files: $($missing -join ', ')" }

$allowedSkills = @("backend-api-architecture", "commerce-system")
$unexpected = Get-ChildItem -LiteralPath ".agents/skills" -Directory |
    Where-Object { $_.Name -notin $allowedSkills }
if ($unexpected) { throw "Unexpected skills: $($unexpected.Name -join ', ')" }

$markdown = Get-ChildItem -LiteralPath ".agents" -Recurse -File -Filter "*.md"
$stalePatterns = @(
    '\.agents/(examples|context/decisions\.md|context/glossary\.md|context/project-context\.md)',
    '\.agents/skills/(Device-system|camera-media|device-connection|FarmPlan-system|FCM-notification|pond_error|report-notification|scale_optimise|ZoneImage)',
    'Builds are user-run by default',
    '## Always Load'
)

foreach ($pattern in $stalePatterns) {
    $hit = $markdown | Select-String -Pattern $pattern
    if ($hit) { throw "Stale or prohibited guidance '$pattern': $($hit.Path):$($hit.LineNumber)" }
}

$rootLines = (Get-Content -LiteralPath "AGENTS.md").Count
if ($rootLines -gt 100) { throw "AGENTS.md exceeds 100-line bootstrap budget: $rootLines" }

Write-Output "Guidance verification completed."
