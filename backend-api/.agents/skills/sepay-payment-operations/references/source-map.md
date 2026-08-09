# SePay source map

Run `scripts/scan-sepay-payment.ps1` before relying on this map; current source and worktree override it.

| Boundary | Anchor |
| --- | --- |
| Customer checkout | `Presentation/Ecom.API/Controllers/V1/OrdersController.cs` |
| Anonymous IPN | `Presentation/Ecom.API/Controllers/V1/SePayPaymentsController.cs` |
| Reconciliation read | `Presentation/Ecom.API/Controllers/V1/ManagementSePayPaymentsController.cs` |
| Checkout/IPN/reconciliation CQRS | `Core/Ecom.Application/Features/Commerce/Payments/` |
| Form/signature/secret | `Infrastructure/Ecom.Infrastructure/Services/SePayCheckoutService.cs` |
| Options/validator | `SePayOptions.cs`, `SePayOptionsValidator.cs`, `appsettings.json` |
| Domain | `Payment.cs`, `PaymentGatewayAttempt.cs`, `PaymentGatewayNotification.cs` |
| Persistence | `PaymentGatewayAttemptConfiguration.cs`, `PaymentGatewayNotificationConfiguration.cs` |
| Migration | `*AddSePayHostedCheckoutIpnAudit*` |

| Provider event | Local result |
| --- | --- |
| `ORDER_PAID`, exact valid, local pending | Mark paid + PaymentTransaction + accepted notification |
| repeated IPN | No duplicate transaction; acknowledge |
| `TRANSACTION_VOID`, late, mismatch | Notification + `NeedsReconciliation`; no automatic financial mutation |

Repository configuration is disabled by default and must never contain real credentials.
