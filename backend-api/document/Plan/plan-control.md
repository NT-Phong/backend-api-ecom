Được. Nếu mở rộng BE theo hướng ở trên thì FE cũng nên được chuẩn hóa thành **2 khu vực rõ ràng: User Storefront và Admin Backoffice**, thay vì tạo page rời rạc theo từng API.

Tôi đề xuất map FE theo nguyên tắc:

**Page → Use case → API/Handler BE**

Như vậy khi BE thêm capability thì FE biết chính xác cần page nào, component nào và luồng nào.

---

# PHẦN 1 — USER / STOREFRONT

## 1. Cấu trúc route tổng thể

```text
/
├── /products
├── /products/:slug
├── /categories/:slug
├── /producers
├── /producers/:slug
├── /collections/:slug
├── /search
│
├── /cart
├── /checkout
├── /checkout/success
├── /checkout/failed
│
├── /orders
├── /orders/:id
├── /orders/:id/tracking
│
├── /returns
├── /returns/:id
│
├── /wishlist
├── /reviews
├── /notifications
│
├── /account
│   ├── /profile
│   ├── /addresses
│   ├── /security
│   └── /preferences
│
├── /support
├── /support/:ticketId
│
├── /trace/:code
│
├── /articles
├── /articles/:slug
├── /pages/:slug
│
├── /login
├── /register
├── /forgot-password
└── /reset-password
```

---

# A. Nhóm Home / Discovery

## 2. Home Page

Route:

```text
/
```

Nên có:

* Hero banner
* Danh mục nổi bật
* Producer nổi bật
* Sản phẩm mới
* Best seller
* Sản phẩm OCOP/chứng nhận
* Khuyến mãi
* Sản phẩm đang giảm giá
* Recently viewed
* Bài viết
* Trust section

Map BE:

```text
GET /api/home
GET /api/banners/home
GET /api/categories/featured
GET /api/products?sort=best-selling
GET /api/products?sort=newest
GET /api/promotions/available
GET /api/articles
```

Tôi khuyên tạo riêng:

```text
GET /api/storefront/home
```

thay vì frontend gọi 10 API.

BE có thể dùng:

```text
GetStorefrontHomeQuery
StorefrontHomeService
```

---

# B. Catalog

## 3. Product Listing Page

Route:

```text
/products
```

Chức năng:

* pagination
* sorting
* filter
* category
* producer
* price
* attributes
* stock
* promotion
* rating

Ví dụ:

```text
/products?
category=nem-chua
&producer=abc
&minPrice=50000
&maxPrice=300000
&inStock=true
&sort=best-selling
&page=1
```

Map:

```text
SearchProductsQuery
CatalogReadService
ProductAvailabilityService
PricingService
PromotionEngine
```

---

# 4. Category Page

```text
/categories/:slug
```

Ví dụ:

```text
/categories/nem-chua
/categories/banh-keo
/categories/do-kho
```

Nên có:

* category banner
* SEO
* description
* subcategory
* filters
* sản phẩm
* content SEO cuối page

API:

```text
GET /api/categories/:slug
GET /api/categories/:slug/products
```

---

# 5. Product Detail Page — rất quan trọng

```text
/products/:slug
```

Một trang sản phẩm chuyên nghiệp nên có:

### Khu vực mua hàng

* Product name
* Product images/gallery
* Price
* Compare price
* Discount
* Variant selection
* Quantity
* Availability
* Add to cart
* Buy now
* Wishlist

### Nội dung

* Mô tả
* Thành phần
* Hướng dẫn sử dụng
* Bảo quản
* Xuất xứ
* Producer
* Certification

### Trust

* chứng nhận
* traceability
* OCOP
* verified producer

### Social proof

* rating
* reviews
* Q&A

### Upsell

* related products
* frequently bought together

Map BE:

```text
GET /api/products/{slug}
GET /api/products/{id}/reviews
GET /api/products/{id}/questions
GET /api/products/{id}/related
GET /api/products/{id}/traceability
```

Service:

