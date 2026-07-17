# ĐẶC TẢ NGHIỆP VỤ CHỨC NĂNG CHO AI AGENT
## Thạnh Hóa Digital Commerce Platform — Website thương mại điện tử sản phẩm địa phương

> **Mục đích tài liệu:** Cung cấp ngữ cảnh nghiệp vụ thống nhất để AI Agent, BA, Frontend, Backend, QA và đội vận hành hiểu đúng các chức năng cần xây dựng, cách các chức năng liên hệ với nhau, các quy tắc phải bảo toàn và những vấn đề phải làm rõ trước khi tạo code, API, handler, service, model, diagram hoặc cơ sở dữ liệu.
>
> **Phạm vi ưu tiên hiện tại:** Website bán hàng công khai và công cụ quản trị vận hành tối thiểu. Portal riêng cho doanh nghiệp, hợp tác xã và hộ kinh doanh tự quản lý dữ liệu thuộc giai đoạn sau.
>
> **Nguyên tắc quan trọng:** Tài liệu này không tự quyết định mô hình cơ sở dữ liệu vật lý, không áp đặt bảng, cột, khóa hoặc công nghệ lưu trữ. Khi thiết kế dữ liệu, AI Agent phải dựa trên nghiệp vụ trong tài liệu, đặt câu hỏi còn thiếu và trình bày phương án để đội dự án phê duyệt.

---

# 1. Cách AI Agent phải sử dụng tài liệu này

## 1.1. Thứ tự ưu tiên khi có xung đột

1. Yêu cầu mới nhất đã được chủ dự án hoặc người có thẩm quyền xác nhận.
2. Quy định nghiệp vụ và phạm vi trong tài liệu này.
3. Tài liệu kế hoạch chính thức của dự án xã Thạnh Hóa.
4. Hiện trạng source code và các quy tắc trong repository.
5. Tài liệu kiến trúc, API, design system và hướng dẫn kỹ thuật hiện có.
6. Giả định của AI Agent.

AI Agent không được dùng giả định cá nhân để ghi đè một quyết định nghiệp vụ đã xác nhận.

## 1.2. Những việc AI Agent không được tự ý thực hiện

- Không tự thêm chức năng marketplace nhiều nhà bán vào giai đoạn hiện tại.
- Không tự cho doanh nghiệp, HTX hoặc hộ kinh doanh quyền quản lý gian hàng nếu chưa được duyệt.
- Không tự đổi tên, gộp hoặc tách trạng thái nghiệp vụ chỉ vì thuận tiện khi code.
- Không tự tạo dữ liệu doanh nghiệp, sản phẩm, chứng nhận hoặc nguồn gốc không có thật.
- Không tự thiết kế database vật lý rồi xem đó là yêu cầu chính thức.
- Không tự lựa chọn cổng thanh toán, đơn vị vận chuyển hoặc nhà cung cấp bản đồ mà không phân tích ảnh hưởng.
- Không đưa bí mật hệ thống, khóa dịch vụ hoặc thông tin quản trị vào frontend.
- Không đặt tên domain theo tên component template như `ProductDetailsOne`, `BannerOne` hoặc các hậu tố demo.
- Không khôi phục các route demo đã bị loại khỏi source nếu người dùng không yêu cầu.

## 1.3. Đầu ra tối thiểu khi AI Agent xử lý một chức năng

Trước khi tạo code hoặc diagram, AI Agent phải xác định:

- Mục tiêu nghiệp vụ.
- Người sử dụng hoặc hệ thống liên quan.
- Điều kiện bắt đầu.
- Luồng chính.
- Luồng thay thế và lỗi.
- Quy tắc validation.
- Các trạng thái nghiệp vụ.
- Dữ liệu đầu vào và đầu ra ở mức khái niệm.
- Ảnh hưởng đến FE, API, Backend, báo cáo và vận hành.
- Các câu hỏi chưa được xác nhận.
- Tiêu chí nghiệm thu.

---

# 2. Ngữ cảnh và định vị dự án

## 2.1. Định vị

Thạnh Hóa Digital Commerce Platform là nền tảng thương mại điện tử phục vụ bán sản phẩm địa phương, quảng bá hình ảnh sản phẩm và kết nối khách hàng với thị trường. Website phải hỗ trợ hành trình mua hàng hoàn chỉnh, đồng thời tạo niềm tin thông qua thông tin nguồn gốc, đơn vị sản xuất, chứng nhận, hình ảnh, video và các nội dung giới thiệu địa phương.

Dự án không chỉ là trang giới thiệu sản phẩm. Tuy nhiên, trong giai đoạn hiện tại dự án cũng chưa được xem là một marketplace hoàn chỉnh có nhiều nhà bán tự vận hành độc lập.

## 2.2. Mục tiêu nghiệp vụ giai đoạn hiện tại

- Giúp khách hàng tìm thấy sản phẩm địa phương phù hợp.
- Giúp khách hàng hiểu và tin tưởng sản phẩm trước khi mua.
- Cho phép khách hàng đặt hàng và theo dõi đơn hàng thuận tiện.
- Cho phép nhân viên vận hành quản lý sản phẩm, nội dung, đơn hàng và khách hàng.
- Tạo kênh tiếp nhận nhu cầu mua số lượng lớn và hợp tác thương mại.
- Thu thập dữ liệu hoạt động để đánh giá hiệu quả bán hàng và quảng bá.
- Chuẩn bị nền tảng để mở rộng Portal nhà cung cấp ở giai đoạn sau mà không phá vỡ nghiệp vụ hiện tại.

## 2.3. Các nhóm người dùng

| Nhóm người dùng | Vai trò trong giai đoạn hiện tại |
|---|---|
| Khách vãng lai | Xem sản phẩm, tìm kiếm, thêm giỏ, đặt hàng không cần tài khoản nếu được phép, tra cứu đơn hàng |
| Khách hàng có tài khoản | Quản lý hồ sơ, địa chỉ, đơn hàng, yêu thích, đánh giá và mua lại |
| Nhân viên xử lý đơn hàng | Xác nhận, chuẩn bị, cập nhật trạng thái, hỗ trợ khách hàng |
| Nhân viên nội dung | Quản lý sản phẩm, bài viết, banner, hình ảnh và nội dung quảng bá |
| Quản trị viên | Phân quyền nội bộ, cấu hình vận hành, kiểm duyệt và theo dõi báo cáo |
| Đối tác mua số lượng lớn | Gửi yêu cầu báo giá, hợp tác, phân phối hoặc làm đại lý |
| Hệ thống bên ngoài | Thanh toán, vận chuyển, bản đồ, email/SMS/thông báo nếu được tích hợp |

> **Ngoài phạm vi hiện tại:** Tài khoản nhà cung cấp tự tạo sản phẩm, tự xử lý đơn, tự xem doanh thu hoặc tự đối soát.

---

# 3. Nguyên tắc nghiệp vụ xuyên suốt

## 3.1. Tính xác thực

- Mọi nội dung về chứng nhận, OCOP, VietGAP, nguồn gốc hoặc đơn vị sản xuất phải có căn cứ.
- Chỉ hiển thị nhãn xác thực khi trạng thái xác minh cho phép.
- Không dùng nội dung marketing làm thay đổi bản chất thông tin thực tế.
- Khi dữ liệu chưa đầy đủ, hệ thống phải thể hiện rõ là chưa cập nhật thay vì tự suy diễn.

## 3.2. Tính nhất quán

- Giá hiển thị tại các vị trí khác nhau phải cùng nguồn và cùng thời điểm hiệu lực.
- Trạng thái đơn hàng phải có ý nghĩa giống nhau ở trang khách hàng, màn hình quản trị, email và báo cáo.
- Một thuật ngữ nghiệp vụ chỉ nên có một nghĩa trong toàn hệ thống.
- API không được trả trạng thái hoặc mã lỗi mà FE không hiểu hoặc không thể hiển thị.

## 3.3. Bảo toàn lịch sử giao dịch

- Thay đổi tên, giá, hình ảnh hoặc quy cách sản phẩm sau này không được làm sai thông tin của đơn hàng đã đặt.
- Các hành động quan trọng như xác nhận đơn, hủy đơn, hoàn tiền, duyệt đánh giá hoặc xuất bản sản phẩm phải truy vết được.
- Báo cáo phải phân biệt dữ liệu hiện tại và dữ liệu tại thời điểm giao dịch.

## 3.4. Dễ sử dụng

- Quy trình mua hàng phải phù hợp với cả người ít sử dụng công nghệ.
- Form phải dùng ngôn ngữ dễ hiểu, thông báo lỗi gần trường dữ liệu và chỉ rõ cách sửa.
- Website phải sử dụng tốt trên điện thoại, không có cuộn ngang hoặc thành phần bị che.
- Các thao tác quan trọng phải có phản hồi rõ ràng: đang xử lý, thành công, thất bại hoặc cần thử lại.

## 3.5. Khả năng mở rộng

- Không gắn nghiệp vụ cốt lõi vào tên giao diện hoặc component hiện tại.
- Cần phân biệt rõ sản phẩm, quy cách bán, giá, tồn kho và nội dung giới thiệu dù giai đoạn đầu có thể đơn giản.
- Phải giữ khả năng bổ sung nhà cung cấp tự quản lý ở giai đoạn sau mà không làm sai dữ liệu lịch sử.

---

# 4. Bản đồ chức năng tổng thể

1. Quảng bá địa phương và thương hiệu.
2. Khám phá, tìm kiếm và lựa chọn sản phẩm.
3. Thông tin chi tiết, nguồn gốc và niềm tin sản phẩm.
4. Giỏ hàng và mua nhanh.
5. Checkout, giao hàng và thanh toán.
6. Quản lý vòng đời đơn hàng.
7. Tài khoản và trải nghiệm khách hàng.
8. Đánh giá, hỏi đáp và tương tác.
9. Kết nối giao thương B2B.
10. Bản đồ và trải nghiệm địa phương.
11. Nội dung, SEO và truyền thông.
12. Quản trị vận hành.
13. Báo cáo và đo lường hiệu quả.
14. Các chức năng điểm nhấn khi có đầu tư mở rộng.

