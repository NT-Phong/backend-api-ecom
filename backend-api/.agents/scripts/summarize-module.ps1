param(
    [Parameter(Mandatory = $true)]
    [string]$Path
)

Write-Output "Files under ${Path}:"
rg --files $Path | Sort-Object

Write-Output ""
Write-Output "Key symbols:"
rg -n "class |interface |record |enum |IRequest|IRequestHandler|AbstractValidator|EnableUnitOfWork" $Path