```text
ProductDetailQueryService
ProductAvailabilityService
PricingService
PromotionEngine
ReviewService
TraceabilityService
RecommendationService
```

Tôi còn khuyên tạo endpoint aggregate:

```text
GET /api/storefront/products/:slug
```

trả một response đủ dùng cho page.

---

# C. Search

## 6. Search Page

```text
/search?q=nem+chua
```

Nên có:

* keyword
* autocomplete
* search suggestions
* recent search
* facets
* category
* producer
* price
* sort
* no-result recommendation

API:

```text
GET /api/search?q=
GET /api/search/suggestions?q=
```

BE:

```text
SearchProductsHandler
SearchSuggestionService
SearchRankingService
```

---

# D. Producer

## 7. Producer Listing

```text
/producers
```

Nên hiển thị:

* logo
* tên
* địa chỉ
* vùng sản xuất
* verified badge
* certification

---

# 8. Producer Detail

```text
/producers/:slug
```

Trang này rất phù hợp với mô hình đặc sản địa phương.

Nên có:

* cover
* story
* producer profile
* địa chỉ
* chứng nhận
* sản phẩm
* gallery
* traceability
* thông tin liên hệ

API:

```text
GET /api/producers/:slug
GET /api/producers/:slug/products
GET /api/producers/:slug/certifications
```

---

# E. Cart

## 9. Cart Page

```text
/cart
```

Nên có:

* cart items
* variant
* quantity
* current price
* old price
* promotion
* coupon
* subtotal
* estimated shipping
* unavailable item warning
* price changed warning

API:

```text
GET /api/cart

POST /api/cart/items
PUT /api/cart/items/:id
DELETE /api/cart/items/:id

POST /api/cart/coupons
DELETE /api/cart/coupons/:code

POST /api/cart/validate
```

UI đặc biệt nên có trạng thái:

```text
Price changed
Out of stock
Promotion expired
Quantity adjusted
Product unavailable
```

---

# F. Checkout

## 10. Checkout Page

```text
/checkout
```

Nên chia sections:

```text
1. Customer
2. Shipping address
3. Shipping method
4. Payment
5. Coupon/discount
6. Order review
```

Luồng:

```text
Cart
 ↓
Checkout preview
 ↓
User chọn shipping
 ↓
Preview lại
 ↓
Payment method
 ↓
Place order
```

API:

```text
POST /api/checkout/preview
POST /api/checkout/shipping-options
POST /api/checkout/validate
POST /api/checkout/place-order
```

Map:

```text
PreviewCheckoutHandler
GetShippingOptionsHandler
PlaceOrderHandler
```

---

# 11. Checkout Success

```text
/checkout/success
```

Hiển thị:

* order number
* payment status
* shipping information
* expected delivery
* CTA track order

Lưu ý:

Nếu payment online:

```text
redirect success
```

không có nghĩa:

```text
Payment = Paid
```

FE nên polling hoặc reload trạng thái:

```text
GET /api/orders/:id/payment-status
```

---

# G. User Orders

## 12. My Orders

```text
/orders
```

Tabs:

```text
All
Pending payment
Confirmed
Preparing
Shipping
Delivered
Cancelled
Returned
```

API:

```text
GET /api/orders?status=
```

---

# 13. Order Detail

```text
/orders/:id
```

Nên hiển thị:

* order timeline
* products
* payment
* shipping
* delivery address
* coupon
* discount
* total
* invoice
* support
* return button
* reorder

Actions tùy trạng thái:

```text
Pay
Cancel
Track
Return
Review
Reorder
Contact support
```

API:

```text
GET /api/orders/:id

POST /api/orders/:id/cancel
POST /api/orders/:id/reorder
```

---

# 14. Tracking Page

```text
/orders/:id/tracking
```

Nên có timeline:

```text
Order confirmed
↓
Preparing
↓
Picked up
↓
In transit
↓
Out for delivery
↓
Delivered
```

API:

```text
GET /api/orders/:id/tracking
```

---

