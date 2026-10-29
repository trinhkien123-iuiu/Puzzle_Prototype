# Quyết định kiến trúc

Mỗi dòng: ngày · chọn gì · vì sao · bỏ phương án nào.

- 2026-10-29 · Cạnh lưu là `Edge(a, b)` chuẩn hoá `a < b` (theo dòng rồi cột); `BoardShape` đánh số sẵn ô/cạnh và `PuzzleState` giữ `EdgeState[]` theo chỉ số cạnh · tra nhanh, sao chép rẻ cho undo/solver, không cấp phát khi đọc · bỏ `Dictionary<Edge, EdgeState>` và bỏ lưu hai mảng cạnh ngang/dọc riêng (khó với bàn khuyết ô).
- 2026-10-29 · Khi tính vùng, cạnh `Unknown` vẫn nối hai ô (chỉ `Border` cắt); mỗi `Region` đếm cạnh Unknown bên trong để luật phân biệt Incomplete/Broken · khớp cách chơi "vẽ ranh giới để chia bàn", và vùng đúng hình thì không bắt người chơi đánh dấu Joined từng cạnh · bỏ cách chỉ nối qua `Joined` (bàn mới sẽ là toàn ô lẻ, luật nào cũng báo lỗi).
- 2026-10-29 · So hình bằng dạng chính tắc: biến thể nhỏ nhất trong 8 phép xoay/lật (`ShapeVariants.Canonical`), bank lưu sẵn tập dạng chính tắc · mỗi lần kiểm tra vùng chỉ là một phép tra HashSet · bỏ so từng cặp biến thể.
- 2026-10-29 · `RuleViolation` mang `RuleId` dạng chuỗi (khoá trong `RuleRegistry`) thay vì tham chiếu lớp luật · Presentation tra hình ảnh lỗi theo dữ liệu, không cần `is XxxRule` · bỏ enum loại luật (thêm luật lại phải sửa enum).
- 2026-10-29 · Domain biên dịch được ngoài Unity: chạy nhanh bằng một `.csproj` tạm (net8.0, LangVersion 9) gom `Code/Domain` + `Tests` với NUnit, không đưa vào repo · kiểm tra logic khi Unity chưa mở; Test Runner trong Unity vẫn là chuẩn trước khi commit.
