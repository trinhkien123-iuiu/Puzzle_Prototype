# Phân tích luật: The Artisan of Glimmith (bản Demo)

> Ghi chú nghiên cứu. Nguồn: bản demo miễn phí trên Steam, đọc file `.puz` trong pak (UE 4.27, không mã hóa).
> File gốc nằm ở `D:\GitHub\Glimmith_Reference` (ngoài repo, không commit). Tài liệu này chỉ mô tả luật để tự thiết kế lại.

## 1. Lõi game

Lưới ô vuông (có thể khuyết ô, tạo hình bàn không đều). Người chơi **vẽ ranh giới trên các cạnh** để chia toàn bộ lưới thành các vùng. Mỗi level có một tập luật; tất cả vùng phải thoả mọi luật.

## 2. Định dạng level `.puz` (văn bản thuần)

```
VERSION 1
DIFFICULTY 1..5
SHAPE <id> <số dòng>      # định nghĩa một polyomino bằng '#'
SHAPE_BANK <id> <id> ...  # các hình được phép
AREA_EQUALS <n>           # (tuỳ chọn) luật toàn cục
NO_RECTANGLES             # (tuỳ chọn) luật toàn cục
DIMENSIONS <rộng> <cao>
PUZZLE                    # đề bài
SOLUTION                  # lời giải (cạnh '#' = ranh giới)
```

Lưới vẽ bằng ASCII, mỗi ô rộng 3 ký tự:

| Ký hiệu | Vị trí | Ý nghĩa |
|---|---|---|
| `..` | ô | ô trống |
| khoảng trắng | ô | không có ô (bàn khuyết) |
| `--` / `\|` | cạnh | cạnh chưa quyết định |
| `##` / `#` | cạnh | ranh giới cho sẵn |
| `==` / `=` | cạnh | **Gemini**: hai vùng hai bên khác nhau nhưng **cùng hình** |
| `!` | cạnh | **Delta**: hai vùng hai bên **khác hình** |
| `P1..Pn` | ô | ký hiệu màu/nhóm |
| `S1..Sn` | ô | ô gắn với hình số n |

## 3. Danh sách luật (đã kiểm chứng trên lời giải của 53 puzzle)

| Luật | Mô tả | Kiểm chứng |
|---|---|---|
| **Shape bank** | Mọi vùng phải là một trong các hình trong bank (được xoay/lật) | 35/36 khớp (lệch `0067`, có thể luật xoay/lật khác) |
| **Ô hình S_n** | Vùng chứa ô `S_n` phải đúng hình n | 15/15 |
| **Gemini `=`** | Cạnh `=` luôn là ranh giới; hai vùng hai bên cùng hình | 23/23 |
| **Delta `!`** | Cạnh `!` luôn là ranh giới; hai vùng hai bên khác hình | 26/26 |
| **One of each (P)** | Mỗi vùng chứa đúng một ô mỗi loại `P1..Pn` | 14/14 |
| **AREA_EQUALS n** | Mọi vùng có diện tích n | 2/2 |
| **NO_RECTANGLES** | Không vùng nào là hình chữ nhật | 1/1 |

## 4. Cấu trúc tiến trình (Zone 1)

| Pack | Số puzzle | Luật được dạy | Lưới |
|---|---|---|---|
| `1-single-shape` | 11 | Bank chỉ 1 hình | 3×4 → 7×8 |
| `2-shape-bank` | 10 | Bank 2+ hình | 4×6 → 11×10 |
| `3-gemini-delta` | 6 | Thêm cạnh `=` / `!` | 4×6 → 6×6 |
| `4-slash-pack` | 5 | Ký hiệu `P` (one of each), không cần bank | 4×4 → 7×8 |
| `6-gemini-delta` | 6 | Gemini/Delta + bank, khó hơn | đến 8×4, 7×7 |
| `7-slash-pack` | 6 | P + bank trộn, khó hơn | đến 11×10 |
| Zone1 (demo) | 9 | Giới thiệu `AREA_EQUALS`, `NO_RECTANGLES`, `S_n` | 4×3 → 11×12 |

Nhận xét thiết kế:
- Mỗi pack chỉ dạy **một luật**, mở đầu bằng lưới rất nhỏ (3×4, 4×4) và độ khó 1.
- Pack sau **trộn** luật cũ với luật mới thay vì thêm luật mới liên tục.
- Lưới nhỏ (đa số ≤ 7×7) → **hợp màn hình dọc mobile**.

