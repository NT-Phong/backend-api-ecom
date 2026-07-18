param(
    [switch]$ValidateOnly
)

$ErrorActionPreference = "Stop"
$path = ".agents/evals/cases.json"
if (-not (Test-Path -LiteralPath $path)) { throw "Missing eval cases: $path" }

$cases = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
if ($cases.Count -lt 10) { throw "Expected at least 10 eval cases; found $($cases.Count)." }

$ids = @{}
foreach ($case in $cases) {
    foreach ($field in @("id", "prompt", "primarySkill", "expectedFirstSearch", "risk", "verification")) {
        if ([string]::IsNullOrWhiteSpace([string]$case.$field)) { throw "Eval case missing ${field}: $($case.id)" }
    }
    if ($ids.ContainsKey($case.id)) { throw "Duplicate eval id: $($case.id)" }
    $ids[$case.id] = $true
}

Write-Output "Validated $($cases.Count) agent eval cases."
if (-not $ValidateOnly) {
    Write-Output "Execution is evidence-driven: run each prompt in a fresh task, score with eval-rubric.md, and record raw output without leaking expected answers."
}
