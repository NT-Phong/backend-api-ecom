# 🛒 MarketPro (Chợ Xanh) - Backend Development Guide

> Tài liệu hướng dẫn thiết lập môi trường, cấu hình cơ sở dữ liệu đám mây (Azure) và định hướng kiến trúc phát triển cho hệ thống backend của sàn thương mại điện tử **MarketPro (Chợ Xanh)**.

---

## 🏗️ 1. Cấu Trúc Mã Nguồn (Clean Architecture)

Hệ thống được thiết kế theo mô hình Clean Architecture chuẩn của .NET, đảm bảo tính dễ mở rộng và kiểm thử:

```
Source_Ecom/
│
├── backend-api/
│   ├── Core/
│   │   ├── Ecom.Domain/          # Thực thể (Entities), Hằng số (Constants), Interface Repositories
│   │   └── Ecom.Application/     # Logic nghiệp vụ (CQRS - MediatR), DTOs, Mapper, Behaviours
│   │
│   ├── Infrastructure/
│   │   └── Ecom.Infrastructure/  # Entity Framework DbContext, Migrations, Redis Cache, Security Services
│   │
│   ├── Presentation/
│   │   └── Ecom.API/             # Controllers, Web API, Middleware, Configurations
│   │
│   ├── Ecom.sln                  # File Solution quản lý dự án
│   └── Dockerfile                # File Docker build phục vụ deployment
```

---

## 🐳 2. Hướng Dẫn Chạy Môi Trường Local (Docker + Redis)

Để chạy thử database và cache cục bộ tại máy tính của bạn:

### 2.1 Khởi động Docker Compose
Tại thư mục `backend-api/`, khởi chạy lệnh:
```bash
docker compose up -d
```
Hệ thống sẽ dựng sẵn 2 dịch vụ nội bộ kết nối qua mạng `marketpro-workspace`:
- **PostgreSQL 18** (Cổng `5432`) - Username: `postgres`, Password: `1`
- **pgAdmin 4** (Cổng `5433` - Trình quản lý DB trực quan) - Email: `admin@abc.xyz`, Password: `1`

### 2.2 Cấu hình kết nối local trong `appsettings.Development.json`
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=marketpro_dev;Username=postgres;Password=1;",
  "ReadConnection": "Host=localhost;Port=5432;Database=marketpro_dev;Username=postgres;Password=1;",
  "WriteConnection": "Host=localhost;Port=5432;Database=marketpro_dev;Username=postgres;Password=1;",
  "Redis": "localhost:6379,abortConnect=False"
}
```

---

## ☁️ 3. Hướng Dẫn Cấu Hình Azure Database PostgreSQL (Miễn Phí 100%)

Để vận hành thử ứng dụng trên cloud mà không lo tốn phí, hãy làm theo đúng chỉ dẫn thiết lập **Azure Database for PostgreSQL Flexible Server** dưới đây:

### 3.1 Cấu hình phần cứng tối ưu cho ví tiền
- **Workload type:** Chọn chấm tròn **Dev/Test** (để mở khóa cấu hình giá rẻ).
- **Zonal resiliency:** Chọn **Disabled (99.9% SLA)** (tắt nhân bản dự phòng để tránh nhân đôi chi phí).
- **Compute tier:** Chọn **Burstable** (Gói tiết kiệm).
- **Compute size:** Chọn **Standard_B1ms** (1 vCPU, 2 GiB RAM — *Được Azure tặng miễn phí 750 giờ/tháng*).
- **Storage size:** Điều chỉnh về đúng **32 GiB** (*Hạn mức ổ cứng SSD miễn phí tối đa*).

### 3.2 Đặt tài khoản bảo mật
- **Authentication method:** Chọn **PostgreSQL authentication only**.
- **Admin username:** Nhập `devmarketpro`.
- **Password:** Đặt mật khẩu an toàn và lưu lại ngay lập tức.

### 3.3 Thiết lập tường lửa mạng (Networking)
- Chọn **Public access (allowed IP addresses)**.
- Tick chọn **Allow public access from any Azure service within Azure to this server** (cho phép web app trên Azure gọi vào DB).
- Bấm **+ Add current client IP address** để cấp quyền cho máy tính local của bạn gọi trực tiếp vào database.

### 3.4 Nhập chuỗi kết nối vào ứng dụng của bạn
Mở file `backend-api/Presentation/Ecom.API/appsettings.Development.json` và cập nhật:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=marketpro-db-server.postgres.database.azure.com;Port=5432;Database=marketpro_dev;Username=devmarketpro;Password=<MẬT_KHẨU_AZURE_CỦA_BẠN>;SSL Mode=Require;Trust Server Certificate=true;",
  "ReadConnection": "Host=marketpro-db-server.postgres.database.azure.com;Port=5432;Database=marketpro_dev;Username=devmarketpro;Password=<MẬT_KHẨU_AZURE_CỦA_BẠN>;SSL Mode=Require;Trust Server Certificate=true;",
  "WriteConnection": "Host=marketpro-db-server.postgres.database.azure.com;Port=5432;Database=marketpro_dev;Username=devmarketpro;Password=<MẬT_KHẨU_AZURE_CỦA_BẠN>;SSL Mode=Require;Trust Server Certificate=true;"
}
```

