# Định dạng level `.puz` — The Artisan of Glimmith

Nguồn: 54 file `.puz` trong bản Demo (`Geri/Content/AGeri/Puzzles/**`), export raw bằng FModel vào `D:\GitHub\Glimmith_Reference`.
Parser + bộ kiểm luật: [`tools/puz.py`](../tools/puz.py).

> Luật bên dưới được **suy ra từ lời giải có sẵn** (52/54 file có `SOLUTION`, cả 52 đều qua `puz.check`), chưa đối chiếu với code game. Ký hiệu chưa xuất hiện trong demo (vd `>`) là suy luận theo đối xứng.

## Thể loại

Chia lưới thành các **vùng (region)** liên thông — mỗi vùng là một ô kính màu. Lời giải = tập đường biên giữa các ô.

## Cấu trúc file (text, UTF-8)

```
VERSION 1
PUZZLE_VERSION 2        (tuỳ chọn, 2/54 file)
DIFFICULTY <n>          1..5
SHAPE <id> <rows>       (0..n lần) theo sau là <rows> dòng pixel '#'
SHAPE_BANK <id> [<id>…] (tuỳ chọn)
AREA_EQUALS <n>         (tuỳ chọn)
NO_RECTANGLES           (tuỳ chọn, cờ không tham số)
DIMENSIONS <W> <H>      W = số cột, H = số hàng
PUZZLE
<lưới ASCII 2H+1 dòng>
SOLUTION
<lưới ASCII cùng kích thước>
```

## Hình học lưới

Mỗi ô rộng 3 ký tự, cao 2 dòng. Với ô hàng `r`, cột `c`:

| Thành phần | Dòng | Ký tự |
|---|---|---|
| Nội dung ô | `2r+1` | `[3c+1 : 3c+3]` |
| Cạnh trái ô | `2r+1` | `3c` |
| Cạnh trên ô | `2r` | `[3c+1 : 3c+3]` |
| Góc `+` | `2r` | `3c` |

Ô có nội dung `"  "` (2 dấu cách) **không thuộc bàn chơi** → bàn có thể có hình dạng bất kỳ / có lỗ.

Trong `SOLUTION`: cạnh `#`/`##` = biên vùng, cạnh trống = hai ô cùng vùng.

## Ký hiệu trong `PUZZLE`

### Nội dung ô

| Token | Ý nghĩa |
|---|---|
| `..` | ô trống |
| `P1`…`P4` | Mỗi vùng phải chứa **đúng một** ô `Pn` cho mỗi loại `n` có trên bàn (số ô `Pn` = số vùng). |
| `S1`…`S5` | Các vùng cùng số `Sn` phải **cùng hình dạng**; số khác nhau → hình dạng khác nhau. Con số không phải diện tích. |

### Cạnh (giữa hai ô kề nhau)

Dọc dùng 1 ký tự, ngang dùng 2 ký tự lặp lại.

| Dọc | Ngang | Ý nghĩa |
|---|---|---|
| `\|` | `--` | cạnh bình thường, chưa quyết định |
| `#` | `##` | tường có sẵn — hai ô chắc chắn khác vùng |
| `=` | `==` | hai ô khác vùng, hai vùng **cùng hình dạng** (cho xoay + lật) |
| `!` | `!!` | hai ô khác vùng, hai vùng **khác hình dạng** |
| `<` / `>` | `^^` / `vv` | hai ô khác vùng, mũi nhọn chỉ về **vùng có diện tích nhỏ hơn** |

### Luật toàn bàn (header)

| Header | Ý nghĩa |
|---|---|
| `SHAPE_BANK a b …` | Mọi vùng phải trùng một trong các `SHAPE` có id liệt kê (cho xoay + lật; 25/233 vùng trong demo cần lật). |
| `AREA_EQUALS n` | Mọi vùng có diện tích `n`. |
| `NO_RECTANGLES` | Không vùng nào được là hình chữ nhật. |

"Cùng hình dạng" = trùng nhau sau khi xoay 90° và/hoặc lật gương (`puz.canon`).

## Các pack trong demo (theo thư mục)

| Thư mục | Cơ chế chính |
|---|---|
| `1-single-shape` | `SHAPE_BANK` 1 hình |
| `2-shape-bank` | `SHAPE_BANK` 2–3 hình |
| `3-gemini-delta`, `6-gemini-delta` | cạnh `=` / `!` kết hợp shape bank |
| `4-slash-pack`, `7-slash-pack` | ô `P1..P4` |
| `Zone1/*demo` | mỗi file giới thiệu 1 cơ chế (`S`, mũi tên, `AREA_EQUALS`, `NO_RECTANGLES`…) |

`Data/default.puz` là bàn 6×6 trống, không có lời giải (template). `1-single-shape/0067.puz` cũng không có `SOLUTION`.

## Dùng parser

```bash
python tools/puz.py <thư mục chứa .puz> <thư mục xuất json>
```

```python
import puz
p = puz.parse("0607demo.puz")   # cells, vedges, hedges, header, shapes, regions
puz.check(p)                    # [] nếu lời giải hợp lệ
puz.check(p, my_regions)        # kiểm một cách chia vùng khác (list các list (r, c))
```

## Chuyển sang `.pzl` (định dạng level của project)

```bash
python tools/puz2pzl.py <thư mục chứa .puz> [thư mục xuất] [--lossy]
```

- Mặc định xuất ra `D:\GitHub\Glimmith_Reference\pzl`, **ngoài `Assets/`**: mọi `.pzl` trong `Assets/` đều vào catalog + test, và [level-format.md](design/level-format.md) cấm chép level game khác. Chỉ dùng để tham khảo/thử cơ chế.
- `.pzl` hiện chỉ có `rule shape-bank` và chưa có ranh giới cho sẵn → chỉ level dùng **thuần shape bank** mới chuyển được (17/54 ở demo). Level khác bị bỏ qua kèm lý do.
- `--lossy`: vẫn ghi level có clue không hỗ trợ (35/54), liệt kê clue bị bỏ trong comment đầu file — lời giải khi đó có thể không còn duy nhất.
- Hình `SHAPE <id>` → `shape S<id>`; vùng → nhãn `A–Z0–9`; hàng/cột trống ở rìa được cắt; id = `glim-<tên file>`.

Muốn chuyển nốt phần còn lại cần thêm vào game: luật `=`/`!` (cùng/khác hình), mũi tên diện tích, `P`, `S`, `AREA_EQUALS`, `NO_RECTANGLES`, và ranh giới cho sẵn (`given`).
