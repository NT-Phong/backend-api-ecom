# Báo cáo thiết kế và kế hoạch nâng cấp hệ thống lưu trữ File/Image

> Phạm vi: Thanh Hoa Commerce backend API. Báo cáo này đối chiếu tài liệu tham khảo từ Mebieco với source hiện tại của dự án, sau đó đề xuất lộ trình triển khai có thể thực hiện theo từng batch. Source hiện tại là bằng chứng ưu tiên.

> Cập nhật source 2026-08-04: Product Image V1 hiện có `MediaController` với upload Pending, metadata và soft-delete; `MediaAsset` lưu intent/target visibility, Azure provider có Managed Identity option, và worker ClamAV/thumbnail được cấu hình bằng `MediaProcessing`. Scanner chỉ chạy khi `MediaProcessing:Enabled=true`; chưa có bằng chứng PostgreSQL/Azure/ClamAV staging và migration vẫn chưa được áp dụng.

## 1. Kết luận điều hành

Dự án **đã có media foundation**, không phải bắt đầu từ con số không:

- `MediaAsset` là registry metadata: lưu `StorageKey`, tên file, MIME, kích thước, loại media, visibility và scan status; bytes không nằm trong PostgreSQL.
- `LocalStorageService` đã tách khu vực `quarantine`, `public`, `private`; chỉ object public được expose qua `/media`.
- `FileUploadPolicy` đã kiểm tra byte signature của JPG/PNG/WebP/PDF, đối chiếu extension/MIME và áp giới hạn 10 MB hoặc 20 MB theo intent.
- `ProductMedia`, `InquiryAttachment`, payment proof và một số entity CMS đã dùng hoặc định hình tham chiếu `MediaAssetId`.

Tuy nhiên, đây mới là foundation nội bộ. Hiện chưa có `MediaController`, command upload, endpoint scan/promote, endpoint đọc private, ownership authorization hoặc Azure Blob implementation đang được DI đăng ký. `CommerceMediaService.UploadPendingAsync`, `CompleteScanAsync`, cleanup có source nhưng chưa có API/caller từ client. Vì vậy FE hiện không thể tạo một `MediaAssetId` hợp lệ để dùng với endpoint attach ảnh Product đã tồn tại.

Quyết định kiến trúc được đề xuất là **mở rộng `MediaAsset` thành media registry duy nhất của Commerce**, không tạo thêm `Document` aggregate/table song song và không sao chép nguyên module Mebieco. Các aggregate giữ bảng relation có ngữ nghĩa riêng, thay vì dùng `uuid[]` hoặc `AttachmentLink` generic ngay từ V1.

## 2. Đối chiếu tài liệu tham khảo và source hiện tại

| Hạng mục | Tài liệu tham khảo | Source Commerce hiện tại | Quyết định |
| --- | --- | --- | --- |
| Metadata tách bytes | `Document` lưu object key/metadata | `MediaAsset` đã lưu `StorageKey`/metadata | Giữ `MediaAsset`; không tạo `Document` thứ hai |
| Storage abstraction | Storage provider độc lập | `IStorageService` + `LocalStorageService` | Giữ abstraction, bổ sung Azure implementation |
| Quarantine lifecycle | Upload, scan, promote | `Pending/Clean/Rejected/Failed`; quarantine/public/private đã có | Hoàn thiện API, scanner và worker |
| Image thumbnail | WebP thumbnail | `IDocumentService` đã có ImageSharp thumbnail helper, nhưng `MediaAsset` chưa có thumbnail key | Bổ sung metadata thumbnail và orchestration |
| Upload API | Multipart/Base64 CQRS endpoint | Chưa có controller/command upload | Xây multipart Media API; không mở Base64 V1 |
| Private access | SAS/download sau quyền resource | Không có endpoint private download/read URL | Bổ sung resource authorization + proxy/SAS TTL ngắn |
| Attachment relationship | Relation rõ ràng ưu tiên hơn `uuid[]` | `ProductMedia`, `InquiryAttachment`, payment proof đã tồn tại | Tiếp tục relation theo aggregate |
| Production provider | Azure Blob | Package Azure Blob tồn tại nhưng không có implementation/DI usage | Azure Blob ở Production; Local chỉ Dev/test |

## 3. Hiện trạng source và các khoảng trống

### 3.1 Thành phần đã có