---

## 🔐 4. Hướng Dẫn Cấu Hình Biến Môi Trường Trên Azure (Production)

> [!IMPORTANT]
> Khi đưa code lên môi trường chạy thực tế trên Azure App Service, tuyệt đối không sửa trực tiếp mật khẩu vào code. Hãy cấu hình thông qua **Environment variables** trên Azure Portal theo quy tắc gạch dưới kép `__`:

- **Key:** `ConnectionStrings__DefaultConnection` 
  - **Value:** `Host=marketpro-db-server.postgres.database.azure.com;Port=5432;Database=marketpro_dev;Username=devmarketpro;Password=<MẬT_KHẨU_CỦA_BẠN>;SSL Mode=Require;Trust Server Certificate=true;`
- **Key:** `Jwt__SecretKey`
  - **Value:** *Mã khóa JWT bảo mật của bạn*
- **Key:** `Jwt__Issuer`
  - **Value:** `MarketPro.API`
- **Key:** `Jwt__Audience`
  - **Value:** `MarketPro.Client`

---

## ⚡ 5. Chạy Migration Khởi Tạo Database

Để khởi tạo cấu trúc bảng bảo mật và danh sách vai trò mặc định lên cơ sở dữ liệu (local hoặc Azure tùy thuộc vào Connection String đang bật):

Mở Terminal tại thư mục `backend-api/` và chạy lệnh:
```bash
dotnet ef database update --project Infrastructure/Ecom.Infrastructure --startup-project Presentation/Ecom.API
```

---

## 🚀 6. Lộ Trình Phát Triển Tính Năng Nghiệp Vụ Trong Tương Lai

Để xây dựng hệ thống thành một sàn thương mại điện tử đa người bán nông sản thực tế, các phân hệ nghiệp vụ chính cần được triển khai bao gồm:

### 6.1 Phân hệ Tách Đơn Hàng (Split-Order Engine)
- Tự động phân tích sản phẩm trong giỏ hàng theo `VendorId`.
- Tách đơn hàng tổng thành các đơn hàng con (`Sub-Orders`) để từng nhà bán hàng tự xác nhận, đóng gói và giao vận.

### 6.2 Bản Chụp Thông Tin Sản Phẩm (Order Snapshot)
- Bảng `OrderItem` cần có cột lưu trữ trực tiếp: `ProductName`, `SKU`, `PriceAtPurchase`, `ThumbnailUrl` tại thời điểm mua, tránh việc giá thay đổi hoặc sản phẩm bị xóa làm hỏng lịch sử mua hàng của khách.

### 6.3 Xác Thực Đăng Nhập OTP Qua Số Điện Thoại
- Tích hợp cổng SMS API (VietGuys) để gửi mã xác thực.
- Sử dụng Redis để quản lý OTP tạm thời với thời gian hết hạn (TTL) từ 2 - 3 phút.

### 6.4 Kiểm soát Bán Vượt Tồn Kho (Concurrency control)
- Áp dụng khóa lạc quan thông qua `ConcurrencyStamp` trên model Product.
- Kết hợp sử dụng Redis Distributed Lock (`IDistributedLockService`) khi trừ kho hàng tại bước thanh toán để đảm bảo không bị lỗi quá bán (Over-selling) khi có lượng truy cập đồng thời lớn.

# backend-api-ecom
