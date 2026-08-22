# Quy tắc duy trì Source of Truth

## Thứ tự ưu tiên

1. Source và effective configuration hiện tại.
2. Tests/migration/current diff.
3. Bộ tài liệu này.
4. Roadmap, ticket và giả định.

## Khi nào cập nhật

Trong cùng change-set khi route, DTO, policy, state transition, entity relationship, migration hoặc external contract thay đổi. Cập nhật `reference/source-status.md` chỉ khi có bằng chứng durable.

## Cách tránh trùng

- Mỗi nghiệp vụ có đúng một file canonical trong `domains/`.
- Codegraph xuyên domain đặt trong `06-codegraphs/`; không copy nguyên contract vào nhiều file.
- Kế hoạch chưa triển khai phải ghi `ROADMAP`, không trộn vào bảng endpoint implemented.
- Không lưu thêm DOCX/PDF/ERD export làm nguồn song song. Artifact xuất bản phải trỏ ngược về Markdown canonical và ghi snapshot.

## Header khi thêm tài liệu

Ghi mục đích, scope, source map, trạng thái bằng chứng và ngày/commit đối chiếu. Link tương đối phải resolve; chạy kiểm tra whitespace/link trước commit.