| Boundary | Thành phần | Trạng thái |
| --- | --- | --- |
| Domain | `Core/Ecom.Domain/Entities/Commerce/Catalog/MediaAsset.cs` | Có lifecycle scan/visibility và `IsPubliclyUsable` |
| Persistence | `MediaAssetConfiguration`, `ProductMediaConfiguration` | Có unique `StorageKey`, size check và unique primary ProductMedia |
| Storage | `IStorageService`, `LocalStorageService` | Có local quarantine/promote/delete/path traversal guard |
| Validation | `IFileUploadPolicy`, `FileUploadPolicy` | Có signature/MIME/extension/size policy |
| Application service | `CommerceMediaService`, `MediaFileService` | Có upload pending/promote/reject/cleanup source |
| HTTP serving | `Program.cs` static file provider | Chỉ expose `wwwroot/uploads/public` qua `/media` |
| Product integration | `CatalogProductsController` + `ManageProductMedia` | Có attach/update/primary/remove bằng `MediaAssetId` |
| Tests | `Tests/Ecom.IntegrationTests/Media` | Có file policy và local storage tests; chưa có PostgreSQL/API lifecycle tests |

### 3.2 Khoảng trống phải đóng

1. Không có route upload, metadata read, private download/read URL, scan completion hoặc delete media.
2. Không có uploader ownership/resource authorization: một GUID chưa được phép là quyền đọc/xóa file.
3. Không có scanner production; một object Pending không được tự động trở thành public/Clean.
4. Chưa có `ThumbnailStorageKey`, checksum hoặc service orchestration để tạo thumbnail.
5. Azure Blob package chưa thành provider vận hành; local disk không phù hợp production nhiều instance.
6. `CommerceMediaService` đang tự `SaveChangesAsync`; cần điều chỉnh để phù hợp UnitOfWork transaction convention hiện tại.
7. Chưa có background worker/reconciliation cho pending stale, orphan object, lỗi promote/delete và retry.
8. Chưa có PostgreSQL integration/API authorization tests; các test hiện tại không chứng minh rollback hay FK/concurrency.

## 4. Phạm vi V1 được khóa

### 4.1 Media intent và chính sách

| Intent | Cho phép | Visibility sau scan | Nơi gắn |
| --- | --- | --- | --- |
| `ProductImage` | JPG, PNG, WebP; tối đa 10 MB | Public | `ProductMedia` |
| `TradeInquiryAttachment` | JPG, PNG, WebP, PDF; tối đa 20 MB | Restricted | `InquiryAttachment` |
| `BankTransferProof` | JPG, PNG, WebP, PDF; tối đa 10 MB | Restricted | `PaymentTransaction.ProofMediaAssetId` |

Không hỗ trợ trong V1: video, archive/Office file, SVG/HTML, Base64 public API, direct public Blob upload, generic attachment relation, delete physical object ngay khi một link bị xóa.

### 4.2 Ownership và access model

- Trước khi attach, media thuộc uploader qua audit field `CreatedBy`; chỉ uploader hoặc administrator được xem trạng thái/xóa Pending asset.
- Sau khi attach, authorization dựa vào owner/permission của aggregate đang tham chiếu media. Ví dụ Product public chỉ được đọc nếu media `Public + Clean`; Trade Inquiry và payment proof không có anonymous URL.
- Delete chỉ đánh dấu/xóa physical file khi asset không còn active reference. V1 thực hiện reference check qua các relation đã hỗ trợ; các entity mới phải khai báo reference check của chúng.

### 4.3 Provider và key policy

- Development/test: `LocalStorageService`.
- Production: private Azure Blob container, Managed Identity hoặc secret store; không commit account key/connection string.
- Provider do server chọn theo environment, không nhận `StorageType` từ client.
- Key do server sinh; không dùng original filename làm key:

```text
media/quarantine/yyyy/MM/dd/{guid}.{ext}
media/public/yyyy/MM/dd/{guid}.{ext}
media/private/yyyy/MM/dd/{guid}.{ext}
media/thumbnails/yyyy/MM/dd/{guid}.webp
```

## 5. Target architecture

```text
Browser / Admin UI
  -> POST /api/v1/media (multipart)
  -> UploadMediaCommand + validator
  -> FileUploadPolicy: size + signature + MIME + intent
  -> IObjectStorage: write quarantine object
  -> Create MediaAsset(Pending) in PostgreSQL
  -> scan/thumbnail worker
  -> promote to public/private + MediaAsset.MarkClean(...)
  -> feature command attaches clean MediaAssetId to Product/Inquiry/Payment

Public product page
  -> ProductMediaReader -> /media/... only when Public + Clean

Restricted consumer
  -> GET download/read-url -> resource authorization -> stream proxy or short-lived read URL
```

