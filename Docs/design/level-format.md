# Định dạng level `.pzl`

File văn bản thuần, mỗi level một file trong `Assets/_Project/Levels/<pack>/`. Unity tự nhập thành `LevelAsset` (importer `LevelFileImporter` trong `Puzzle.Editor`). Sửa level = sửa file `.pzl`, không sửa asset trong Inspector.

```
// comment tới cuối dòng
id: p1-01            // khoá lưu tiến độ, duy nhất (mặc định: tên file)
difficulty: 1        // 1..5
tip: Kéo ngón qua các ô để gộp thành vùng.   // câu hiện trên băng-rôn hướng dẫn (tuỳ chọn)

shape L              // khai báo hình: '#' là ô, '.' là trống
#.
#.
##

rule shape-bank L    // luật = id trong RuleRegistry + tham số (tên hình đã khai báo)

solution             // lời giải: mỗi ký tự một ô, '.' = không có ô
AAA.                 // cùng chữ liền nhau = cùng vùng; cạnh giữa hai chữ khác nhau = ranh giới
BBA.
B..C
BCCC
```

Quy tắc:
- Dòng lưới không chứa khoảng trắng; nhãn vùng dùng chữ hoa hoặc chữ số.
- Lời giải là **bắt buộc**. Test `LevelCatalogTests` kiểm tra mọi level: đọc được, và lời giải thoả mọi luật.
- Mỗi level phải có **đúng một lời giải**. Chưa có solver trong game nên kiểm tra bằng công cụ ngoài trước khi thêm level (ghi trong `Docs/decisions.md`); khi có `Solver` thì đưa vào test.
- Chưa hỗ trợ ranh giới cho sẵn (`given`); thêm khi có level cần.
- Không chép level của game khác.
