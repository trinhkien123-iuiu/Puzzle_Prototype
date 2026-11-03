# Phân tích UI màn chơi: The Artisan of Glimmith (bản Demo)

> Ghi chú từ video người dùng tự chơi (2026-10-29, file ngoài repo `D:\GitHub\Video\`). Mô tả **bố cục và cách tương tác** của màn chơi.

## 1. Bố cục màn chơi (PC, màn ngang)

| Vị trí | Thành phần | Vai trò |
|---|---|---|
| Giữa | Tấm giấy da lớn chứa bàn chơi | Bàn là lưới ô; ô chưa tô màu trắng, khe giữa ô là **đường chấm** |
| Trên giữa | Băng-rôn hướng dẫn (khung kính màu) | Một câu + icon minh hoạ, đổi theo từng bước tutorial |
| Phải | Cuộn giấy **Shape Bank** | Tên luật + một câu ("All regions must be this shape" / "one of these shapes") + hình nhỏ của từng hình được phép |
| Trái | Bảng 9 màu + nút bảng màu đầy đủ | Chọn màu để tô vùng (mở dần qua tutorial) |
| Trái (sau) | 3 nút chế độ + "?" | Tô màu / vẽ ranh giới kim loại / ghi chú |
| Trên trái | Exit | Thoát ra bản đồ |
| Dưới trái | Undo, Restart | Hoàn tác, chơi lại |
| Dưới phải | Difficulty + số | Độ khó của puzzle |
| Dưới bàn | Continue | Chỉ hiện khi giải xong |

## 2. Cách chơi thật sự (khác bản greybox hiện tại)

- **Thao tác chính là kéo để gộp ô thành vùng có màu** ("Click and drag to merge regions"). Ô chưa gộp đứng riêng, không phải "cả bàn là một vùng".
- Vùng đã gộp hiện như **mảnh kính màu**; vùng nằm kề nhau khác màu là khác vùng.
- **Ranh giới kim loại là tuỳ chọn**: kéo từ góc dọc theo đường chấm để đánh dấu cạnh "chắc chắn" (ghi chú cho người chơi), không bắt buộc để giải.
- Xoá: kéo chuột phải (vùng sắp xoá hiện sọc vàng).
- Giải xong: các vùng sáng viền, hiện nút Continue; tiến độ tự lưu.
- Tutorial dạy từng ý một qua băng-rôn: gộp → xoay hình → chọn màu tự do → ranh giới kim loại → đổi chế độ.

## 3. Hàm ý cho bản mobile

- Đổi mô hình chơi của greybox: **ô bắt đầu tách riêng**, kéo ngón để gộp thành vùng (vùng = các ô nối bằng cạnh Joined); ranh giới chỉ là đánh dấu phụ. Cần sửa `RegionMap` (chỉ nối qua Joined) và cách luật tính Incomplete.
- Bố cục dọc đề xuất: băng-rôn hướng dẫn trên cùng → khung Shape Bank → bàn → hàng màu → Undo / Restart / Gợi ý / Kiểm tra; Continue thay popup.
- Art: kính màu, giấy da; asset tham khảo lấy từ bản dump trong `D:\GitHub\Glimmith_Reference\Export` (dùng học tập, không thương mại).