### 5.1 Storage and media interfaces

Giữ public contract nhỏ. `IStorageService` có thể được đổi tên thành `IObjectStorage` nếu làm trong batch refactor riêng; không bắt buộc đổi tên ở batch đầu.

```csharp
public interface IObjectStorage
{
    Task<string> PutQuarantineAsync(Stream content, ObjectUpload upload, CancellationToken ct);
    Task<string> PromoteAsync(string quarantineKey, MediaVisibility visibility, CancellationToken ct);
    Task<Stream> OpenReadAsync(string storageKey, CancellationToken ct);
    Task<Uri> CreateReadUrlAsync(string storageKey, TimeSpan ttl, CancellationToken ct);
    Task DeleteIfExistsAsync(string storageKey, CancellationToken ct);
}
```

`ICommerceMediaService` điều phối metadata, thumbnail, compensation và lifecycle; không có feature handler nào gọi Azure SDK trực tiếp.

### 5.2 Transaction and compensation rule

Không gọi storage/scanner khi DB transaction đang mở. Upload object và PostgreSQL không có distributed transaction.

1. Validate và ghi object vào quarantine ngoài DB transaction.
2. Gửi command persist `MediaAsset(Pending)` trong transaction DB riêng.
3. Nếu persist thất bại, xóa quarantine object bằng compensation.
4. Worker scan/promote chạy sau commit. Nếu promote thành công nhưng DB update thất bại, xóa promoted object hoặc ghi job reconciliation.
5. Cleanup retry được thiết kế idempotent: `DeleteIfExists` không lỗi nếu object đã mất.

Điều này thay thế việc để service tự mở transaction hoặc tự `SaveChangesAsync`; normal commit thuộc handler/pipeline UnitOfWork.

### 5.3 MediaAsset model V1

Giữ các field hiện tại, bổ sung qua forward migration:

```text
ThumbnailStorageKey nullable, max 1000
Sha256 nullable, max 64
```

`StorageProvider` chỉ thêm khi có yêu cầu chạy đồng thời nhiều provider. Với một provider được quyết định theo deployment, không thêm enum/cột giả tạo.

Không lưu SAS URL, permanent public URL, Base64 hay raw byte trong database/log.

## 6. API contract đề xuất

### 6.1 Upload

```text
POST /api/v1/media
Content-Type: multipart/form-data
Authorization: media.upload policy

file        IFormFile, required
intent      ProductImage | TradeInquiryAttachment | BankTransferProof
altText     optional, only for image
```

Response 201:

```json
{
  "id": "uuid",
  "fileName": "tea-package.jpg",
  "contentType": "image/jpeg",
  "sizeBytes": 234567,
  "mediaType": "Image",
  "visibility": "Restricted",
  "scanStatus": "Pending",
  "intendedVisibility": "Public"
}
```

Không trả public URL khi upload. FE polling metadata hoặc nhận event sau khi scan hoàn tất; chỉ attach ProductImage khi `scanStatus=Clean` và `visibility=Public`.

### 6.2 Read and delete

```text
GET    /api/v1/media/{mediaAssetId}              metadata after authorization
GET    /api/v1/media/{mediaAssetId}/download     private stream after resource authorization
POST   /api/v1/media/{mediaAssetId}/read-url     short-lived restricted URL if provider supports it
DELETE /api/v1/media/{mediaAssetId}              detach/delete only when unreferenced
```

`/media/{path}` remains anonymous only for promoted `Public + Clean` objects. Private media must never be served by this static provider.

### 6.3 Aggregate integration

- Existing `POST /api/v1/catalog/products/{productId}/media` remains attach-only. It validates that `MediaAssetId` is clean/public, preserves product concurrency behavior and does not upload bytes.
- Trade Inquiry API will accept/attach only clean restricted assets owned by or authorized for the inquiry actor.
- Bank transfer confirmation accepts only clean restricted proof assets; never trust a client-provided payment result or media visibility.

## 7. Detailed delivery plan

### Phase 0 — Contract and approval gate

**Goal:** lock security and public contract before protected changes.

