# Future Capabilities và idea space

Các mục dưới đây là hướng phát triển phù hợp model dự án, **không phải API live**. Agent được dùng để brainstorming/roadmap nhưng phải gắn nhãn `ROADMAP` và chỉ chuyển thành implemented sau contract, migration, test và approval.

## Trust và traceability

- Certification cho Product/Producer/Facility với evidence, verifier, expiry và status.
- TraceProfile → TraceLot → TraceEvent → Evidence để kể nguồn gốc theo lô.
- Public trust badge chỉ khi verified/active; hết hạn/thu hồi phải ẩn hoặc ghi rõ.
- QR truy xuất nên trỏ stable public page, không chứa secret/raw internal ID nếu không cần.

## Customer engagement

- Wishlist theo user.
- ProductReview chỉ cho purchase eligibility, moderation và media safety.
- ProductQuestion/ProductAnswer với moderation, staff attribution và customer-safe visibility.
- Newsletter consent có state Pending/Subscribed/Unsubscribed và audit consent.

## Content/CMS và SEO

- Page/Section, Article/Category, Banner, Campaign, NavigationItem, SeoRedirect.
- Schedule publish/unpublish, preview, audit và responsive media.
- Product/category slug change tạo redirect thay vì phá URL public.
- Không trộn marketing claim với verified certification facts.

## Promotion

- Promotion/Coupon, Product/Category scope và redemption ledger.
- Pricing service phải tính server-side; OrderDiscount lưu snapshot.
- Cần concurrency, eligibility, limit per customer, validity window và abuse controls.

## B2B

- TradeInquiry cho mua số lượng lớn/đại lý/hợp tác, items và status history.
- PartnerApplication, attachment restricted và staff assignment.
- Không dùng public product price làm quotation authority.

## Analytics và reporting

- VisitorSession/AnalyticsEvent cho PageView, Search, ProductView, AddToCart, Checkout.
- Consent/privacy, retention và bot filtering trước tracking production.
- Sales/finance reports lấy Order/Payment snapshots, không cộng từ storefront list.

## Điểm nhấn sản phẩm

- Mỗi Product một brand story page.
- Bản đồ producer/facility/point-of-sale.
- Bộ quà tặng địa phương/bundle.
- Short video commerce và trợ lý tư vấn nhưng không tự tạo claim về sức khỏe/chứng nhận.

## Gate để mở một capability

Business owner và actor → use cases/state machine → API/permission/error contract → entity/relationship/migration → security/privacy → FE states → tests PostgreSQL/integration → staging acceptance → cập nhật `source-status.md`.
