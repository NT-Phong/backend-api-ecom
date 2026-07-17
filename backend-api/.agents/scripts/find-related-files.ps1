param(
    [Parameter(Mandatory = $true)]
    [string]$Term
)

rg -n --hidden `
  --glob '!bin/**' `
  --glob '!obj/**' `
  --glob '!.git/**' `
  --glob '!.vs/**' `
  $Term `
  Core Infrastructure Presentation .agents
