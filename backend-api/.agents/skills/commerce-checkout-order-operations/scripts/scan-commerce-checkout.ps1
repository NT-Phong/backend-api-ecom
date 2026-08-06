param(
    [switch]$IncludeTests,
    [switch]$IncludeWorkingTree
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath 'Core/Ecom.Domain') -or -not (Test-Path -LiteralPath 'Infrastructure/Ecom.Infrastructure')) {
    throw 'Run this script from the backend-api repository root.'
}

function Show-Section([string]$Title) {
    Write-Output "`n### $Title"
}

Show-Section 'Commerce endpoints'
& rg -n "\[Http|class (Cart|Checkout|Orders|ManagementOrders)Controller" `
    Presentation/Ecom.API/Controllers/V1/CartController.cs `
    Presentation/Ecom.API/Controllers/V1/CheckoutController.cs `
    Presentation/Ecom.API/Controllers/V1/OrdersController.cs `
    Presentation/Ecom.API/Controllers/V1/ManagementOrdersController.cs

Show-Section 'CQRS slices and checkout services'
& rg --files Core/Ecom.Application/Features/Commerce Core/Ecom.Application/Common | rg "(Cart|Checkout|Order|Payment|Shipment|CheckoutPricing|CommerceCheckout|ICommerceCheckout)"

Show-Section 'Domain ownership and transitions'
& rg -n "CheckoutSelectedItems|ConfirmHold|Reserve\(|Release\(|Consume\(|MarkDeliveryFailed|RetryShipping|GuestTokenHashSnapshot" Core/Ecom.Domain/Entities/Commerce/Ordering Core/Ecom.Domain/Entities/Commerce/Inventory -g '*.cs'

Show-Section 'Persistence and locking'
& rg -n "IdempotencyRecord|FOR UPDATE|GuestTokenHashSnapshot|IsDeleted.*false|CartItem" `
    Infrastructure/Ecom.Infrastructure/Services/CartPrincipalResolver.cs `
    Infrastructure/Ecom.Infrastructure/Services/IdempotencyStore.cs `
    Infrastructure/Ecom.Infrastructure/Services/InventoryReservationStore.cs `
    Infrastructure/Ecom.Infrastructure/Persistence/Database/Configurations/Commerce/Ordering `
    Infrastructure/Ecom.Infrastructure/Migrations/20260806065505_AddCommerceCheckoutIdempotency.cs `
    Infrastructure/Ecom.Infrastructure/Migrations/20260806070313_AddGuestOrderOwnership.cs

Show-Section 'Transaction and side-effect boundaries'
& rg -n "ITransactionalRequest|UnitOfWorkBehavior|SaveChangesAsync|ConvertDomainEventsToOutbox|Outbox:Enabled" `
    Core/Ecom.Application/Common/Behaviours `
    Core/Ecom.Application/Features/Commerce `
    Infrastructure/Ecom.Infrastructure/DependencyInjection.cs `
    Infrastructure/Ecom.Infrastructure/Persistence/Database/Interceptors

if ($IncludeTests) {
    Show-Section 'Relevant tests'
    & rg -n "Cart|Checkout|Order|Reservation|Idempotency|Shipment" Tests/Ecom.Domain.Tests/Commerce Tests/Ecom.IntegrationTests/Application Tests/Ecom.IntegrationTests/PostgreSql -g '*.cs'
}

if ($IncludeWorkingTree) {
    Show-Section 'Working tree summary'
    & git -c safe.directory='C:/Users/Admin/OneDrive/Máy tính/Source_Ecom' status --short
}