## 5. Toàn bộ luật của bản đầy đủ (từ file Localization tiếng Anh)

Bản demo chỉ dùng 7 luật, nhưng file ngôn ngữ chứa 24 luật. Tên và mô tả trong file nằm tách rời (khoá GUID), nên cột "Ghép cặp" ghi mức độ chắc chắn: **rõ** = khớp icon/tên hoặc đã thấy trong demo, **suy luận** = ghép theo nghĩa.

| Nhóm | Luật | Mô tả | Ghép cặp |
|---|---|---|---|
| Hình | Shape Bank | Mọi vùng phải là một trong các hình cho sẵn | rõ |
| Hình | Polyomino | Ký hiệu trong ô vẽ đúng hình của vùng chứa nó | rõ |
| Hình | Match | Mọi vùng cùng một hình | suy luận |
| Hình | Mismatch | Mọi vùng phải khác hình nhau | suy luận |
| Hình | Mingle Shape | Hai vùng kề nhau phải khác hình | suy luận |
| Hình | Elusive | Hình cho trước không được nằm vừa trong bất kỳ vùng nào | suy luận |
| Hình | Boxy / Non-Boxy | Mọi vùng là hình chữ nhật / không vùng nào là hình chữ nhật | rõ |
| Cạnh | Gemini | Hai vùng chạm ký hiệu phải cùng hình | rõ |
| Cạnh | Delta | Hai vùng chạm ký hiệu phải khác hình | rõ |
| Cạnh | Inequality | Mũi nhọn chỉ về vùng nhỏ hơn | rõ (icon `>`) |
| Cạnh | Difference | Số cho biết hiệu diện tích hai vùng | rõ |
| Diện tích | Area Number | Số trong ô = diện tích vùng chứa nó | rõ |
| Diện tích | Precision / Minimum / Maximum / Range | Mọi vùng có diện tích đúng n / ít nhất n / nhiều nhất n / trong khoảng | rõ |
| Diện tích | Size Separation | Hai vùng kề nhau phải khác diện tích | suy luận |
| Ký hiệu | Rose Windows | Mỗi vùng chứa đúng một ký hiệu mỗi loại | rõ (luật P trong demo) |
| Ký hiệu | Solitude | Mỗi vùng chỉ chứa một ký hiệu | suy luận |
| Đếm | Compass | Đếm số ô của vùng nằm xa hơn về Bắc/Nam/Đông/Tây so với ô gợi ý | rõ (icon la bàn 16 biến thể) |
| Đếm | Watchtower | Đếm số vùng chạm vào ô gợi ý | rõ (icon Radar 1–4 chấm) |
| Ranh giới | Palisade | Ký hiệu cho biết các cạnh của ô đó là ranh giới thế nào (xoay được) | rõ (icon Palisade) |
| Ranh giới | Bricky | Không có điểm nào 4 ranh giới gặp nhau | suy luận |
| Ranh giới | Loopy | Không có điểm nào đúng 3 ranh giới gặp nhau | suy luận |

Các ghi chú hướng dẫn trong game cũng cho thấy:
- Hình được **xoay và lật** (mirror).
- Một vùng không bao giờ chạm cả hai bên của một ranh giới; ranh giới luôn ngăn cách hai vùng khác nhau.
- Thao tác: kéo để **gộp ô thành vùng** (tô màu), vẽ **ranh giới kim loại** cho cạnh chắc chắn, chế độ ghi chú, Q tô cả vùng.
- Bản đầy đủ có **28 khu vực**, mỗi khu là một "cửa sổ kính" cần hoàn thành một phần để mở đường.

## 6. Hàm ý cho prototype mobile

- Định dạng dữ liệu level dạng văn bản như trên rất dễ tự định nghĩa lại (JSON/ScriptableObject) và có sẵn lời giải để kiểm tra.
- Có thể làm **solver/generator** vì mọi luật đều là kiểm tra trên đồ thị vùng; lời giải có sẵn cho phép dựng booster "gợi ý cạnh" chính xác.
- Thứ tự luật đề xuất cho bản mobile: Single shape → Shape bank → One of each → Gemini/Delta → Area/No rectangles.
- Không dùng lại level, art hay tên của game gốc; tự thiết kế level mới theo cùng nguyên tắc.