---

# 5. Đặc tả nhóm quảng bá địa phương và thương hiệu

## 5.1. Trang chủ thương mại địa phương

### Mục tiêu

Tạo điểm vào chính của website, giúp khách nhanh chóng hiểu website bán gì, sản phẩm nào đáng quan tâm và hành động tiếp theo là gì.

### Thành phần nghiệp vụ

- Banner hoặc chiến dịch nổi bật.
- Danh mục sản phẩm chính.
- Sản phẩm nổi bật, sản phẩm mới hoặc sản phẩm OCOP.
- Nội dung về địa phương và câu chuyện sản phẩm.
- Tin tức, sự kiện hoặc chương trình xúc tiến.
- Lời kêu gọi mua hàng, liên hệ hoặc gửi yêu cầu báo giá.

### Luồng chính

1. Khách mở trang chủ.
2. Hệ thống tải cấu hình nội dung đang được xuất bản.
3. Hệ thống ưu tiên hiển thị nội dung còn hiệu lực.
4. Khách chọn danh mục, sản phẩm, bài viết hoặc hành động mua hàng.
5. Hệ thống điều hướng đến trang tương ứng.

### Quy tắc nghiệp vụ

- Không hiển thị chiến dịch đã hết hạn như đang còn hiệu lực.
- Sản phẩm bị ẩn, ngừng bán hoặc không hợp lệ không được xuất hiện trong khối mua hàng.
- Một khu vực phải có phương án hiển thị khi chưa có dữ liệu.
- Banner phải có nội dung thay thế và liên kết hợp lệ.
- Nội dung quảng cáo không được che khuất thông tin mua hàng cốt lõi.

### Vấn đề cần làm rõ trước khi thiết kế API hoặc dữ liệu

- Nội dung trang chủ được sắp xếp thủ công hay theo quy tắc?
- Có cho đặt lịch hiển thị banner và chiến dịch không?
- Sản phẩm nổi bật do nhân viên chọn hay tính tự động?
- Khi một sản phẩm hết hàng, có tiếp tục quảng bá không?
- Có cần cá nhân hóa trang chủ theo khách hàng hay không?

### Tiêu chí nghiệm thu

- Trang chủ hiển thị đúng nội dung đang hoạt động.
- Mọi liên kết chính đều dẫn đến nội dung hợp lệ.
- Có trạng thái loading, empty và error.
- Hoạt động tốt trên mobile và desktop.

---

## 5.2. Không gian giới thiệu xã Thạnh Hóa

### Mục tiêu

Giới thiệu hình ảnh, tiềm năng, con người và định hướng phát triển để tăng uy tín cho website và sản phẩm địa phương.

### Nội dung dự kiến

- Giới thiệu chung.
- Điều kiện tự nhiên và thế mạnh địa phương.
- Hình ảnh, video, cột mốc hoặc thành tựu.
- Định hướng phát triển và xúc tiến thương mại.
- Liên hệ cơ quan hoặc đầu mối hỗ trợ.

### Quy tắc

- Nội dung phải được duyệt trước khi công khai.
- Thông tin hành chính và số liệu phải có nguồn xác nhận.
- Video hoặc tài liệu bên ngoài phải xử lý trường hợp liên kết hỏng.
- Không sử dụng thông tin nhà đầu tư hoặc dự án chưa được công bố.

### Vấn đề cần làm rõ

- Ai có quyền duyệt nội dung chính thức?
- Có cần quản lý phiên bản nội dung không?
- Có cần đa ngôn ngữ không?
- Có yêu cầu lưu trữ tài liệu đính kèm hoặc chỉ hiển thị nội dung web?

---

## 5.3. Gian hàng sản phẩm đặc trưng và OCOP

### Mục tiêu

Tạo khu vực nhận diện các sản phẩm có giá trị nổi bật hoặc đã được công nhận.

### Quy tắc

- Sản phẩm chỉ được gắn nhãn OCOP khi có dữ liệu chứng minh phù hợp.
- Cần phân biệt “sản phẩm tiêu biểu” do chiến dịch quảng bá và “sản phẩm được chứng nhận”.
- Thời hạn chứng nhận hoặc trạng thái còn hiệu lực phải được xem xét khi hiển thị.
- Không để nhãn tiếp thị gây hiểu nhầm thành nhãn chứng nhận.

### Vấn đề khi làm diagram và dữ liệu

- Quan hệ giữa sản phẩm và chứng nhận có thể thay đổi theo thời gian.
- Một sản phẩm có thể có nhiều chứng nhận.
- Chứng nhận có thể áp dụng cho sản phẩm, cơ sở hoặc quy trình; không được mặc định tất cả giống nhau.
- Cần xác định cách xử lý chứng nhận hết hạn, bị thu hồi hoặc đang chờ xác minh.

---

# 6. Đặc tả nhóm khám phá và lựa chọn sản phẩm

## 6.1. Danh mục sản phẩm

### Mục tiêu

Cho phép khách duyệt sản phẩm theo cấu trúc dễ hiểu và phù hợp cách gọi của người dân.

### Luồng chính

1. Khách mở trang danh mục hoặc cửa hàng.
2. Hệ thống tải danh mục khả dụng và danh sách sản phẩm.
3. Khách chọn danh mục, lọc hoặc sắp xếp.
4. Hệ thống cập nhật kết quả và trạng thái URL phù hợp.
5. Khách mở chi tiết sản phẩm.

### Quy tắc

- Chỉ hiển thị danh mục được xuất bản.
- Danh mục không có sản phẩm có thể ẩn hoặc hiển thị theo cấu hình; không tự quyết định.
- Một sản phẩm có thể thuộc một hoặc nhiều nhóm hiển thị, nhưng cần xác định nhóm chính cho điều hướng và SEO.
- Khi đổi cấu trúc danh mục, cần bảo toàn liên kết cũ nếu đã công khai.

### Vấn đề cần làm rõ

- Có hỗ trợ danh mục nhiều cấp hay chỉ một cấp?
- Sản phẩm có được thuộc nhiều danh mục không?
- Danh mục có thứ tự thủ công không?
- URL và slug thay đổi được không? Nếu thay đổi thì xử lý chuyển hướng thế nào?

### Các tình huống lỗi

- Không có sản phẩm phù hợp.
- Danh mục không tồn tại hoặc đã bị ẩn.
- Bộ lọc không hợp lệ.
- Trang phân trang vượt quá số trang hiện có.

---

## 6.2. Tìm kiếm sản phẩm

### Mục tiêu

Giúp khách tìm sản phẩm bằng từ khóa tự nhiên, tên sản phẩm hoặc nhu cầu.

### Hành vi dự kiến

- Tìm theo tên sản phẩm.
- Có thể mở rộng tìm theo danh mục, thương hiệu địa phương, đơn vị sản xuất hoặc từ khóa mô tả.
- Gợi ý khi nhập nếu dữ liệu và hiệu năng cho phép.
- Hiển thị kết quả rõ ràng khi không tìm thấy.

### Quy tắc

- Không hiển thị sản phẩm không còn công khai.
- Tìm kiếm phải xử lý dấu tiếng Việt hợp lý.
- Từ khóa không được tạo truy vấn nguy hiểm hoặc gây quá tải.
- Cần quy định độ dài tối thiểu, tối đa và giới hạn tần suất.
- Thứ tự kết quả phải có tiêu chí rõ ràng.

### Vấn đề cần làm rõ

- Tìm kiếm sử dụng cơ chế đơn giản hay hệ thống tìm kiếm chuyên dụng?
- Có cần gợi ý sửa lỗi chính tả không?
- Có cần lưu từ khóa tìm kiếm để báo cáo không?
- Có cần tìm kiếm theo giọng nói không?

---

## 6.3. Bộ lọc và sắp xếp

### Mục tiêu

Giúp khách thu hẹp danh sách theo nhu cầu thực tế.

### Tiêu chí có thể hỗ trợ

- Khoảng giá.
- Danh mục.
- Chứng nhận.
- Trạng thái còn hàng.
- Sản phẩm mới hoặc nổi bật.
- Quy cách hoặc đặc tính phù hợp từng ngành hàng.

### Quy tắc

- Bộ lọc chỉ được dùng những trường đã được công bố và có dữ liệu đáng tin cậy.
- Kết hợp nhiều bộ lọc phải có logic rõ ràng.
- URL nên lưu trạng thái lọc để chia sẻ và quay lại.
- Không trả kết quả sai do dữ liệu thiếu; cần xác định cách xử lý giá trị chưa có.

### Vấn đề khi thiết kế API

- Cần thống nhất tên tham số, cách truyền nhiều giá trị và giá trị mặc định.
- Phải whitelist trường sắp xếp.
- Phân trang phải ổn định khi dữ liệu thay đổi.
- Cần xác định có trả số lượng sản phẩm theo từng bộ lọc hay không.

---

## 6.4. Sản phẩm nổi bật, sản phẩm mới và sản phẩm liên quan

### Mục tiêu

Tăng khả năng khám phá và giá trị đơn hàng.

### Quy tắc

- “Mới” cần có định nghĩa về thời gian hoặc ngày xuất bản.
- “Nổi bật” cần biết do nhân viên chọn hay tính theo dữ liệu.
- Sản phẩm liên quan phải có nguyên tắc: cùng danh mục, cùng nhu cầu, thường mua cùng hoặc chọn thủ công.
- Không gợi ý sản phẩm bị ẩn hoặc không thể mua.

### Rủi ro

