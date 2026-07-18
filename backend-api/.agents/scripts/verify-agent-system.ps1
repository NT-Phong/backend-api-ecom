$ErrorActionPreference = "Stop"

& ".agents/scripts/verify-guidance.ps1"
& ".agents/scripts/verify-skill-docs.ps1"
& ".agents/scripts/run-agent-evals.ps1" -ValidateOnly

$requiredAgents = @(
    ".codex/agents/commerce-reviewer.toml",
    ".codex/agents/backend-verifier.toml",
    "Core/Ecom.Domain/Entities/Commerce/AGENTS.md",
    "Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/Commerce/AGENTS.md",
    "Tests/AGENTS.md"
)
$missingAgents = $requiredAgents | Where-Object { -not (Test-Path -LiteralPath $_) }
if ($missingAgents) { throw "Missing custom/nested agent files: $($missingAgents -join ', ')" }

foreach ($agentFile in $requiredAgents[0..1]) {
    $text = Get-Content -LiteralPath $agentFile -Raw
    foreach ($field in @('name =', 'description =', 'developer_instructions =')) {
        if ($text -notmatch [regex]::Escape($field)) { throw "Missing $field in $agentFile" }
    }
}

$status = Get-Content -LiteralPath ".agents/skills/commerce-system/references/current-status.md" -Raw
foreach ($heading in @('Last source review:', '## Verified Evidence', '## Unverified Working-Tree Work', '## Active Gate')) {
    if ($status -notmatch [regex]::Escape($heading)) { throw "Commerce status missing evidence section: $heading" }
}

$cases = Get-Content -LiteralPath ".agents/evals/cases.json" -Raw | ConvertFrom-Json
$skillCount = (Get-ChildItem -LiteralPath ".agents/skills" -Directory).Count
$metadataCount = (Get-ChildItem -LiteralPath ".agents/skills" -Recurse -File -Filter "openai.yaml").Count
$metrics = [ordered]@{
    RootAgentLines = (Get-Content -LiteralPath "AGENTS.md").Count
    SkillCount = $skillCount
    SkillMetadataCoveragePercent = [math]::Round(($metadataCount / $skillCount) * 100)
    EvalCaseCount = $cases.Count
    ForwardTestCaseCount = 3
    CustomAgentCount = (Get-ChildItem -LiteralPath ".codex/agents" -File -Filter "*.toml").Count
    NestedAgentCount = 3
}

Write-Output "Agent system verification completed."
$metrics | ConvertTo-Json