# H. Return / Refund

## 15. Create Return

Có thể:

```text
/orders/:id/return
```

Hoặc modal.

User chọn:

* item
* quantity
* reason
* description
* photos
* desired resolution

```text
Refund
Exchange
```

API:

```text
POST /api/orders/:id/returns
```

---

# 16. Return Detail

```text
/returns/:id
```

Timeline:

```text
Requested
↓
Approved
↓
Return shipping
↓
Received
↓
Inspection
↓
Refund
↓
Completed
```

API:

```text
GET /api/me/returns/:id
```

---

# I. Wishlist

## 17. Wishlist Page

```text
/wishlist
```

Features:

* add to cart
* remove
* stock state
* current price
* price-drop indicator

API:

```text
GET /api/me/wishlist
POST /api/me/wishlist/:productId
DELETE /api/me/wishlist/:productId
```

---

# J. Reviews

## 18. My Reviews

```text
/reviews
```

Sections:

```text
Waiting for review
Reviewed
```

API:

```text
GET /api/me/reviews
POST /api/products/:id/reviews
```

Order delivered rồi mới hiện:

```text
Write review
```

---

# K. Account

## 19. Account Overview

```text
/account
```

Hiển thị nhanh:

* profile
* order statistics
* address
* wishlist
* notifications
* security

---

# 20. Profile

```text
/account/profile
```

API:

```text
GET /api/me/profile
PUT /api/me/profile
```

---

# 21. Addresses

```text
/account/addresses
```

API:

```text
GET /api/me/addresses
POST /api/me/addresses
PUT /api/me/addresses/:id
DELETE /api/me/addresses/:id
POST /api/me/addresses/:id/set-default
```

---

# 22. Security

```text
/account/security
```

Nên có:

* password
* phone
* email
* session devices
* logout all devices

API:

```text
PUT /api/auth/password/change
GET /api/me/sessions
DELETE /api/me/sessions/:id
POST /api/me/logout-all
```

---

# L. Notification Center

## 23. Notifications

```text
/notifications
```

Các loại:

```text
Order
Payment
Shipping
Promotion
Return
System
```

API:

```text
GET /api/me/notifications
POST /api/me/notifications/:id/read
POST /api/me/notifications/read-all
```

---

# M. Support

## 24. Support Center

```text
/support
```

Có thể gồm:

* FAQ
* return policy
* payment guide
* shipping guide
* create ticket

---

# 25. Support Ticket

```text
/support/:ticketId
```

UI dạng conversation.

API:

```text
GET /api/me/support/tickets/:id
POST /api/me/support/tickets/:id/messages
```

---

# N. Traceability

## 26. Trace Page

```text
/trace/:code
```

Rất đáng đầu tư.

Ví dụ scan QR sản phẩm:

```text
Producer
↓
Batch
↓
Production date
↓
Origin
↓
Certification
↓
Packaging
```

API:

```text
GET /api/trace/:code
```

Page này có thể truy cập không cần login.

---

# O. CMS

## 27. Article Listing

```text
/articles
```

---

# 28. Article Detail

```text
/articles/:slug
```

---

# 29. Generic CMS Page

```text
/pages/:slug
```

Ví dụ:

```text
/pages/about-us
/pages/return-policy
/pages/privacy
/pages/shipping-policy
```

---

---

# PHẦN 2 — ADMIN / BACKOFFICE

Admin nên tách route:

```text
/admin/*
```

và layout hoàn toàn khác storefront.

---

# 1. Admin Dashboard

Route:

```text
/admin
```

Widgets nên có:

```text
Revenue today
Orders today
Pending payment
Pending confirmation

Preparing shipments
Failed shipments

Returns pending
Refund pending

Low stock
Out of stock

Top products
Top categories

Recent orders
Recent payments
```

API:

```text
GET /api/admin/dashboard
```

Tôi khuyên dùng endpoint aggregate.

---

# A. Orders

## 2. Order List

```text
/admin/orders
```

Nên filter:

