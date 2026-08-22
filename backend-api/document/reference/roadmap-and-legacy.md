# Phạm vi triển khai và legacy

Repository còn code aquaculture/IoT lịch sử. Không coi module đó là hướng phát triển Commerce và không mở rộng nếu yêu cầu không nêu rõ.

Các entity có trong domain nhưng chưa được một controller/handler hiện tại expose hoàn chỉnh phải được ghi là **roadmap/source-only**, không tạo UI/contract giả định. Trước khi triển khai feature mới, kiểm tra theo thứ tự: controller → request/validator/handler → domain entity → EF configuration/migration → test hiện có.

Build/test local chỉ là bằng chứng source. PostgreSQL migration/constraint, Redis lock, CSRF cookie qua BFF, SePay credential/IPN HTTPS và browser E2E là các gate riêng.