1. Write the four media documents listed in section 10.
2. Confirm provider choice, scanner technology, retention duration, max file count/request, and who owns uploaded-but-unattached media.
3. Add explicit permissions such as `media.upload`, `media.read`, `media.delete`, or document why existing policies are reused.
4. Review public API DTOs, authorization model, configuration and migration proposal.

**Approval required:** public API, authorization/permissions, EF migration/model, Azure configuration and background worker.

**Exit criteria:** ADR and endpoint contract approved; no code or migration yet.

### Phase 1 — Model and persistence hardening

**Goal:** make `MediaAsset` able to represent final stored media safely.

1. Add thumbnail/checksum fields and domain methods needed for completed scan lifecycle.
2. Update one EF configuration file for `MediaAsset`; generate a forward migration only after approval.
3. Verify existing references: `ProductMedia`, `InquiryAttachment`, payment proof, Banner and Article cover media.
4. Add indexes only after query/access-path evidence; preserve unique `StorageKey` and Product primary-media constraint.
5. Review generated idempotent SQL and snapshot; never rewrite an applied migration.

**Tests:** domain transitions, migration model check, PostgreSQL constraint tests.

### Phase 2 — Provider and physical storage

**Goal:** retain local testability while enabling production object storage.

1. Extract/refine storage abstraction without breaking existing local tests.
2. Implement `AzureBlobStorageService`: private container, generated keys, content headers, promote/copy, delete-if-exists, read stream and short-lived read URL.
3. Register Local only in Development/Test and Azure only in target production environments.
4. Validate startup configuration without logging secrets; configure Managed Identity/secret store outside tracked appsettings.
5. Preserve the current public static mapping only for `uploads/public` in Development; production public URL strategy is Azure/CDN only after approved deployment config.

**Tests:** local provider regression; Azure provider integration through emulator/test account where approved; no live credential in test output.

### Phase 3 — Multipart upload and Pending registry

**Goal:** allow a client to safely create a media asset.

1. Create `Features/Media/Commands/UploadMedia/` with one request, validator and handler.
2. Add `MediaController` with request/form limits aligned with Kestrel and reverse proxy limits.
3. Reuse and extend `FileUploadPolicy`: signature/MIME/extension agreement, intent allow-list, actual length, image decode and pixel limit.
4. Write original object to quarantine, create `MediaAsset(Pending)` and return metadata only.
5. Add compensation when database persistence fails after physical upload.
6. Refactor `CommerceMediaService` so it does not own a normal `SaveChangesAsync`; all transactional persistence follows the repository UnitOfWork convention.

**Acceptance tests:** valid image/PDF by intent, MIME spoof, extension spoof, oversize, unsupported signature, malformed image, DB failure compensation.

### Phase 4 — Scan, thumbnails and promotion worker

**Goal:** make only verified media usable.

1. Implement a scoped background worker/job that finds Pending media after commit.
2. Run malware scanner integration; keep `Pending` when scanner is unavailable, mark `Rejected`/`Failed` with a safe reason when appropriate.
3. For images, run AutoOrient/resize with ImageSharp and generate a WebP thumbnail; enforce dimension/pixel limits.
4. Promote original/thumbnail to public or private storage only after clean result; update keys/status atomically as far as possible with compensation/reconciliation.
5. Emit structured logs and metrics using media ID, intent, size, status and correlation ID; never log bytes, Base64, SAS URL or secrets.

**Policy:** do not fake a clean scan in Production. A Development-only scanner stub must be isolated by environment and visibly documented.

### Phase 5 — Authorized reads, deletion and cleanup

**Goal:** close the private access and lifecycle gaps.

1. Add metadata/read/download/delete commands and queries under `Features/Media/`.
2. Add `IMediaAccessService` to resolve uploader ownership before attach and aggregate/resource authorization after attach.
3. Implement stream proxy first; add Azure read URLs only after authorization and with 5–15 minute Read-only TTL.
4. Reference-check before deletion across supported relations. Soft-delete metadata only after physical delete is successful or a durable cleanup job is recorded.
5. Add scheduled cleanup for stale Pending/Rejected assets and orphan physical objects; make retry idempotent.

**Acceptance tests:** unauthenticated/private GUID denied, wrong user denied, public Clean image accessible, pending/rejected asset denied, repeated cleanup safe, referenced asset not deleted.

### Phase 6 — Commerce feature activation

**Goal:** consume verified media without changing aggregate ownership.

