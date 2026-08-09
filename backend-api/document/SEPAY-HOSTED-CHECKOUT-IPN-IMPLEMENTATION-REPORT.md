# Báo cáo triển khai SePay Hosted Checkout và IPN

**Ngày:** 2026-08-08  
**Phạm vi:** Thanh Hoa Commerce backend, source/worktree hiện tại.  
**Kết luận:** Source, migration và test hẹp đã hoàn thành; chưa đủ bằng chứng để bật Sandbox hay Production.

## Kết quả theo yêu cầu

| Hạng mục | Trạng thái | Bằng chứng |
| --- | --- | --- |
| Server làm chủ số tiền/trạng thái | Đạt | Checkout dùng Order/Payment DB; IPN đối chiếu invoice, VND, hai amount, trạng thái local |
| Hosted Checkout có chữ ký | Đạt | HMAC-SHA256/Base64 server-side; DTO trả ordered field list + POST |
| IPN là authority | Đạt | Chỉ `ProcessSePayIpnCommand` gọi `Payment.MarkPaid`; redirect chỉ dành cho UX |
| Secret/IPN hardening | Đạt ở source | fixed-time secret; SecretKey mode; JSON-only, 16 KB, rate limit |
| Replay/late/mismatch/void | Đạt ở source | notification audit + reconciliation, không automatic financial mutation |
| Schema | Đạt ở source | migration `AddSePayHostedCheckoutIpnAudit`, FK/check/unique indexes, snapshot sạch |
| Runtime proof | Chưa đạt | không có PostgreSQL test DB, merchant Sandbox, secret store hoặc public HTTPS IPN |

## Luồng hiện tại

```text
Checkout owner-scoped -> PaymentGatewayAttempt -> ordered signed form -> SePay
SePay IPN -> secret validation -> normalized notification -> exact verification
-> Payment.MarkPaid + PaymentTransaction in one UnitOfWork
```

`ORDER_PAID` chỉ cập nhật local khi provider trả `CAPTURED`/`APPROVED`, amount/invoice/currency khớp và Order/Payment còn Pending. `TRANSACTION_VOID`, late payment và mismatch thành `NeedsReconciliation`; V1 không auto-refund/void/cancel.

## Files/capabilities thêm mới

- Attempt/notification entities, configurations, DbSets và migration `20260807165319_AddSePayHostedCheckoutIpnAudit`.
- SePay checkout service/options/validator, ordered form contract và API endpoints.
- IPN state handler cho paid/duplicate/void/mismatch/late paths.
- Read-only reconciliation API, yêu cầu `payments.verify`.
- Domain transition tests; service signature/order/options tests.

## Verification đã chạy

| Lệnh | Kết quả |
| --- | --- |
| API build với output tách | Pass, 0 errors; 17 warnings có sẵn |
| Infrastructure build | Pass, 0 errors; 17 warnings có sẵn |
| Domain tests | Pass 44/44 |
| SePay service/validator filtered tests | Pass 2/2 |
| `dotnet ef migrations has-pending-model-changes` | Pass: no pending changes |
| Idempotent SQL review | Có table, FK, check, unique/index đúng thiết kế |
| `git diff --check` | Pass; chỉ cảnh báo CRLF |

## Gate chưa đạt

1. `ECOM_TEST_POSTGRES` không có: chưa chứng minh migration apply, rollback, constraint và concurrent IPN trên PostgreSQL.
2. Chưa có merchant Sandbox/public HTTPS IPN: chưa chứng minh end-to-end form, redirect và IPN.
3. Frontend phải đổi sang render `fields` theo thứ tự trả về và refresh trạng thái từ GET order, không dùng redirect để mark paid.
4. V1 không có provider refund/void tự động; finance vận hành reconciliation bằng quyền `payments.verify`.
5. EF tool local 10.0.3 thấp hơn runtime 10.0.10; nên đồng bộ trước release pipeline.

## Điều kiện bật Sandbox/Production

1. Apply migration trên staging được phê duyệt và chạy PostgreSQL tests.
2. Đưa `SePay__MerchantId`, `SePay__MerchantSecretKey`, `SePay__IpnSecretKey` vào secret store, không commit secrets.
3. Merchant cấu hình `SECRET_KEY` và IPN HTTPS `/api/v1/payments/sepay/ipn`.
4. Sandbox chứng minh paid, duplicate, void, mismatch, late payment và IPN—not redirect—là nguồn cập nhật local state.
5. Có approval rollout trước khi dùng Production credential/host riêng.