- Dùng doanh số làm tiêu chí có thể ưu tiên sản phẩm cũ và làm sản phẩm mới khó tiếp cận.
- Gợi ý sai ngữ cảnh có thể giảm niềm tin.
- Nếu dữ liệu ít, cần fallback hợp lý.

---

# 7. Đặc tả nhóm thông tin và niềm tin sản phẩm

## 7.1. Trang chi tiết sản phẩm

### Mục tiêu

Cung cấp đủ thông tin để khách hiểu, tin tưởng và quyết định mua.

### Nội dung cốt lõi

- Tên sản phẩm.
- Hình ảnh và video.
- Giá bán hoặc giá tham khảo theo chính sách.
- Quy cách, khối lượng, đơn vị tính hoặc biến thể.
- Trạng thái còn hàng.
- Mô tả và công dụng.
- Hướng dẫn sử dụng, bảo quản và cảnh báo nếu cần.
- Đơn vị sản xuất.
- Câu chuyện sản phẩm.
- Chứng nhận và nguồn gốc.
- Đánh giá, hỏi đáp và sản phẩm liên quan.

### Luồng mua

1. Khách mở sản phẩm bằng URL hợp lệ.
2. Hệ thống tải thông tin công khai.
3. Khách chọn quy cách hoặc biến thể nếu có.
4. Hệ thống cập nhật giá, tồn kho và thông tin liên quan.
5. Khách chọn số lượng.
6. Khách thêm giỏ hoặc mua ngay.

### Quy tắc

- Không cho mua khi sản phẩm chưa xuất bản, ngừng bán hoặc không đủ điều kiện.
- Nếu sản phẩm có biến thể, giá và tồn kho phải theo biến thể được chọn.
- Không hiển thị một mức giá gây hiểu nhầm nếu có nhiều mức giá.
- Giới hạn số lượng phải được kiểm tra lại ở Backend khi đặt hàng.
- Ảnh đại diện và thứ tự media phải nhất quán.
- Nội dung nhạy cảm như cảnh báo dị ứng hoặc điều kiện bảo quản không được ẩn dưới phần ít quan trọng.

### Trường hợp đặc biệt

- Sản phẩm còn thông tin nhưng tạm hết hàng.
- Sản phẩm đã ngừng bán nhưng cần giữ URL để tham khảo.
- Sản phẩm chưa có giá và chỉ tiếp nhận báo giá.
- Sản phẩm bán theo mùa hoặc có ngày bắt đầu bán.
- Sản phẩm có giới hạn mua tối thiểu hoặc tối đa.

### Câu hỏi bắt buộc trước khi xây model/API

- Khái niệm “sản phẩm” và “quy cách bán” khác nhau thế nào?
- Giá áp dụng cho sản phẩm hay từng quy cách?
- Tồn kho được theo dõi đến mức nào?
- Có cho phép sản phẩm không có tồn kho nhưng vẫn đặt trước không?
- Có cần giá khuyến mại theo thời gian không?
- Có cần quản lý lô, hạn sử dụng hoặc ngày thu hoạch không?

---

## 7.2. Câu chuyện sản phẩm

### Mục tiêu

Tăng giá trị cảm xúc và thương hiệu thông qua nguồn gốc, con người và quá trình tạo ra sản phẩm.

### Quy tắc

- Phân biệt nội dung câu chuyện với thông tin xác thực.
- Không sử dụng tuyên bố về sức khỏe hoặc chất lượng vượt quá chứng cứ.
- Nội dung cần được duyệt và có người chịu trách nhiệm.
- Hình ảnh nhân vật phải có quyền sử dụng phù hợp.

### Vấn đề cần làm rõ

- Câu chuyện là một phần của sản phẩm hay một nội dung độc lập có thể tái sử dụng?
- Có quản lý tác giả, phiên bản và lịch xuất bản không?
- Có hỗ trợ video dọc, phụ đề hoặc nội dung đa ngôn ngữ không?

---

## 7.3. Thông tin đơn vị sản xuất

### Mục tiêu

Giúp khách biết sản phẩm do ai sản xuất và tăng tính xác thực.

### Nội dung có thể hiển thị

- Tên đơn vị.
- Giới thiệu ngắn.
- Địa chỉ hoặc khu vực.
- Thông tin liên hệ công khai.
- Hình ảnh, video hoặc vị trí bản đồ.
- Danh sách sản phẩm liên quan.

### Quy tắc

- Không công khai dữ liệu cá nhân không được phép.
- Địa chỉ hiển thị công khai có thể khác địa chỉ lưu trữ nội bộ.
- Cần phân biệt nhà sản xuất, nhà cung cấp, đơn vị đóng gói và đơn vị bán hàng nếu nghiệp vụ phát sinh.
- Giai đoạn hiện tại đơn vị sản xuất chưa có quyền tự quản lý dữ liệu.

### Vấn đề cho diagram và mô hình quan hệ

- Một sản phẩm có thể có nhiều vai trò đơn vị khác nhau.
- Một đơn vị có thể sản xuất nhiều sản phẩm.
- Quan hệ này có thể thay đổi theo thời gian.
- Không nên mặc định một trường duy nhất giải quyết mọi vai trò trong chuỗi cung ứng.

---

## 7.4. Chứng nhận và truy xuất nguồn gốc

### Mục tiêu

Cung cấp bằng chứng giúp khách đánh giá tính minh bạch và chất lượng sản phẩm.

### Thành phần nghiệp vụ

- Loại chứng nhận.
- Cơ quan cấp.
- Mã hoặc số chứng nhận nếu được phép công khai.
- Ngày cấp, ngày hết hạn.
- Phạm vi áp dụng.
- Tệp hoặc hình ảnh minh chứng.
- Trạng thái xác minh nội bộ.
- Mã QR hoặc đường dẫn truy xuất.

### Quy tắc

- Không hiển thị nhãn chứng nhận khi dữ liệu chưa được duyệt.
- Chứng nhận hết hạn không được hiển thị như còn hiệu lực.
- Cần xác định rõ chứng nhận áp dụng cho sản phẩm, đơn vị, vùng trồng hoặc quy trình.
- Tệp minh chứng phải được bảo vệ nếu có thông tin không được công khai.
- QR phải dẫn đến trang ổn định và không phụ thuộc vào phiên đăng nhập nếu dành cho người tiêu dùng.

### Vấn đề cần giải quyết khi thiết kế

- QR do hệ thống tạo hay lấy từ hệ thống truy xuất bên ngoài?
- Có cần kiểm tra tính hợp lệ của liên kết bên ngoài định kỳ không?
- Trạng thái xác minh gồm những bước nào?
- Ai được quyền xác nhận hoặc thu hồi thông tin?
- Có lưu lịch sử thay đổi chứng nhận không?

---

# 8. Đặc tả giỏ hàng và mua nhanh

## 8.1. Giỏ hàng

### Mục tiêu

Lưu các sản phẩm khách có ý định mua và chuẩn bị cho checkout.

### Luồng chính

1. Khách chọn quy cách và số lượng.
2. Hệ thống kiểm tra dữ liệu đầu vào sơ bộ.
3. Sản phẩm được thêm hoặc cập nhật trong giỏ.
4. Hệ thống hiển thị giá dự kiến và tổng tạm tính.
5. Khi khách mở giỏ, hệ thống kiểm tra lại giá, trạng thái bán và tồn kho.

### Quy tắc

- Một mục giỏ phải xác định đúng quy cách bán, không chỉ sản phẩm chung.
- Thêm cùng một quy cách có thể tăng số lượng thay vì tạo dòng mới.
- Giỏ của khách vãng lai và khách đăng nhập cần có chính sách hợp nhất rõ ràng.
- Giá trong giỏ chỉ là dự kiến cho đến khi checkout xác nhận.
- Không cho số lượng âm, bằng 0 hoặc vượt giới hạn.
- Khi sản phẩm thay đổi giá hoặc hết hàng, phải thông báo trước khi đặt.

### Trường hợp lỗi

- Sản phẩm bị ngừng bán sau khi thêm giỏ.
- Giá thay đổi.
- Số lượng vượt tồn kho.
- Quy cách bị xóa hoặc không còn khả dụng.
- Khuyến mại hết hạn.
- Giỏ hết hạn hoặc không thể đồng bộ.

### Vấn đề cần làm rõ

- Giỏ khách vãng lai được lưu bao lâu?
- Có đồng bộ giỏ nhiều thiết bị không?
- Có giữ tồn kho khi thêm giỏ không? Thông thường không nên mặc định.
- Có giới hạn số dòng hoặc tổng số lượng không?
- Có cho đặt sản phẩm báo giá chung với sản phẩm có giá không?

---

## 8.2. Mua ngay

### Mục tiêu

Giảm số bước cho khách muốn mua một sản phẩm nhanh chóng.

### Quy tắc

- Mua ngay không được bỏ qua kiểm tra giá, tồn kho và điều kiện bán.
- Cần xác định mua ngay có dùng giỏ hiện tại hay tạo phiên checkout riêng.
- Không được vô tình xóa giỏ hiện có của khách.
- Khi checkout thất bại, khách vẫn có thể quay lại sản phẩm hoặc giỏ.

---

# 9. Đặc tả checkout, giao hàng và thanh toán

## 9.1. Checkout

### Mục tiêu

Thu thập đủ thông tin để tạo đơn chính xác nhưng giữ quy trình đơn giản.

### Thông tin cần xử lý

- Thông tin người nhận.
- Số điện thoại.
- Địa chỉ giao hàng.
- Ghi chú.
- Phương thức giao hàng.
- Phương thức thanh toán.
- Mã ưu đãi nếu có.
- Xác nhận các điều khoản cần thiết.

### Luồng chính