```text
order number
customer
phone
date
status
payment status
shipment status
total
```

Batch actions:

```text
Confirm
Export
Prepare shipment
```

---

# 3. Order Detail

```text
/admin/orders/:id
```

Đây nên là một page rất mạnh.

Tabs:

```text
Overview
Items
Payment
Shipment
Return
Refund
Timeline
Notes
Audit
```

Actions:

```text
Confirm order
Cancel order

Hold
Release

Create shipment

Refund

Add note
```

Không cho admin edit trực tiếp:

```text
status = Delivered
```

---

# B. Payment

## 4. Payment List

```text
/admin/payments
```

Filter:

```text
Paid
Pending
Failed
Expired
Needs reconciliation
```

---

# 5. Payment Detail

```text
/admin/payments/:id
```

Tabs:

```text
Payment
Attempts
Transactions
Webhook
Reconciliation
Refunds
```

Actions:

```text
Verify
Reconcile
Refund
```

---

# C. Returns

## 6. Return List

```text
/admin/returns
```

Tabs:

```text
Requested
Approved
In Transit
Received
Inspection
Refund
Completed
Rejected
```

---

# 7. Return Detail

```text
/admin/returns/:id
```

Sections:

* request
* item
* reason
* evidence
* shipment
* inspection
* restock decision
* refund decision

Actions:

```text
Approve
Reject
Receive
Inspect
Refund
Exchange
Complete
```

---

# D. Products

## 8. Product List

```text
/admin/products
```

Filters:

```text
Draft
Review
Published
Paused
Discontinued

Category
Producer
Stock
Price
```

---

# 9. Create Product

```text
/admin/products/create
```

Tôi khuyên dùng wizard:

```text
Step 1 Basic information
Step 2 Category
Step 3 Variant
Step 4 Pricing
Step 5 Inventory
Step 6 Media
Step 7 SEO
Step 8 Review
```

---

# 10. Product Detail/Edit

```text
/admin/products/:id
```

Tabs:

```text
General
Variants
Pricing
Inventory
Media
Categories
Attributes
SEO
Certification
Traceability
Reviews
Audit
```

Actions:

```text
Save draft
Submit review
Publish
Pause
Discontinue
```

---

# E. Category

## 11. Category Management

```text
/admin/categories
```

Nên dạng tree:

```text
Food
├── Nem chua
├── Bánh
└── Đồ khô
```

Functions:

* create
* rename
* reorder
* move
* SEO
* visibility

---

# F. Producer

## 12. Producer List

```text
/admin/producers
```

---

# 13. Producer Detail

```text
/admin/producers/:id
```

Tabs:

```text
Profile
Products
Certification
Media
Contact
SEO
Audit
```

Actions:

```text
Verify
Publish
Pause
```

---

# G. Inventory

## 14. Inventory Overview

```text
/admin/inventory
```

Table:

```text
SKU
Product
Variant
Location
On hand
Reserved
Available
Low stock
```

Filters:

```text
Low stock
Out of stock
Location
Producer
Category
```

---

# 15. Inventory Detail

```text
/admin/inventory/:variantId
```

Sections:

```text
Current stock
Reservation
Movement history
Adjustments
```

---

# 16. Inventory Adjustment

```text
/admin/inventory/adjustments/new
```

Admin phải nhập:

```text
quantity
reason
note
location
```

Không cho sửa số tồn trực tiếp.

---

# H. Warehouse

## 17. Purchase Orders

```text
/admin/purchase-orders
```

---

# 18. Purchase Order Detail

```text
/admin/purchase-orders/:id
```

Actions:

```text
Submit
Approve
Receive
Close
```

---

# 19. Goods Receiving

```text
/admin/receiving
```

Dùng cho:

```text
PO
→ Receive
→ Count
→ Inventory movement
```

---

# 20. Stock Transfers

```text
/admin/stock-transfers
```

Flow:

```text
Draft
→ Ship
→ In transit
→ Receive
```

---

# 21. Stocktake

```text
/admin/stocktakes
```

Flow:

```text
Create count
↓
Enter physical quantities
↓
Compare
↓
Variance
↓
Reconcile
```

---

# I. Shipping

## 22. Shipment List

```text
/admin/shipments
```

Tabs:

```text
Preparing
Ready
Shipped
Delivered
Failed
Returned
```

---

# 23. Shipment Detail

```text
/admin/shipments/:id
```

Actions:

```text
Prepare
Print label
Dispatch
Track
Mark failed
Receive return
```

---

# 24. Shipping Configuration

```text
/admin/settings/shipping
```

Subpages:

```text
Zones
Methods
Rates
Carriers
```

Routes:

```text
/admin/settings/shipping/zones
/admin/settings/shipping/methods
/admin/settings/shipping/rates
/admin/settings/shipping/carriers
```

---

# J. Promotions

## 25. Promotion List

```text
/admin/promotions
```

Tabs:

```text
Draft
Scheduled
Active
Paused
Expired
```

---

# 26. Promotion Create/Edit

```text
/admin/promotions/create
/admin/promotions/:id
```

Wizard:

```text
1. Promotion type
2. Conditions
3. Products/categories
4. Discount
5. Usage limit
6. Customer eligibility
7. Stack rules
8. Schedule
```

---

# 27. Coupon Management

```text
/admin/coupons
```

Features:

```text
Single coupon
Bulk generation
Usage
Redemption history
```

---

# K. Reviews

## 28. Review Moderation

```text
/admin/reviews
```

Tabs:

```text
Pending
Published
Reported
Hidden
```

Actions:

```text
Approve
Reject
Hide
Reply
```

---

# L. Q&A

## 29. Product Questions

```text
/admin/questions
```

Admin/staff có thể:

```text
Answer
Hide
Moderate
```

---

# M. CMS

## 30. Page Management

```text
/admin/content/pages
```

---

# 31. Articles

```text
/admin/content/articles
```

---

# 32. Banner Management

```text
/admin/content/banners
```

Fields:

```text
position
desktop image
mobile image
link
start/end time
priority
```

---

# 33. Navigation

```text
/admin/content/navigation
```

Nên drag/drop tree.

---

# N. SEO

## 34. SEO Management

```text
/admin/seo
```

Subpages:

```text
Redirects
Sitemap
Broken links
```

---

# O. Traceability

## 35. Certification Management

```text
/admin/certifications
```

---

# 36. Batch / Lot

```text
/admin/batches
```

---

# 37. Trace Detail

```text
/admin/batches/:id/traceability
```

Admin tạo:

```text
Production
Processing
Packaging
Inspection
Shipping
```

event.

---

# P. Customers

## 38. Customer List

```text
/admin/customers
```

Filter:

```text
new
returning
high value
inactive
```

---

# 39. Customer Detail

```text
/admin/customers/:id
```

Tabs:

```text
Profile
Orders
Returns
Reviews
Support
Timeline
Notes
Tags
```

Đây chính là Customer 360.

---

# Q. Support

## 40. Support Inbox

```text
/admin/support
```

Layout giống helpdesk:

```text
New
Assigned
Waiting customer
Resolved
```

---

# 41. Support Ticket Detail

```text
/admin/support/:id
```

Sections:

```text
Conversation
Customer
Order
Payment
Shipment
Return
Internal notes
```

---

# R. Notifications

## 42. Notification Templates

```text
/admin/notifications/templates
```

Ví dụ:

```text
Order confirmed
Payment success
Payment failed
Shipment dispatched
Delivered
Return approved
Refund completed
```

Channels:

```text
Email
SMS
Push
Zalo
```

---

# 43. Notification Logs

```text
/admin/notifications/logs
```

Hiển thị:

```text
Sent
Failed
Retrying
Delivered
```

---

# S. Analytics

## 44. Analytics Overview

```text
/admin/analytics
```

---

# 45. Sales Analytics

```text
/admin/analytics/sales
```

Metrics:

```text
Revenue
Orders
AOV
Refund
Discount
```