1. Product: upload `ProductImage` → scan clean/public → existing attach endpoint → publish eligibility passes.
2. Trade Inquiry: introduce its application/API slice only when B2B scope opens; attach clean restricted media.
3. Bank transfer: integrate clean restricted proof before manual payment confirmation; preserve payment state/amount checks.
4. CMS Banner/Article and later producer avatar/certification evidence each get a dedicated relation/access policy when their API slice is implemented.

No feature is allowed to construct a storage URL itself or invoke Azure SDK directly.

### Phase 7 — PostgreSQL, staging and operational rollout

**Goal:** prove runtime behavior before production use.

1. Run the dedicated external PostgreSQL test fixture for `MediaAsset` lifecycle, FK/restrict rules, filtered primary ProductMedia constraint, rollback and cleanup idempotency. Docker/Testcontainers are not used.
2. Run API authorization tests using a storage test double/emulator.
3. Deploy to staging with private container, managed identity, scanner, worker and observability.
4. Smoke-test upload → scan → attach → public read; restricted download; compensation; stale cleanup; scanner outage behavior.
5. Record retention, restore, incident and key-rotation procedures before enabling Production.

## 8. Test matrix

| Area | Minimum test cases |
| --- | --- |
| Upload policy | valid JPG/PNG/WebP/PDF; spoofed MIME/extension; unknown magic bytes; zero/oversize; wrong declared length |
| Image safety | corrupt image; decode failure; excessive pixels; thumbnail failure; orientation/thumbnail output |
| Storage | generated key only; quarantine-only promotion; path traversal rejection; delete idempotency; private path cannot become public URL |
| Database | unique StorageKey; positive SizeBytes; primary ProductMedia uniqueness; FK restrict; soft delete query filter |
| Transaction/compensation | storage success + DB failure deletes quarantine; promote success + DB failure enters reconciliation; retry is safe |
| Authorization | owner/admin allowed; arbitrary GUID denied; public Clean allowed anonymously; private/pending/rejected denied |
| Feature integration | product cannot attach/publish unclean media; Trade Inquiry/payment reject public or unclean attachments |
| Operations | stale Pending cleanup; scanner outage; orphan cleanup; no secret/SAS/raw bytes in logs |

## 9. Risks and non-goals

| Risk | Mitigation |
| --- | --- |
| Blob and database cannot share a transaction | quarantine + compensation + reconciliation worker |
| Infected/active content becomes public | scan gate, strict signature policy, no SVG/HTML V1, `nosniff` header |
| GUID-based unauthorized reads | resource-level `IMediaAccessService`, not controller `[Authorize]` alone |
| Local disk breaks across instances | Azure Blob is mandatory before multi-instance Production |
| Orphan costs/data leakage | idempotent cleanup, retention policy, monitoring and reference checks |
| Schema/API/auth changes are high-risk | approval gate, forward migration, reviewed SQL and staging proof |

Out of scope: image transformation variants beyond one thumbnail, video transcoding, generic DAM search, OCR, client direct-to-Blob signed upload, cross-provider migration and bulk legacy file migration.

## 10. Required project documents

Create and maintain these documents alongside implementation:

1. `document/architecture/media/MEDIA-SYSTEM-DESIGN.md` — boundary, entities, relation ownership, storage keys, lifecycle and diagrams.
2. `document/architecture/media/MEDIA-API-FE-GUIDE.md` — multipart fields, status handling, polling/error contract, attach flows and public/private URL rules.
3. `document/architecture/media/MEDIA-SECURITY-OPERATIONS.md` — authorization matrix, scanning, retention, cleanup, logging, incident response and secrets/Managed Identity runbook.
4. `document/architecture/media/ADR-001-media-provider-and-lifecycle.md` — why `MediaAsset` is canonical, Azure choice, no client provider selection, quarantine/scan decision, and V1 non-goals.
5. `document/architecture/media/MEDIA-TEST-ACCEPTANCE.md` — the test matrix and staging acceptance evidence.

This report is the planning baseline. It must be updated only when source, public contract, migration state or deployment decision has been verified.

## 11. Approval checklist before implementation

- [ ] Confirm V1 intents and allowed file types/limits.
- [ ] Confirm Azure Blob, Managed Identity and scanner ownership/budget.
- [ ] Approve public Media API and permission names.
- [ ] Approve `MediaAsset` migration fields and retention/deletion policy.
- [ ] Approve private download approach: proxy first or short-lived signed URL.
- [ ] Confirm worker hosting/monitoring and staging environment availability.
- [ ] Approve phase-by-phase implementation, beginning with Phase 1 only.