1. Khách mở checkout từ giỏ hoặc mua ngay.
2. Hệ thống kiểm tra lại toàn bộ sản phẩm.
3. Khách đăng nhập, tiếp tục với tài khoản hiện có hoặc đặt không cần tài khoản theo chính sách.
4. Khách nhập/chọn địa chỉ.
5. Hệ thống tính phí giao hàng và thời gian dự kiến nếu có.
6. Khách chọn thanh toán.
7. Hệ thống tính tổng cuối cùng.
8. Khách xác nhận.
9. Backend tạo đơn một lần duy nhất.
10. Hệ thống hiển thị kết quả và mã đơn.

### Quy tắc

- Tất cả giá, ưu đãi và tồn kho phải được kiểm tra ở server.
- Không tin giá hoặc tổng tiền gửi từ frontend.
- Tạo đơn phải chống gửi lặp do khách bấm nhiều lần hoặc mạng chậm.
- Thông tin đơn phải lưu giá trị tại thời điểm đặt.
- Nếu có lỗi, phải nói rõ khách cần sửa gì và giữ lại dữ liệu hợp lệ đã nhập.
- Không tạo đơn thành công nếu không xác định được phương thức giao/nhận hàng hợp lệ.

### Vấn đề cần làm rõ

- Có cho guest checkout không?
- Địa bàn giao hàng trong giai đoạn đầu là đâu?
- Phí giao hàng cố định, theo khu vực hay tích hợp đối tác?
- Có cho nhận tại điểm bán không?
- Có cho đặt trước hoặc chọn ngày giao không?
- Mã đơn được tạo theo nguyên tắc nào và có được công khai không?

---

## 9.2. Địa chỉ giao hàng

### Quy tắc

- Phải phân biệt địa chỉ lưu trong tài khoản và địa chỉ snapshot của đơn.
- Thay đổi sổ địa chỉ sau này không làm thay đổi đơn cũ.
- Cần xác định cấu trúc địa chỉ phù hợp Việt Nam và thay đổi hành chính.
- Số điện thoại phải được chuẩn hóa nhưng vẫn lưu giá trị người dùng nhập khi cần truy vết.
- Địa chỉ cần có ghi chú chỉ đường nếu phục vụ khu vực nông thôn.

### Vấn đề cần làm rõ

- Có cần tọa độ bản đồ cho địa chỉ giao không?
- Có kiểm tra địa chỉ qua nhà cung cấp bản đồ không?
- Có cho lưu nhiều người nhận không?
- Khi địa giới hành chính thay đổi, dữ liệu cũ xử lý thế nào?

---

## 9.3. Thanh toán khi nhận hàng và chuyển khoản

### COD

- Đơn có thể được tạo ở trạng thái chờ xác nhận.
- Cần quy định giới hạn giá trị COD nếu có.
- Có thể cần xác minh số điện thoại với đơn giá trị cao.

### Chuyển khoản

- Cần hiển thị thông tin chuyển khoản chính xác.
- Cần quy định cách khách cung cấp nội dung chuyển khoản.
- Trạng thái “đã chuyển khoản” do khách báo không đồng nghĩa “đã xác nhận tiền”.
- Cần có quy trình đối soát và xử lý chuyển thiếu/thừa.

### Vấn đề khi tích hợp thanh toán trực tuyến sau này

- Phải tách trạng thái đơn hàng và trạng thái thanh toán.
- Callback/webhook phải được xác minh.
- Tạo thanh toán và xác nhận thanh toán phải chống lặp.
- Không tin trạng thái thanh toán từ frontend.
- Cần chính sách hoàn tiền, thất bại, hết hạn và giao dịch treo.

---

## 9.4. Mã giảm giá và khuyến mại

### Mục tiêu

Hỗ trợ chiến dịch bán hàng mà không làm sai giá hoặc gây lạm dụng.

### Quy tắc cần xác nhận

- Thời gian hiệu lực.
- Giá trị đơn tối thiểu.
- Giới hạn lượt dùng toàn hệ thống và theo khách hàng.
- Danh mục hoặc sản phẩm áp dụng.
- Có cộng dồn với khuyến mại khác không.
- Xử lý hoàn/hủy đơn có trả lại lượt dùng không.
- Làm tròn tiền và phân bổ giảm giá vào từng dòng đơn thế nào.

### Rủi ro

- Áp dụng cùng mã nhiều lần do request lặp.
- Khách thay đổi giỏ sau khi mã đã được kiểm tra.
- Mã hết hạn giữa lúc checkout.
- Báo cáo doanh thu không phân biệt doanh thu trước và sau giảm giá.

---

# 10. Đặc tả vòng đời đơn hàng

## 10.1. Mục tiêu

Theo dõi toàn bộ quá trình từ khi khách gửi yêu cầu mua đến khi hoàn thành, hủy hoặc xử lý sự cố.

## 10.2. Trạng thái nghiệp vụ tham khảo

Các trạng thái sau chỉ là ngôn ngữ nghiệp vụ cần được đội dự án xác nhận trước khi xây enum chính thức:

- Mới tạo / chờ xác nhận.
- Đã xác nhận.
- Đang chuẩn bị hàng.
- Sẵn sàng giao hoặc chờ đơn vị vận chuyển.
- Đang giao.
- Hoàn thành.
- Yêu cầu hủy.
- Đã hủy.
- Giao không thành công.
- Chờ xử lý hoàn tiền hoặc khiếu nại nếu có.

> Không tự gộp trạng thái thanh toán vào trạng thái đơn hàng.

## 10.3. Quy tắc chuyển trạng thái

- Mỗi bước chuyển phải xác định người hoặc hệ thống được phép thực hiện.
- Không cho chuyển tùy ý từ mọi trạng thái sang mọi trạng thái.
- Phải lưu lý do khi hủy, từ chối hoặc thất bại.
- Cập nhật trạng thái phải chống xung đột khi nhiều nhân viên thao tác cùng lúc.
- Khách chỉ nhìn thấy trạng thái và thông điệp phù hợp; không nhất thiết thấy toàn bộ trạng thái nội bộ.
- Thông báo cho khách phải dựa trên sự kiện đã được ghi nhận thành công.

## 10.4. Xác nhận đơn

### Kiểm tra trước khi xác nhận

- Thông tin liên hệ hợp lệ.
- Sản phẩm còn khả dụng.
- Giá và ưu đãi hợp lệ.
- Phương thức giao hàng thực hiện được.
- Thanh toán đáp ứng điều kiện tương ứng.

### Trường hợp cần hỗ trợ thủ công

- Không liên hệ được khách.
- Sản phẩm thiếu hàng.
- Địa chỉ ngoài khu vực.
- Chuyển khoản chưa xác minh.
- Đơn có dấu hiệu bất thường.

## 10.5. Hủy đơn

### Cần làm rõ

- Khách được hủy đến trạng thái nào?
- Sau thời điểm nào phải liên hệ nhân viên?
- Hủy một phần có được hỗ trợ không?
- Tồn kho được hoàn lại ở bước nào?
- Mã giảm giá hoặc điểm ưu đãi xử lý thế nào?
- Nếu đã thanh toán, hoàn tiền theo quy trình nào?

## 10.6. Giao hàng thất bại

- Ghi nhận lý do thất bại.
- Xác định số lần giao lại tối đa.
- Xử lý phí phát sinh.
- Liên hệ lại khách hàng.
- Quyết định hủy hoặc lên lịch giao lại.
- Không đánh dấu hoàn thành nếu chưa có căn cứ giao thành công.

## 10.7. Tra cứu đơn hàng

### Đối với khách có tài khoản

- Xem danh sách và chi tiết đơn thuộc tài khoản.
- Không được truy cập đơn của người khác.

### Đối với khách vãng lai

- Có thể tra cứu bằng mã đơn và thông tin xác minh bổ sung.
- Không trả quá nhiều dữ liệu nếu chỉ biết mã đơn.
- Cần giới hạn số lần thử để tránh dò thông tin.

---

# 11. Đặc tả tài khoản khách hàng

## 11.1. Đăng ký

### Mục tiêu

Tạo tài khoản để lưu lịch sử mua hàng và cá nhân hóa trải nghiệm.

### Quy tắc

- Xác định email, số điện thoại hoặc cả hai là định danh đăng nhập.
- Không tạo nhiều tài khoản trùng định danh đã xác minh nếu chính sách không cho phép.
- Mật khẩu phải tuân thủ chính sách bảo mật.
- Thông báo lỗi không được làm lộ tài khoản tồn tại trong các luồng nhạy cảm.
- Cần chính sách xác minh và khôi phục tài khoản.

### Vấn đề cần làm rõ

- Có bắt buộc xác minh OTP không?
- Có đăng nhập Google/Facebook/Zalo không?
- Có cho hợp nhất tài khoản guest order với tài khoản mới không?
- Có cho đổi số điện thoại chính không?

## 11.2. Đăng nhập và phiên làm việc

- Có cơ chế hết hạn phiên.
- Đăng xuất phải vô hiệu hóa trạng thái phù hợp.
- Không lưu token tùy tiện trong component.
- Cần xử lý tài khoản khóa, chưa xác minh hoặc bị hạn chế.
- Các thao tác nhạy cảm có thể yêu cầu xác thực lại.

## 11.3. Hồ sơ và sổ địa chỉ

- Khách chỉ sửa dữ liệu của mình.
- Cần lưu lịch sử thay đổi nếu có yêu cầu pháp lý/vận hành.
- Xóa địa chỉ khỏi sổ không làm mất địa chỉ trong đơn cũ.
- Cần xác định địa chỉ mặc định và xử lý khi địa chỉ mặc định bị xóa.

## 11.4. Quên mật khẩu

- Không tiết lộ tài khoản có tồn tại hay không.
- Mã xác minh có thời hạn và chỉ dùng một lần.
- Giới hạn số lần gửi và số lần thử.
- Sau khi đổi mật khẩu, cần xác định có thu hồi phiên cũ không.

---

# 12. Đặc tả yêu thích, đánh giá và hỏi đáp

## 12.1. Danh sách yêu thích

### Quy tắc