---

# 46. Product Analytics

```text
/admin/analytics/products
```

```text
Views
Add to cart
Conversion
Units sold
Revenue
Return rate
```

---

# 47. Customer Analytics

```text
/admin/analytics/customers
```

```text
New customer
Returning
Retention
Repeat purchase
```

---

# 48. Conversion Funnel

```text
/admin/analytics/funnel
```

```text
Product View
 ↓
Add To Cart
 ↓
Checkout
 ↓
Order
 ↓
Paid
```

---

# T. Audit / Security

## 49. Audit Log

```text
/admin/audit
```

Filters:

```text
user
action
entity
date
IP
```

---

# 50. Admin Users

```text
/admin/settings/users
```

---

# 51. Roles & Permissions

```text
/admin/settings/roles
```

Nên có permission thay vì chỉ role cứng:

```text
product.read
product.publish

order.read
order.confirm
order.cancel

payment.verify
refund.execute

inventory.adjust
```

---

# U. System

## 52. General Settings

```text
/admin/settings/general
```

Ví dụ:

```text
Store name
Support phone
Support email
Default currency
Timezone
```

---

# 53. Commerce Settings

```text
/admin/settings/commerce
```

```text
Payment timeout
Return period
Low stock threshold
Default shipping
```

---

# 54. Feature Flags

```text
/admin/settings/features
```

Ví dụ:

```text
Reviews      ON
Coupons      ON
Returns      OFF
Traceability ON
```

Cực hữu ích khi release feature.

---

# 55. Integration Settings

```text
/admin/settings/integrations
```

Cards:

```text
SePay
Blob Storage
Email
SMS
GHN
GHTK
```

Không nên hiển thị secret trực tiếp.

---

# 56. System Health

```text
/admin/system/health
```

Hiển thị:

```text
API
Database
Blob Storage
Payment provider
Shipping provider
Background worker
```

---

# 57. Failed Jobs

Nếu có background processing:

```text
/admin/system/jobs
```

Có:

```text
Pending
Running
Failed
Completed
```

Cho phép:

```text
Retry
```

với quyền phù hợp.

---

# 58. Payment Reconciliation

Nên có page riêng:

```text
/admin/reconciliation/payments
```

Đây là page rất thực tế mà nhiều hệ thống nhỏ không có.

Ví dụ:

```text
Order      Expected      Received      Result

ORD001     250,000       250,000       ✓
ORD002     350,000       300,000       Mismatch
ORD003     180,000       —             Missing
```

Actions:

```text
Match
Ignore
Investigate
Refund
```

---

# 59. FE sitemap chuẩn tôi đề xuất

Nếu rút gọn lại:

```text
USER
│
├── Home
├── Products
│   ├── Listing
│   ├── Category
│   ├── Detail
│   └── Search
│
├── Producers
│   ├── Listing
│   └── Detail
│
├── Cart
├── Checkout
│
├── Account
│   ├── Profile
│   ├── Address
│   ├── Security
│   └── Notification
│
├── Orders
│   ├── List
│   ├── Detail
│   └── Tracking
│
├── Returns
├── Wishlist
├── Reviews
├── Support
├── Traceability
│
└── CMS
    ├── Articles
    └── Pages
```

Và:

```text
ADMIN
│
├── Dashboard
│
├── Commerce
│   ├── Orders
│   ├── Payments
│   ├── Reconciliation
│   ├── Shipments
│   ├── Returns
│   └── Refunds
│
├── Catalog
│   ├── Products
│   ├── Categories
│   ├── Producers
│   ├── Pricing
│   └── Media
│
├── Inventory
│   ├── Stock
│   ├── Adjustments
│   ├── Purchase Orders
│   ├── Receiving
│   ├── Transfers
│   └── Stocktake
│
├── Marketing
│   ├── Promotions
│   ├── Coupons
│   ├── Reviews
│   └── Questions
│
├── Customers
│   ├── Customer 360
│   └── Support
│
├── Trust
│   ├── Certifications
│   ├── Batches
│   └── Traceability
│
├── Content
│   ├── Pages
│   ├── Articles
│   ├── Banners
│   ├── Navigation
│   └── SEO
│
├── Analytics
│
└── System
    ├── Users
    ├── Roles
    ├── Audit
    ├── Notifications
    ├── Integrations
    ├── Feature Flags
    ├── Jobs
    └── Health
```

