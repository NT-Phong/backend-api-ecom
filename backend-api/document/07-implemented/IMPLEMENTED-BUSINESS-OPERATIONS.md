# Danh mục nghiệp vụ đã triển khai

Đây là inventory nghiệp vụ của snapshot hiện tại, không phải backlog. `Implemented` nghĩa backend slice và contract tồn tại; vẫn cần smoke/integration evidence riêng cho môi trường. `Partial` nghĩa chỉ phạm vi ghi rõ đang có. Entity-only được xếp `Foundation`, không liệt kê như live operation.

## Authentication và authorization

| Nghiệp vụ đã có | Actor / điều kiện | Kết quả và bảo vệ |
| --- | --- | --- |
| OTP V1 register/login | Public; phone/OTP hợp lệ | tạo/xác minh principal và session facts; rate/security rule áp dụng |
| Password V2 register/verify/login | Public; feature/config hiệu lực | account/password flow, token/session; register accepted có thể trả 202 |
| Forgot/reset/change/setup password | Public token hoặc Bearer; change/setup có CSRF | protected reset/change, revoke/rotate phù hợp |
| Refresh, me, logout, logout-all | token/session hợp lệ theo route | rotate hoặc revoke session; không tin refresh token đã dùng |
| Revoke own/management session | owner hoặc management permission | session bị revoke, security projection có thể tra cứu |
| Role-policy và profile update | permission tương ứng | authorization theo claim/policy; profile update bị giới hạn field |

## Catalog, Producer và Media

| Nghiệp vụ đã có | Điều kiện chính | State/data effect |
| --- | --- | --- |
| Public Product/Category search, list, detail | entity public và read conditions | DTO public, filter/sort/paging server-side |
| Create/update Product Draft | Producer hợp lệ, policy, fresh stamp | thay content/SEO/producer facts và stamp |
| Replace categories | categories hợp lệ, fresh stamp | thay toàn bộ assignments/primary ordering |
| Option/value/variant management | thuộc đúng Product, unique/invariant hợp lệ | cấu hình purchasable variants và option mapping |
| Append Variant price period | amount/currency/window hợp lệ | thêm lịch sử giá; không overwrite order snapshot |
| Product submit/publish/pause/discontinue/restore | lifecycle gate + permission + stamp | domain transition; guarded soft-delete là operation riêng |
| Category create/update/publish/pause/hide/tree | hierarchy/lifecycle hợp lệ | thay category lifecycle và public visibility |
| Producer create/update/verify/publish/hide | management permission, dependency gate | producer lifecycle; hide không bypass published dependencies |
| Producer contact và facility create | producer ownership/permission | lưu contact/facility; facility update/delete chưa được cam kết live |
| Media upload/metadata/retry/delete | ProductImage, tối đa 10 MiB, ownership/reference rule | Pending → Clean/Public hoặc Rejected/Failed |
| Attach/update/primary/remove Product media | Clean/usable asset + Product stamp | `ProductMedia` ordering/caption/primary thay đổi |

## Cart, Customer, Checkout và Order

| Nghiệp vụ đã có | Điều kiện chính | State/data effect |
| --- | --- | --- |
| Get/add/change/remove Cart item | guest/user principal, CSRF mutation, active variant | server resolves price facts; trả authoritative CartDto |
| Merge guest Cart sau login | Bearer + guest cookie + CSRF | lock và merge carts; clear guest ownership sau commit |
| Address list/create/update/default/delete | authenticated owner + CSRF mutation | CRUD sổ địa chỉ và invariant default |
| Checkout preview | selected `CartItem.Id`, recipient/payment facts | tính lines/totals/shipping/fingerprint; không reserve |
| Create Order | CSRF, fingerprint còn đúng, Idempotency-Key | snapshots Order/Items/Payment, reservation và cart conversion atomically |
| Customer list/detail/cancel Order | owner; cancel state cho phép | read history hoặc cancel + release reservation |
| Management list/detail/analytics | management permissions/filters | read model; không tự đổi transactional state |
| Confirm/cancel/add internal note | permission và current state hợp lệ | Order transition/history/note; không generic patch status |

## Payment, Shipment và Inventory

| Nghiệp vụ đã có | Điều kiện chính | State/data effect |
| --- | --- | --- |
| Create SePay Hosted Checkout intent | customer-owned payable Order | create/reuse attempt và trả hosted form; chưa Paid |
| Create SePay VietQR intent | customer-owned payable Order | create/reuse QR attempt/reference; chưa Paid |
| Process Hosted IPN | valid `X-Secret-Key`, matching facts | deduplicate notification; Paid hoặc NeedsReconciliation |
| Process bank webhook | valid timestamp/signature/raw body | deduplicate và đối chiếu transfer trước transition |
| Reconciliation/manual bank verification | management permission | resolve payment theo invariant và lưu audit facts |
| Refund Payment | refundable payment/state/permission | payment transaction/state đổi; không auto-restock |
| Prepare/start/complete/fail Shipment | confirmed order và valid shipment state | shipment items/history; Ship consumes reservation/stock |
| Create/update/list StockLocation | Inventory permission + stamp | location lifecycle/configuration |
| List/initialize InventoryLevel | tracked variant/location | initialize tạo level 0, không tự sinh stock |
| Adjust inventory | reason, delta, permission, fresh stamp | atomic level change + append Adjustment movement |
| List movement ledger | permission/filter/paging | read audit trail, không sửa ledger |
| Expire/release reservation | active expired/cancelled allocation | release reserved quantity + movement/state |
| Receive returned items | failed/returned shipment facts | accepted quantity tạo Return movement và restore stock |

## Management và system

| Nghiệp vụ đã có | Mức triển khai |
| --- | --- |
| Dashboard overview và Order analytics | Implemented core; aggregate read model theo range/filter |
| Standard shipping fee setting GET/PUT | Partial typed setting; có concurrency, không phải generic secret editor |
| Audit log query | Partial projection; không cam kết mọi mutation đều có audit record |
| Security sessions/events | Implemented management read/revoke slice |

## Capability chưa được gọi là đã triển khai

- Promotion/Coupon/redemption.
- Certification/Trust/Trace lot-event-evidence.
- Wishlist, Review, Q&A.
- CMS Page/Article/Banner/Navigation/SEO redirect operations.
- TradeInquiry/Partner application hoàn chỉnh.
- Notification delivery và Analytics platform hoàn chỉnh.
- Purchase Order, receiving workflow, transfer, stocktake và generic warehouse suite.
- Generic Order status patch, edit Order lines sau tạo, multi-shipping-method và coupon checkout.
- Passkey/OIDC production và mọi guarantee về email/SMS/provider availability.

## Bằng chứng và cách dùng

Các operation trên được đối chiếu từ route/controller, request-handler, domain entity/service và test slice ở snapshot tài liệu. Điều đó chứng minh **có implementation trong code**, không tự chứng minh database migration đã apply, external provider reachable hay production configuration đúng. Khi lên ý tưởng, Agent phải ghi rõ đang mở rộng operation hiện có hay đề xuất capability mới.