- Xác định guest có thể lưu tạm hay bắt buộc đăng nhập.
- Một sản phẩm chỉ xuất hiện một lần trong danh sách.
- Sản phẩm ngừng bán vẫn có thể hiển thị nhưng không cho mua; cần thông báo rõ.
- Không coi số lượt yêu thích là số người mua.

## 12.2. Đánh giá sản phẩm

### Mục tiêu

Tăng niềm tin bằng trải nghiệm thực tế của khách hàng.

### Quy tắc cần xác nhận

- Chỉ người đã mua mới được đánh giá hay mọi tài khoản đều được?
- Một đơn hàng/sản phẩm được đánh giá bao nhiêu lần?
- Có cho chỉnh sửa đánh giá không?
- Nội dung có cần duyệt trước khi công khai không?
- Xử lý nội dung vi phạm, spam hoặc thông tin cá nhân thế nào?
- Điểm trung bình có tính đánh giá ẩn hoặc bị từ chối không?

### Trạng thái có thể cần

- Chờ duyệt.
- Đã công khai.
- Bị từ chối.
- Bị ẩn sau khi công khai.

### Vấn đề cho báo cáo

- Điểm trung bình cần ổn định khi đánh giá thay đổi.
- Phải phân biệt số đánh giá và số đơn hàng.
- Không cho nhân viên sửa nội dung đánh giá của khách mà không lưu dấu vết.

## 12.3. Hỏi đáp sản phẩm

- Khách gửi câu hỏi về sản phẩm.
- Nhân viên trả lời hoặc chuyển chuyên môn.
- Cần kiểm duyệt trước khi công khai.
- Không công khai thông tin liên hệ cá nhân trong câu hỏi.
- Câu trả lời phải xác định người chịu trách nhiệm và thời điểm cập nhật.

---

# 13. Đặc tả kết nối giao thương B2B

## 13.1. Yêu cầu báo giá số lượng lớn

### Mục tiêu

Tiếp nhận nhu cầu của đại lý, doanh nghiệp, nhà phân phối hoặc đơn vị thu mua.

### Thông tin nghiệp vụ

- Người liên hệ và tổ chức.
- Sản phẩm quan tâm.
- Số lượng và quy cách.
- Thời gian cần hàng.
- Khu vực giao nhận.
- Yêu cầu chứng nhận, đóng gói hoặc hóa đơn.
- Ghi chú và kênh liên hệ mong muốn.

### Luồng chính

1. Đối tác gửi yêu cầu.
2. Hệ thống tạo mã tiếp nhận.
3. Hệ thống thông báo cho bộ phận phụ trách.
4. Nhân viên kiểm tra và liên hệ.
5. Nhân viên cập nhật trạng thái xử lý.
6. Kết quả được ghi nhận phục vụ theo dõi và báo cáo.

### Trạng thái tham khảo

- Mới tiếp nhận.
- Đang xác minh.
- Đang chuẩn bị báo giá.
- Đã gửi báo giá.
- Đang thương lượng.
- Thành công.
- Không phù hợp.
- Đã đóng.

### Quy tắc

- Không coi yêu cầu báo giá là đơn hàng.
- Báo giá có thể có thời hạn và điều kiện riêng.
- Cần phân quyền xem thông tin đối tác.
- Mọi thay đổi quan trọng cần lịch sử.
- Không tự động công khai giá B2B nếu chưa được phép.

### Vấn đề cần làm rõ

- Báo giá được tạo trong hệ thống hay chỉ quản lý trạng thái liên hệ?
- Có cần xuất PDF, gửi email và lưu phiên bản không?
- Có chuyển báo giá thành đơn hàng không?
- Ai chịu trách nhiệm và có SLA phản hồi không?

---

## 13.2. Đăng ký đại lý hoặc hợp tác

### Mục tiêu

Thu hút đối tác phân phối, điểm bán và các cơ hội hợp tác thương mại.

### Quy tắc

- Biểu mẫu phải tách rõ nhu cầu đại lý, phân phối, truyền thông, đầu tư hoặc hợp tác khác.
- Dữ liệu doanh nghiệp phải được bảo vệ.
- Cần quy trình tiếp nhận, phân công, phản hồi và đóng hồ sơ.
- Không tự động xem người gửi biểu mẫu là đối tác đã được phê duyệt.

---

# 14. Đặc tả bản đồ và trải nghiệm địa phương

## 14.1. Bản đồ sản phẩm và cơ sở sản xuất

### Mục tiêu

Cho phép khách và đối tác khám phá vị trí sản phẩm, vùng sản xuất hoặc điểm liên quan.

### Hành vi dự kiến

- Xem điểm trên bản đồ.
- Lọc theo loại địa điểm hoặc sản phẩm.
- Mở thông tin tóm tắt.
- Chuyển đến trang sản phẩm/đơn vị.
- Mở chỉ đường qua dịch vụ bản đồ phù hợp.

### Quy tắc

- Chỉ công khai tọa độ đã được phép.
- Có thể cần làm mờ hoặc chỉ hiển thị khu vực thay vì vị trí chính xác.
- Không để bản đồ là cách duy nhất truy cập nội dung; cần danh sách thay thế.
- Xử lý điểm trùng hoặc nhiều điểm gần nhau.
- Dữ liệu bản đồ phải có trạng thái xác minh.

### Vấn đề khi thiết kế tích hợp

- Chọn nhà cung cấp bản đồ ảnh hưởng chi phí, điều khoản và hạn mức.
- Cần xác định địa chỉ, tọa độ và dữ liệu chỉ đường là nguồn nào.
- Phải xử lý trường hợp API bản đồ lỗi hoặc hết hạn mức.
- Cần xem xét quyền riêng tư và an toàn của cơ sở nhỏ/hộ gia đình.

---

# 15. Đặc tả nội dung, tin tức và SEO

## 15.1. Tin tức và xúc tiến thương mại

### Mục tiêu

Cập nhật chính sách, sự kiện, hội chợ, kết nối cung cầu và hoạt động địa phương.

### Quy tắc

- Có trạng thái bản nháp, chờ duyệt, đã xuất bản và ẩn nếu cần.
- Có thời điểm xuất bản và người duyệt.
- URL phải ổn định.
- Nội dung gỡ bỏ cần xử lý liên kết cũ phù hợp.
- Ảnh phải có mô tả thay thế.
- Nội dung trích dẫn cần ghi nguồn.

## 15.2. Quản lý banner và chiến dịch

- Có ngày bắt đầu và kết thúc.
- Có thứ tự ưu tiên.
- Có phạm vi hiển thị theo trang hoặc vị trí.
- Liên kết đích phải hợp lệ.
- Cần xử lý nhiều banner cùng thời điểm.
- Không cho banner không có ảnh phù hợp với mobile.

## 15.3. SEO

### Nội dung cần thống nhất

- Tiêu đề và mô tả trang.
- URL thân thiện.
- Canonical và chuyển hướng khi đổi slug.
- Dữ liệu chia sẻ mạng xã hội.
- Sitemap và trạng thái index.
- Dữ liệu có cấu trúc nếu phù hợp.

### Rủi ro

- Trùng nội dung giữa sản phẩm, danh mục và bài viết.
- URL thay đổi làm mất traffic.
- Công khai trang chưa hoàn thiện.
- Index dữ liệu cá nhân hoặc trang quản trị.

---

# 16. Đặc tả quản trị vận hành

## 16.1. Bảng điều hành

### Mục tiêu

Cung cấp cái nhìn nhanh về hoạt động cần xử lý, không thay thế báo cáo chuyên sâu.

### Thông tin có thể hiển thị

- Đơn mới cần xác nhận.
- Đơn đang giao hoặc có vấn đề.
- Doanh thu theo kỳ.
- Sản phẩm sắp hết hàng nếu có quản lý tồn.
- Yêu cầu báo giá mới.
- Nội dung hoặc đánh giá chờ duyệt.

### Quy tắc

- Chỉ số phải có định nghĩa rõ ràng.
- Thời gian và múi giờ phải thống nhất.
- Số liệu dashboard có thể cập nhật trễ; cần thể hiện thời điểm cập nhật.
- Người dùng chỉ xem dữ liệu theo quyền.

---

## 16.2. Quản lý sản phẩm

### Luồng chuẩn

1. Tạo bản nháp.
2. Nhập thông tin cơ bản.
3. Chọn danh mục và đơn vị sản xuất.
4. Tạo quy cách bán, giá và tồn kho theo phạm vi.
5. Tải media.
6. Gắn chứng nhận và thông tin nguồn gốc.
7. Kiểm tra nội dung.
8. Gửi duyệt hoặc xuất bản tùy quyền.
9. Theo dõi lịch sử thay đổi.

### Quy tắc

- Không xuất bản khi thiếu trường bắt buộc.
- Slug phải duy nhất trong phạm vi công khai.
- Xóa sản phẩm có đơn hàng lịch sử cần chính sách đặc biệt; thường không xóa vật lý.
- Thay đổi giá không làm sửa đơn cũ.
- Tệp tải lên phải kiểm tra loại, kích thước và an toàn.
- Một sản phẩm cần có trạng thái rõ ràng: nháp, chờ duyệt, công khai, tạm ẩn, ngừng bán.

### Vấn đề cần làm rõ trước khi tạo model

- Sản phẩm có biến thể hay chỉ quy cách đơn giản?
- Giá và tồn kho thay đổi theo thuộc tính nào?
- Có lịch sử giá không?
- Có quản lý tồn thực tế hay chỉ trạng thái còn hàng?
- Ai được xuất bản?
- Có quy trình duyệt hai bước không?

---

## 16.3. Quản lý đơn hàng

### Chức năng

- Xem danh sách theo trạng thái.
- Tìm kiếm theo mã đơn, khách hàng hoặc số điện thoại.
- Xem chi tiết.
- Xác nhận, chuẩn bị, giao và hoàn thành.
- Hủy hoặc ghi nhận thất bại theo quyền.
- Ghi chú nội bộ.
- Xem lịch sử trạng thái.