---

# 60. Map nhanh FE → BE

| FE Page         | BE Module / Handler                           |
| --------------- | --------------------------------------------- |
| Home            | `GetStorefrontHomeQuery`                      |
| Product List    | `SearchProductsQuery`                         |
| Product Detail  | `GetProductDetailQuery`                       |
| Search          | `SearchProductsHandler`                       |
| Producer        | `GetProducerDetailQuery`                      |
| Cart            | `GetCart`, `AddCartItem`, `UpdateCartItem`    |
| Checkout        | `PreviewCheckoutHandler`, `PlaceOrderHandler` |
| My Orders       | `GetCustomerOrdersQuery`                      |
| Order Detail    | `GetOrderDetailQuery`                         |
| Tracking        | `GetShipmentTrackingQuery`                    |
| Return          | `CreateReturnRequestHandler`                  |
| Wishlist        | `AddWishlistItemHandler`                      |
| Review          | `CreateReviewHandler`                         |
| Trace           | `GetTraceabilityQuery`                        |
| Admin Dashboard | `GetDashboardSummaryQuery`                    |
| Admin Product   | Product handlers                              |
| Admin Order     | Order handlers                                |
| Admin Payment   | Payment/Reconciliation handlers               |
| Admin Return    | Return/Refund handlers                        |
| Admin Inventory | Inventory handlers                            |
| Admin Warehouse | PO/Receiving/Transfer handlers                |
| Admin Promotion | Promotion handlers                            |
| Admin CMS       | CMS handlers                                  |
| Admin Audit     | Audit queries                                 |
| Admin Analytics | Analytics queries                             |

---

# 61. Những page nên làm ngay

Không cần tạo 50 page một lúc.

### User P0

```text
/
products
product detail
search

cart
checkout

orders
order detail
tracking

account
address

return

login/register
```

### Admin P0

```text
dashboard

products
product detail

orders
order detail

payments
payment detail
reconciliation

inventory
shipments

returns
return detail

customers
```

Đây là nhóm biến hệ thống hiện tại thành một luồng commerce có thể vận hành.

---

# 62. P1 tiếp theo

User:

```text
wishlist
reviews
Q&A
notifications
support
producer detail
traceability
```

Admin:

```text
promotion
coupon

review moderation
Q&A

certification
traceability

notification

support
```

---

# 63. P2

Sau khi core ổn:

```text
CMS
SEO
warehouse
analytics
customer segmentation
recommendation
```

---

Điểm quan trọng nhất khi triển khai FE là **không map mỗi API thành một page**. Một page business như `Order Detail` có thể phối hợp nhiều bounded module:

```text
Order
+
Payment
+
Shipment
+
Return
+
Refund
+
Timeline
+
Audit
```

Ngược lại, một module như `Promotion` có thể xuất hiện ở Home, Product Detail, Cart và Checkout.

Vì vậy kiến trúc FE tốt nhất cho dự án này là **feature-oriented**, ví dụ:

```text
src/
├── app/
├── features/
│   ├── catalog/
│   ├── cart/
│   ├── checkout/
│   ├── orders/
│   ├── payment/
│   ├── shipping/
│   ├── returns/
│   ├── reviews/
│   ├── wishlist/
│   └── account/
│
├── admin/
│   ├── catalog/
│   ├── orders/
│   ├── payments/
│   ├── inventory/
│   ├── shipping/
│   ├── returns/
│   ├── marketing/
│   ├── content/
│   └── analytics/
│
└── shared/
```

Cấu trúc này sẽ mở rộng tốt hơn rất nhiều so với tổ chức đơn thuần `pages/components/services`.
