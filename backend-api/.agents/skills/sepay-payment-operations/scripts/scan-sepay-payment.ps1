param(
    [switch]$IncludeTests,
    [switch]$IncludeWorkingTree
)

$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath 'Core/Ecom.Domain') -or -not (Test-Path -LiteralPath 'Infrastructure/Ecom.Infrastructure')) {
    throw 'Run this script from the backend-api repository root.'
}

function Show-Section([string]$Title) { Write-Output "`n### $Title" }

Show-Section 'Routes and configuration'
& rg -n "SePay|sepay|PaymentCheckout|PaymentIpn" Presentation/Ecom.API/Controllers/V1 Presentation/Ecom.API/Extensions/ServiceExtensions.cs Presentation/Ecom.API/appsettings.json Core/Ecom.Application/Common/Configuration Infrastructure/Ecom.Infrastructure/DependencyInjection.cs Infrastructure/Ecom.Infrastructure/Security

Show-Section 'Application and domain flow'
& rg -n "SePay|sepay|PaymentGatewayAttempt|PaymentGatewayNotification|MarkPaid|NeedsReconciliation" Core/Ecom.Application/Features/Commerce/Payments Core/Ecom.Application/Common/Interfaces/ICommerceCheckoutServices.cs Core/Ecom.Domain/Entities/Commerce/Ordering Core/Ecom.Domain/Enums/Commerce/CommerceEnums.cs

Show-Section 'Persistence and migrations'
& rg -n "PaymentGatewayAttempt|PaymentGatewayNotification|SePay" Infrastructure/Ecom.Infrastructure/Persistence/Database/ApplicationDbContext.cs Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/Commerce/Ordering Infrastructure/Ecom.Infrastructure/Migrations

if ($IncludeTests) {
    Show-Section 'Tests'
    & rg -n "SePay|sepay|PaymentGatewayAttempt|PaymentGatewayNotification" Tests/Ecom.Domain.Tests Tests/Ecom.IntegrationTests
}

if ($IncludeWorkingTree) {
    Show-Section 'Working tree reminder'
    Write-Output "Run git -c safe.directory='<repository-root>' status --short separately; ownership policies vary by host."
}