### Quy tắc

- Hành động phải phù hợp trạng thái hiện tại.
- Không cho chỉnh trực tiếp tổng tiền tùy ý.
- Sửa thông tin giao nhận sau khi xác nhận cần chính sách và lịch sử.
- Ghi chú nội bộ không công khai cho khách.
- Xuất dữ liệu cần phân quyền và che thông tin nhạy cảm phù hợp.

---

## 16.4. Quản lý khách hàng

### Quy tắc

- Chỉ thu thập dữ liệu cần thiết.
- Có quyền xem khác với quyền sửa.
- Không cho nhân viên xem mật khẩu hoặc token.
- Có quy trình xử lý yêu cầu cập nhật/xóa dữ liệu nếu chính sách yêu cầu.
- Hạn chế xuất danh sách khách hàng hàng loạt.
- Lịch sử đơn hàng không được xóa cùng tài khoản nếu cần bảo toàn nghĩa vụ giao dịch.

---

## 16.5. Quản lý đánh giá và nội dung người dùng

- Có hàng chờ duyệt.
- Có lý do từ chối hoặc ẩn.
- Có khả năng xử lý báo cáo vi phạm.
- Lưu người xử lý và thời điểm.
- Không chỉnh sửa nội dung khách mà không để lại dấu vết.

---

## 16.6. Quản lý yêu cầu liên hệ và B2B

- Phân công người phụ trách.
- Theo dõi trạng thái và thời hạn phản hồi.
- Ghi chú lịch sử trao đổi.
- Chống trùng yêu cầu.
- Cho phép tìm kiếm và lọc.
- Không để yêu cầu mới bị mất do email thông báo thất bại.

---

# 17. Đặc tả báo cáo và đo lường

## 17.1. Nguyên tắc chung

Mọi chỉ số phải có:

- Định nghĩa.
- Phạm vi thời gian.
- Múi giờ.
- Nguồn dữ liệu.
- Cách xử lý đơn hủy, hoàn tiền và dữ liệu thử nghiệm.
- Quyền xem và xuất dữ liệu.

## 17.2. Báo cáo bán hàng

Có thể gồm:

- Số đơn theo trạng thái.
- Doanh thu gộp.
- Giảm giá.
- Phí giao hàng.
- Giá trị hủy hoặc hoàn.
- Doanh thu thuần theo định nghĩa đã duyệt.
- Sản phẩm bán chạy.

### Câu hỏi bắt buộc

- Doanh thu ghi nhận khi tạo đơn, xác nhận, giao thành công hay nhận tiền?
- Đơn COD chưa giao có tính doanh thu không?
- Chuyển khoản chưa đối soát xử lý thế nào?
- Báo cáo theo ngày đặt hay ngày hoàn thành?

## 17.3. Báo cáo quảng bá

- Lượt xem sản phẩm.
- Lượt tìm kiếm.
- Sản phẩm được yêu thích.
- Lượt chia sẻ nếu đo được.
- Nguồn truy cập.
- Tỷ lệ chuyển từ xem sang thêm giỏ và đặt hàng.

### Rủi ro

- Không đồng nhất định nghĩa giữa công cụ analytics và database giao dịch.
- Bot hoặc lượt truy cập nội bộ làm sai số liệu.
- Không được lưu dữ liệu theo dõi vượt quá mục đích và chính sách.

## 17.4. Báo cáo kết nối giao thương

- Số yêu cầu báo giá.
- Thời gian phản hồi.
- Tỷ lệ chuyển đổi thành cơ hội hoặc giao dịch.
- Nhóm sản phẩm được quan tâm.
- Khu vực hoặc loại đối tác quan tâm.

---

# 18. Các chức năng điểm nhấn khi có đầu tư mở rộng

## 18.1. Mỗi sản phẩm một trang thương hiệu

### Ý nghĩa

Mỗi sản phẩm có một trang nội dung chuyên sâu, kết hợp mua hàng, câu chuyện, video, chứng nhận, nguồn gốc và chia sẻ.

### Vấn đề cần chuẩn bị

- Quy trình biên tập nội dung chất lượng.
- URL và SEO lâu dài.
- Phân quyền duyệt nội dung.
- Media dung lượng lớn.
- Cập nhật khi chứng nhận hoặc giá thay đổi.

## 18.2. Trung tâm truy xuất và xác thực sản phẩm

### Ý nghĩa

Tập trung thông tin nguồn gốc, chứng nhận, đơn vị, vùng sản xuất và lịch sử xác minh.

### Vấn đề cần chuẩn bị

- Nguồn dữ liệu xác thực.
- Quyền xác minh.
- Cơ chế thu hồi hoặc hết hạn.
- Tích hợp hệ thống bên ngoài.
- Tránh biến QR thành liên kết trang trí không có giá trị.

## 18.3. Bản đồ thương mại sản phẩm Thạnh Hóa

### Ý nghĩa

Trực quan hóa vùng sản xuất, sản phẩm, điểm bán và kết nối địa phương.

### Vấn đề cần chuẩn bị

- Độ chính xác tọa độ.
- Quyền riêng tư.
- Chi phí bản đồ.
- Dữ liệu điểm bán thay đổi.
- Khả năng dùng trên mạng chậm.

## 18.4. Bộ quà tặng địa phương

### Ý nghĩa

Cho phép tạo các bộ sản phẩm theo dịp, ngân sách và đối tượng nhận quà.

### Vấn đề nghiệp vụ

- Bộ quà là sản phẩm độc lập hay tập hợp sản phẩm?
- Tồn kho tính theo thành phần nào?
- Cho phép thay thế thành phần không?
- Giá bộ quà và khuyến mại phân bổ ra sao?
- Đóng gói, vận chuyển và hóa đơn có khác đơn thường không?

## 18.5. Trợ lý tư vấn sản phẩm

### Ý nghĩa

Hỗ trợ khách tìm sản phẩm theo nhu cầu và giải đáp thông tin cơ bản.

### Ràng buộc bắt buộc

- Chỉ trả lời dựa trên dữ liệu đã được duyệt.
- Không tự tạo tuyên bố y tế, chứng nhận hoặc giá.
- Khi không chắc chắn phải chuyển sang nhân viên.
- Cần ghi nhận phản hồi và kiểm soát nội dung.
- Không để trợ lý thực hiện hành động giao dịch nhạy cảm khi chưa xác nhận rõ.

## 18.6. Video mua sắm ngắn

### Ý nghĩa

Tăng khả năng khám phá sản phẩm trên mobile.

### Vấn đề cần chuẩn bị

- Quyền sử dụng video và âm thanh.
- Dung lượng, streaming và mạng chậm.
- Phụ đề và khả năng tiếp cận.
- Liên kết video với sản phẩm và chiến dịch.
- Kiểm duyệt nội dung trước khi công khai.

---

# 19. Yêu cầu trải nghiệm và Design System mà Backend phải hiểu

Backend không quyết định giao diện, nhưng phải trả dữ liệu đủ để FE thể hiện đúng trạng thái.

## 19.1. Trạng thái UI bắt buộc

- Loading.
- Empty.
- Error.
- Retry.
- Success.
- Disabled/pending khi đang gửi.
- Partial data khi một phần dịch vụ phụ thất bại.

## 19.2. Nguyên tắc hiển thị thương mại

- Sản phẩm, giá và độ tin cậy ưu tiên hơn trang trí.
- Mỗi điểm chuyển đổi có một hành động chính rõ ràng.
- Màu xanh rừng là màu chính; màu cam/đất dùng cho khuyến mại thật sự.
- Thông tin lỗi phải dễ hiểu, không hiển thị stack trace hoặc mã nội bộ vô nghĩa.
- Danh sách cần phân trang hoặc tải hợp lý, tránh trả toàn bộ dữ liệu.
- Ảnh cần có kích thước ổn định và mô tả thay thế.

## 19.3. Backend cần hỗ trợ FE bằng dữ liệu nào

- Trạng thái có thể hành động.
- Lý do không thể mua hoặc không thể chuyển trạng thái.
- Field-level validation error.
- Metadata phân trang.
- Thứ tự media.
- Nhãn và trạng thái chứng nhận.
- Tổng tiền đã tính ở server.
- Thời điểm cập nhật và mã truy vết lỗi.

---

# 20. Vấn đề phải phân tích khi tạo Use Case, Activity, Sequence và State Diagram

## 20.1. Use Case Diagram

Trước khi vẽ, phải xác định:

- Actor là vai trò nghiệp vụ, không phải tên màn hình.
- Actor nào là người, actor nào là hệ thống ngoài.
- Guest và customer có quyền khác nhau ở đâu.
- Nhân viên nội dung và nhân viên đơn hàng có tách vai trò không.
- Quản trị viên có thực hiện nghiệp vụ hay chỉ cấp quyền.
- Hệ thống thanh toán, vận chuyển, email/SMS là actor phụ trợ.
- Portal nhà cung cấp thuộc giai đoạn sau, không đưa vào phạm vi hiện tại nếu chưa được duyệt.

### Sai lầm cần tránh

- Dùng “Frontend”, “Backend” làm actor nghiệp vụ.
- Vẽ mỗi nút bấm thành một use case.
- Gộp “quản lý đơn hàng” thành một use case quá lớn mà không mô tả hành động quan trọng.
- Cho actor thực hiện hành động ngoài quyền.

## 20.2. Activity Diagram

Cần thể hiện:

- Điểm kiểm tra và nhánh lỗi.
- Hành động của khách, FE, Backend và hệ thống ngoài nếu dùng swimlane.
- Cách xử lý request lặp.
- Điểm tạo dữ liệu chính thức.
- Điểm gửi thông báo chỉ sau khi giao dịch thành công.
- Cách rollback hoặc bù trừ khi một bước phụ thất bại.

## 20.3. Sequence Diagram

Phải làm rõ:

- Ai khởi tạo request.
- Dịch vụ nào là nguồn sự thật.
- Kiểm tra quyền ở đâu.
- Validation nghiệp vụ ở đâu.
- Transaction bắt đầu/kết thúc ở đâu.
- Khi nào gọi dịch vụ ngoài.
- Webhook hoặc callback được xác minh thế nào.
- Timeout, retry và idempotency xử lý ra sao.
- Không gửi notification trước khi dữ liệu chính được ghi thành công.

## 20.4. State Diagram

Áp dụng cho:

- Đơn hàng.
- Thanh toán.
- Sản phẩm/nội dung.
- Đánh giá.
- Yêu cầu báo giá.
- Chứng nhận/xác minh.

Mỗi state diagram phải chỉ rõ:

- Trạng thái bắt đầu.
- Trạng thái kết thúc.
- Sự kiện chuyển trạng thái.
- Điều kiện cho phép.
- Actor được quyền.
- Side effect.
- Trạng thái không thể quay lại.

---

# 21. Vấn đề phải phân tích khi thiết kế database và mô hình thực thể

> Phần này không đưa ra bảng hoặc cấu trúc vật lý. Mục tiêu là giúp AI Agent biết các câu hỏi cần giải quyết trước khi đề xuất mô hình.

## 21.1. Ranh giới thực thể và giá trị nghiệp vụ

- Phân biệt dữ liệu chính với dữ liệu lịch sử.
- Phân biệt sản phẩm với quy cách bán.
- Phân biệt khách hàng với người nhận hàng.
- Phân biệt đơn hàng với thanh toán và giao hàng.
- Phân biệt đơn vị sản xuất với đơn vị bán hoặc đóng gói.
- Phân biệt chứng nhận với tệp minh chứng và trạng thái xác minh.
- Phân biệt bài viết, trang nội dung và khối nội dung trang chủ.

## 21.2. Quan hệ có thể thay đổi theo thời gian

- Giá sản phẩm.
- Chứng nhận.
- Đơn vị sản xuất/cung cấp.
- Danh mục.
- Trạng thái bán.
- Thông tin liên hệ.

AI Agent phải hỏi xem cần lịch sử hay chỉ giá trị hiện tại.

## 21.3. Xóa dữ liệu

Trước khi đề xuất xóa, phải kiểm tra:

- Dữ liệu có được tham chiếu bởi đơn hàng không?
- Có nghĩa vụ lưu trữ hoặc báo cáo không?
- Có thể ẩn/ngừng hoạt động thay vì xóa không?
- Xóa có làm hỏng URL công khai không?
- Có cần ẩn danh dữ liệu cá nhân thay vì xóa hoàn toàn không?

## 21.4. Tính đồng thời

Các vùng dễ xung đột:

- Tồn kho.
- Cập nhật trạng thái đơn.
- Duyệt nội dung.
- Áp dụng mã giảm giá.
- Xác nhận thanh toán.
- Phân công yêu cầu B2B.

Cần có chiến lược phát hiện và xử lý cập nhật đồng thời; không mặc định lần ghi sau luôn đúng.

## 21.5. Chỉ mục và hiệu năng

Chỉ quyết định sau khi biết:

- Truy vấn chính.
- Khối lượng dữ liệu.
- Cách tìm kiếm và lọc.
- Tần suất đọc/ghi.
- Báo cáo thời gian thực hay định kỳ.
- Dữ liệu cần unique theo nghiệp vụ.

Không tạo chỉ mục chỉ vì tên trường “có vẻ hay tìm”.

## 21.6. Audit và truy vết

Cần xác định cho từng loại dữ liệu:

- Ai tạo.
- Ai sửa.
- Ai duyệt.
- Thời điểm.
- Giá trị trước/sau có cần lưu không.
- Lý do thay đổi.
- Có cần trace đến request hoặc phiên làm việc không.

## 21.7. Dữ liệu cá nhân và nhạy cảm

- Chỉ lưu dữ liệu cần thiết.
- Phân loại dữ liệu công khai, nội bộ và hạn chế.
- Xác định thời gian lưu.
- Che dữ liệu trong log.
- Giới hạn quyền truy cập và xuất dữ liệu.
- Không lưu bí mật hoặc token ở dạng có thể đọc lại nếu không cần.

---

# 22. Vấn đề phải phân tích khi thiết kế API

## 22.1. Hợp đồng API

Mỗi API cần có:

- Mục tiêu nghiệp vụ.
- Người được phép gọi.
- Input.
- Output.
- Validation.
- Mã lỗi nghiệp vụ.
- Trạng thái HTTP phù hợp.
- Idempotency nếu cần.
- Ảnh hưởng phụ.
- Logging và trace.

## 22.2. Quy tắc resource và action

- Dùng resource cho CRUD dữ liệu thông thường.
- Dùng action rõ ràng cho chuyển trạng thái có luật nghiệp vụ.
- Không cho client cập nhật trực tiếp một trường `status` tùy ý nếu việc chuyển trạng thái cần kiểm soát.
- Không để component tự ghép URL backend.

## 22.3. Validation error

FE cần biết:

- Trường nào lỗi.
- Mã lỗi ổn định.
- Thông báo dễ hiểu.
- Có thể retry hay không.
- Lỗi do dữ liệu khách nhập hay do trạng thái hệ thống.

## 22.4. Phân trang và lọc

- Thống nhất `page`, `pageSize` hoặc chiến lược cursor.
- Có giới hạn pageSize.
- Trả tổng số khi thực sự cần.
- Whitelist sort/filter.
- Xác định hành vi khi tham số không hợp lệ.

## 22.5. API upload

- Xác thực và phân quyền.
- Kiểm tra loại file, kích thước và nội dung.
- Không để frontend giữ storage secret.
- Xử lý file tải thành công nhưng metadata thất bại và ngược lại.
- Có cơ chế xóa file mồ côi.
- Có trạng thái xử lý ảnh/video nếu cần.

## 22.6. Tích hợp hệ thống ngoài

- Timeout.
- Retry có giới hạn.
- Circuit breaker nếu phù hợp.
- Idempotency.
- Webhook signature.
- Lưu trạng thái yêu cầu và phản hồi cần thiết.
- Không làm thất bại giao dịch chính chỉ vì notification phụ thất bại, trừ khi nghiệp vụ yêu cầu.

---

# 23. Vấn đề phải phân tích khi xây Handler, Service và Domain Logic

## 23.1. Handler

Handler nên:

- Nhận command/query rõ mục tiêu.
- Kiểm tra quyền ở ranh giới phù hợp.
- Gọi validation.
- Điều phối nghiệp vụ.
- Trả kết quả chuẩn hóa.

Handler không nên:

- Chứa quá nhiều truy vấn rời rạc và logic giao diện.
- Tự gửi response theo nhiều format khác nhau.
- Gọi trực tiếp dịch vụ ngoài ở nhiều nơi không kiểm soát.

## 23.2. Service/domain logic

Logic nên đặt tại nơi bảo vệ được quy tắc nghiệp vụ:

- Chuyển trạng thái đơn.
- Tính tổng tiền.
- Kiểm tra điều kiện áp dụng ưu đãi.
- Xác định quyền công khai chứng nhận.
- Tính khả năng mua sản phẩm.
- Quyết định hủy hoặc hoàn tồn.

Không được lặp cùng quy tắc ở nhiều handler khác nhau.

## 23.3. Transaction

Cần xác định:

- Những thay đổi nào phải thành công cùng nhau.
- Dịch vụ ngoài không nên nằm trong transaction database dài.
- Nếu gửi thông báo thất bại sau khi tạo đơn, xử lý retry thế nào.
- Nếu thanh toán thành công nhưng cập nhật đơn thất bại, cơ chế đối soát thế nào.

## 23.4. Sự kiện nghiệp vụ

Có thể dùng cho:

- Đơn được tạo.
- Đơn được xác nhận.
- Thanh toán được xác nhận.
- Đơn được giao thành công.
- Sản phẩm được xuất bản.
- Yêu cầu báo giá được tiếp nhận.

Nhưng phải xác định:

- Event xảy ra sau khi dữ liệu nào được commit.
- Consumer có chạy lặp an toàn không.
- Có cần outbox hoặc cơ chế đảm bảo không mất event không.
- Event là nội bộ hay tích hợp bên ngoài.

---

# 24. Vấn đề phải phân tích khi xây console/admin và công cụ vận hành

## 24.1. Phân quyền

- Ai xem được dữ liệu nào.
- Ai được tạo, sửa, duyệt, xuất bản hoặc hủy.
- Hành động nào cần xác nhận lại.
- Hành động nào cần ghi lý do.
- Có cần tách người tạo và người duyệt không.

## 24.2. Danh sách và tìm kiếm

- Cột nào cần hiển thị.
- Bộ lọc nào phục vụ công việc thực tế.
- Trạng thái nào cần nổi bật.
- Có thao tác hàng loạt không.
- Xuất dữ liệu có giới hạn và audit không.

## 24.3. Form quản trị

- Không để nhân viên nhập mã nội bộ khó hiểu nếu có thể chọn bằng tên.
- Có autosave hay không.
- Có bản nháp không.
- Có preview trước khi xuất bản không.
- Xử lý rời trang khi chưa lưu.
- Hiển thị validation đồng nhất với Backend.

## 24.4. Audit log

Công cụ vận hành cần giúp trả lời:

- Ai đã thay đổi gì?
- Khi nào?
- Từ giá trị nào sang giá trị nào?
- Lý do?
- Từ màn hình hoặc request nào?

Không dùng audit log như log kỹ thuật thuần túy khó đọc.

---

# 25. Yêu cầu phi chức năng

## 25.1. Hiệu năng

- Trang quan trọng phải tải tốt trên mobile và mạng không ổn định.
- Ảnh/video phải tối ưu.
- Danh sách lớn phải phân trang hoặc tải từng phần.
- Không nạp thư viện client không cần thiết trên mọi route.
- API phải có giới hạn và timeout hợp lý.

## 25.2. Khả năng tiếp cận

- Điều hướng bằng bàn phím.
- Focus rõ ràng.
- Label cho form.
- Alt text cho ảnh.
- Không chỉ dùng màu để truyền tải trạng thái.
- Nút chỉ có icon phải có tên truy cập.

## 25.3. Bảo mật

- Xác thực và phân quyền ở Backend.
- Mã hóa mật khẩu phù hợp.
- Chống request giả mạo, injection và upload nguy hiểm.
- Rate limit các endpoint nhạy cảm.
- Không log mật khẩu, token, OTP hoặc dữ liệu thanh toán nhạy cảm.
- Secret chỉ ở hệ thống server.

## 25.4. Khả năng quan sát

- Trace ID xuyên suốt request.
- Log có cấu trúc.
- Theo dõi lỗi API, job, webhook và dịch vụ ngoài.
- Cảnh báo khi đơn hoặc thanh toán bị treo.
- Dashboard kỹ thuật không thay thế báo cáo nghiệp vụ.

## 25.5. Sao lưu và khôi phục

- Xác định dữ liệu nào cần sao lưu.
- Chu kỳ sao lưu.
- Thời gian khôi phục mục tiêu.
- Kiểm tra khôi phục định kỳ.
- Tách sao lưu dữ liệu và media.

---

# 26. Ma trận chức năng — FE — Backend — kiểm thử

| Nhóm chức năng | FE phải thể hiện | Backend phải bảo đảm | QA phải kiểm tra |
|---|---|---|---|
| Danh mục sản phẩm | Lọc, sắp xếp, empty/error | Dữ liệu công khai, phân trang, whitelist filter | Kết hợp filter, URL, phân trang biên |
| Chi tiết sản phẩm | Giá, quy cách, tồn, media, trust info | Giá/tồn đúng theo quy cách, trạng thái bán | Hết hàng, đổi giá, slug sai, media lỗi |
| Giỏ hàng | Thêm/sửa/xóa, cảnh báo thay đổi | Kiểm tra lại sản phẩm, giá, số lượng | Merge guest/user, request lặp, hết hàng |
| Checkout | Form rõ ràng, tổng tiền, pending | Tính tiền server, tạo đơn một lần | Double click, mạng chậm, validation |
| Đơn hàng | Timeline dễ hiểu | State transition, lịch sử, quyền | Chuyển sai trạng thái, hủy, đồng thời |
| Tài khoản | Hồ sơ, địa chỉ, lịch sử | Bảo mật, ownership | Truy cập chéo, reset password, session |
| Đánh giá | Gửi và hiển thị trạng thái | Eligibility, moderation | Mua/không mua, spam, chỉnh sửa |
| B2B | Form và mã tiếp nhận | Workflow, phân công, audit | Duplicate, SLA, quyền dữ liệu |
| CMS | Draft, preview, publish | Quy trình trạng thái, version/audit | Lịch xuất bản, link hỏng, quyền duyệt |
| Báo cáo | Bộ lọc thời gian, số liệu rõ | Định nghĩa chỉ số thống nhất | Timezone, đơn hủy, dữ liệu thử nghiệm |

---

# 27. Checklist bắt buộc trước khi AI Agent tạo code

## 27.1. Nghiệp vụ

- [ ] Chức năng thuộc giai đoạn hiện tại hay giai đoạn sau?
- [ ] Actor nào được dùng?
- [ ] Luồng chính đã rõ?
- [ ] Luồng lỗi và biên đã rõ?
- [ ] Trạng thái và chuyển trạng thái đã được xác nhận?
- [ ] Có dữ liệu lịch sử cần bảo toàn không?
- [ ] Có quy tắc thời gian, tiền tệ, tồn kho hoặc quyền không?

## 27.2. FE

- [ ] Route và component hiện tại là gì?
- [ ] Có thể tái sử dụng design system hiện có không?
- [ ] Có loading, empty, error, success không?
- [ ] Mobile và accessibility đã tính đến chưa?
- [ ] API mapping đã thống nhất chưa?

## 27.3. Backend/API

- [ ] Endpoint có mục tiêu nghiệp vụ rõ không?
- [ ] Validation và error code đã định nghĩa chưa?
- [ ] Có cần transaction, idempotency hoặc concurrency không?
- [ ] Có gọi hệ thống ngoài không?
- [ ] Có phân quyền và audit không?

## 27.4. Dữ liệu

- [ ] Đây là dữ liệu hiện tại hay snapshot lịch sử?
- [ ] Có được xóa không?
- [ ] Có cần version/audit không?
- [ ] Quan hệ có thay đổi theo thời gian không?
- [ ] Có dữ liệu cá nhân hoặc nhạy cảm không?

## 27.5. Kiểm thử

- [ ] Happy path.
- [ ] Validation lỗi.
- [ ] Không có quyền.
- [ ] Request lặp.
- [ ] Dữ liệu thay đổi đồng thời.
- [ ] Dịch vụ ngoài timeout/thất bại.
- [ ] Mobile và network chậm.

---

# 28. Mẫu đặc tả AI Agent phải dùng khi bổ sung chức năng mới

```markdown
## [Tên chức năng]

### 1. Mục tiêu nghiệp vụ

### 2. Phạm vi
- Trong phạm vi:
- Ngoài phạm vi:

### 3. Actor

### 4. Điều kiện trước

### 5. Luồng chính
1.
2.
3.

### 6. Luồng thay thế và lỗi

### 7. Quy tắc nghiệp vụ

### 8. Trạng thái và chuyển trạng thái

### 9. Dữ liệu khái niệm
> Không định nghĩa bảng/cột nếu chưa được yêu cầu.

### 10. Yêu cầu FE

### 11. Yêu cầu API/Backend

### 12. Vấn đề ảnh hưởng diagram/database

### 13. Bảo mật và phân quyền

### 14. Logging/Audit/Monitoring

### 15. Tiêu chí nghiệm thu

### 16. Câu hỏi chưa xác nhận
```

---

# 29. Các câu hỏi mở cấp dự án cần được chốt dần

1. Giai đoạn đầu có cho đặt hàng không cần tài khoản không?
2. Phạm vi giao hàng và cách tính phí là gì?
3. Có quản lý tồn kho số lượng thực tế hay chỉ còn/hết hàng?
4. Sản phẩm có nhiều quy cách/biến thể đến mức nào?
5. Giá bán là cố định, theo thời điểm hay có giá khuyến mại?
6. Quy trình xác nhận chuyển khoản thực hiện thủ công hay tích hợp ngân hàng?
7. Khách được hủy đơn đến trạng thái nào?
8. Có hỗ trợ đổi/trả/hoàn tiền trong MVP không?
9. Ai được tạo, duyệt và xuất bản sản phẩm/nội dung?
10. Chứng nhận được xác minh bởi ai và quản lý hết hạn thế nào?
11. Bản đồ công khai vị trí chính xác hay chỉ khu vực?
12. Có cho người đã mua mới được đánh giá không?
13. Báo giá B2B chỉ tiếp nhận lead hay cần tạo báo giá chính thức?
14. Có cần hóa đơn điện tử trong giai đoạn đầu không?
15. Chỉ số doanh thu ghi nhận tại thời điểm nào?
16. Dữ liệu nào được phép công khai về đơn vị sản xuất?
17. Có yêu cầu đa ngôn ngữ không?
18. Có tích hợp Zalo, SMS, email ở mức nào?
19. Có yêu cầu backup, SLA và thời gian hỗ trợ cụ thể không?
20. Lộ trình Portal doanh nghiệp/HTX/hộ kinh doanh ảnh hưởng quyền sở hữu dữ liệu hiện tại thế nào?

---

# 30. Definition of Done chung

Một chức năng chỉ được xem là hoàn thành khi:

- Nghiệp vụ và acceptance criteria được xác nhận.
- FE thể hiện đủ trạng thái loading, empty, error, success và responsive.
- API có validation, phân quyền, error contract và tài liệu.
- Backend bảo vệ quy tắc nghiệp vụ, không chỉ kiểm tra ở FE.
- Dữ liệu lịch sử và quan hệ hiện có không bị phá vỡ.
- Có kiểm thử happy path và các lỗi quan trọng.
- Có audit/log/trace phù hợp với mức độ rủi ro.
- Không lộ secret hoặc dữ liệu nhạy cảm.
- Tài liệu chức năng, API và diagram liên quan được cập nhật.
- Build và smoke test luồng chính thành công.
- Không mở rộng ngoài phạm vi mà chưa được duyệt.

---

# 31. Kết luận sử dụng cho AI Agent

AI Agent phải xem tài liệu này là bản đồ nghiệp vụ, không phải giấy phép tự động tạo kiến trúc vật lý. Mỗi khi được yêu cầu tạo handler, model, service, API, diagram, migration, console hoặc nâng cấp chức năng, AI Agent cần:

1. Xác định chức năng và luồng nghiệp vụ liên quan.
2. Kiểm tra phạm vi giai đoạn hiện tại.
3. Nêu rõ giả định và câu hỏi còn thiếu.
4. Đánh giá tác động xuyên FE, Backend, dữ liệu, báo cáo và vận hành.
5. Đề xuất phương án nhưng không xem phương án chưa duyệt là sự thật dự án.
6. Bảo toàn trạng thái, lịch sử giao dịch, tính xác thực và khả năng mở rộng.
7. Cập nhật tài liệu cùng lúc với thay đổi code hoặc mô hình.

Tài liệu được thiết kế để có thể mở rộng bằng các đặc tả con theo từng domain mà không phải viết lại toàn bộ ngữ cảnh dự án.
