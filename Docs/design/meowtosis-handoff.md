# Meowtosis — lưu giữ prototype và chuyển sang Unity

Prototype HTML (`Prototype/games/mitoosis.html`) là bản **đặc tả chơi được** của Meowtosis. Tài liệu này ghi lại prototype có những gì, phần nào dùng lại nguyên được trong bản Unity, phần nào chỉ để tham khảo, phần nào phải bỏ, và cách chuyển để không mất gì.

> Cập nhật lần cuối: 2026-11-03 · 36 level · 5 chương

---

## 1. Hiện có gì, đang nằm ở đâu

| Thứ | Vị trí | Trong git? | Bản Unity dùng thế nào |
|---|---|---|---|
| Game (luật, level, giao diện, VFX, SFX) | `Prototype/games/mitoosis.html` (một file) | **Chưa commit** | Đặc tả + nguồn dữ liệu level và luật |
| Bản giao diện kẹo dẻo cũ | `Prototype/games/mitoosis-keo-deo.html` | Chưa commit | Tham khảo phong cách hình tự vẽ |
| Tiếng mèo (Mixkit) | `Prototype/games/sfx/*.mp3` + `NGUON.txt` | Chưa commit | **Dùng được** trong sản phẩm (xem mục 6) |
| Asset Shikacat (mèo, hộp, ô, icon, nhạc) | `Prototype/games/shikacat/` | **Không** (bị `.gitignore`) | **Không dùng được** trong sản phẩm, chỉ là hình tạm |
| Máy chủ chơi thử | `Prototype/serve.js` | Chưa commit | Chỉ cho prototype |
| Bộ giải, bộ sinh level, test | Thư mục tạm của phiên Claude (`%TEMP%\claude\...\scratchpad\`) | **Không**, có thể mất | Nên chép vào repo (mục 9) |

**Việc nên làm ngay:** commit thư mục `Prototype/` (thư mục `shikacat/` đã bị loại khỏi git nên không lo lộ asset) và chép bộ công cụ từ thư mục tạm vào repo.

---

## 2. Ba nhóm theo khả năng dùng lại

| Nhóm | Gồm | Cách dùng ở Unity |
|---|---|---|
| **Dùng nguyên** | Dữ liệu 36 level, luật chơi, lời thoại hướng dẫn, các con số cân bằng (tim, sao, xu, booster), tiếng mèo Mixkit, phông Baloo 2 | Xuất ra file dữ liệu, nhập vào Unity; luật chép sang C# |
| **Dùng làm đặc tả** | Bố cục màn hình, luồng chơi, hướng dẫn tương tác, danh sách VFX/SFX, cách vẽ khung bàn chơi, hình kẹo dẻo tự vẽ | Quay video/chụp màn hình từng màn; đội art, UI, VFX làm lại theo |
| **Phải bỏ** | Toàn bộ asset Shikacat; âm thanh tổng hợp bằng Web Audio; code giao diện HTML/SVG | Thay bằng art, âm thanh và UI của team |

---

## 3. Dữ liệu level

### 3.1 Định dạng hiện tại

Mỗi level là một phần tử trong mảng `LEVELS` (khối `// ==LOGIC==` của `mitoosis.html`). Lưới viết bằng ký tự:

| Ký tự | Nghĩa |
|---|---|
| `#` | Ngoài cơ thể (không phải ô) |
| `.` | Ô trống |
| `~` | Ô độc (không cần lấp) |
| `A` `B` `C` | Mèo đỏ, xanh, vàng |
| `a` `b` `c` | Mèo ngủ (màu tương ứng) |
| `G` | Vi khuẩn |
| `^` `v` `<` `>` | Virus, mũi tên là hướng lan |

Các trường thêm (đều tuỳ chọn):

| Trường | Ví dụ | Nghĩa |
|---|---|---|
| `goal` | `['..B.']` | Lưới cùng kích thước; chữ cái = hộp mục tiêu cần mèo màu đó |
| `oneway` | `[[0, 1, 'left']]` | Màng một chiều ở ô (hàng 0, cột 1), chỉ cho đi qua theo hướng `left` |
| `kill` | `'G'` | Level diệt khuẩn |
| `tut` | `[[0, 0, 'right', 'Lời thoại']]` | Các bước hướng dẫn: hàng, cột, hướng vuốt, lời thoại |

Ví dụ level 13 (nhập môn hộp mục tiêu):

```js
{ rows: ['AB..'], goal: ['..B.'], tut: [
  [0, 0, 'right', 'Đẩy mèo xanh vào hộp xanh'],
  [0, 2, 'right', 'Lấp nốt ô cuối: vừa kín ổ vừa đúng hộp!']] }
```

Chương được khai báo riêng trong mảng `CHAPTERS` (theo chỉ số level):

| Chương | Level | Nội dung |
|---|---|---|
| 1 · Phòng nuôi cấy | 1–12 | Chia, đẩy, thay thế, khử 3 |
| 2 · Vào vị trí | 13–21 | Hộp mục tiêu |
| 3 · Màng và độc | 22–26 | Màng một chiều, ô độc |
| 4 · Ngủ và virus | 27–32 | Mèo ngủ, virus |
| 5 · Diệt khuẩn | 33–36 | Diệt khuẩn, level tổng hợp |

### 3.2 Xuất ra JSON

Đoạn script dưới đây đọc khối LOGIC, chạy bộ giải và xuất mọi level kèm **số nước tối ưu (`par`)** và **một lời giải**. Đã chạy thử: 36 level, không level nào thiếu lời giải.

```js
// node export-levels.js > levels.json   (chạy ở thư mục gốc repo)
const fs = require('fs'), vm = require('vm');
const html = fs.readFileSync('Prototype/games/mitoosis.html', 'utf8');
const logic = html.match(/\/\/ ==LOGIC==([\s\S]*?)\/\/ ==\/LOGIC==/)[1];
const ctx = {}; vm.createContext(ctx);
vm.runInContext(logic + '\nObject.assign(globalThis, { LEVELS, parseLevel, solve });', ctx);
const levels = ctx.LEVELS.map((d, i) => {
  const def = Array.isArray(d) ? { rows: d } : d;
  const lv = ctx.parseLevel(def), path = ctx.solve(lv, lv.init);
  return { index: i + 1, ...JSON.parse(JSON.stringify(def)), par: path ? path.length : null,
    solution: path ? path.map(([from, dir]) => [Math.floor(from / lv.w), from % lv.w, dir]) : null };
});
process.stdout.write(JSON.stringify(levels, null, 1));
```

### 3.3 Định dạng đề xuất cho Unity

Đi theo cách repo đang làm với `.pzl` (`Docs/design/level-format.md`): **mỗi level một file văn bản**, Unity tự nhập thành `LevelAsset` bằng `ScriptedImporter`. Sửa level là sửa file, diff được trên git.

```text
id: meow-c2-01            // khoá lưu tiến độ, cố định, KHÔNG phụ thuộc thứ tự
chapter: 2
par: 2                    // số nước tối ưu (bộ giải tính, test kiểm tra lại)

rows
AB..

goal
..B.

oneway 0 1 left           // có thể lặp nhiều dòng
kill G

tut 0 0 right  Đẩy mèo xanh vào hộp xanh
tut 0 2 right  Lấp nốt ô cuối: vừa kín ổ vừa đúng hộp!
```

Ghi chú:
- **`id` cố định.** Prototype hiện lưu tiến độ theo *nội dung* level, vì chưa có id. Bản Unity nên có id riêng để sửa nội dung level mà không mất sao của người chơi.
- **Lời thoại nên tách ra bảng dịch** (`tut` chỉ giữ khoá), để làm đa ngôn ngữ về sau.
- **Đưa vào test `LevelCatalogTests`:** mọi level giải được, `par` khớp bộ giải, các bước `tut` đi được và (nếu đi trọn) dẫn tới thắng đúng bằng `par`, hai level không trùng khung (kể cả xoay/lật).

---

## 4. Luật chơi: chép sang C#

Luật nằm trọn trong khối `// ==LOGIC==`, viết bằng hàm thuần (không đụng giao diện), nên chép sang tầng **Domain** C# gần như 1–1.

| Hàm JS | Việc | Gợi ý C# |
|---|---|---|
| `parseLevel` | Đọc lưới, hộp, màng; đánh thức sẵn mèo ngủ đứng cạnh mèo cùng màu | `LevelLoader` → `BoardShape` + trạng thái ban đầu |
| `step` | Ô kế tiếp theo hướng, tính cả màng chặn | `BoardShape.Step(cell, dir)` |
| `swipe` | Một lần vuốt: chia, đẩy cả dãy hoặc thay thế, rồi `resolve` | `SwipeCommand` |
| `resolve` | Kết thúc lượt (thứ tự bên dưới) | `TurnResolver` |
| `findRuns` | Dãy từ 3 cùng màu trở lên (virus không tính) | `RunFinder` |
| `mutate`, `transplant` | Booster Đột biến, Cấy ghép | `IBooster` |
| `isFull`, `isComplete`, `isLost`, `extinct` | Thắng/thua | `WinRule`, `LoseRule` |
| `solve` | BFS tìm lời giải ngắn nhất (Gợi ý, tính `par`) | `Solver` (chạy nền, có giới hạn trạng thái) |

**Thứ tự kết thúc một lượt** (phải giữ đúng, nếu không level sẽ lệch lời giải):
1. Đặt bản sao: chia vào ô trống; ô kề có mèo thì đẩy cả dãy tới ô trống gần nhất; không còn ô trống thì bản sao **thay thế** ô kề (thay thế cùng màu = nước không hợp lệ).
2. Ô độc giết mọi thứ đang đứng trên nó.
3. Khử mọi dãy ≥ 3 cùng màu (mèo ngủ cũng tính, virus không tính).
4. Mỗi virus lan 1 ô theo hướng của nó: vào ô trống hoặc ăn mèo; không đi vào virus khác, bị màng chặn.
5. Ô độc giết tiếp (virus vừa lan vào ô độc).
6. Mèo ngủ đứng cạnh mèo cùng màu đang thức thì thức dậy (lan dây chuyền).

**Thắng:** mọi ô (trừ ô độc) có mèo, không phải virus · mọi hộp có mèo đúng màu · không còn vi khuẩn.
**Thua:** một màu mèo có mặt lúc đầu không còn con nào (kể cả mèo ngủ). Vi khuẩn và virus không tính là loài.

**Kiểm tra chéo khi chép:** dùng file JSON xuất ở mục 3.2 làm "đáp án vàng". Với mỗi level, test C# phải cho ra cùng `par`, và chạy lại `solution` của JS phải thắng. Các ca test luật trong bộ test prototype (mục 9) chép sang NUnit.

---

## 5. Hệ thống meta và các con số

| Hệ thống | Giá trị trong prototype | Ghi chú |
|---|---|---|
| Tim | 3 tim, thua mất 1, hồi 1 tim mỗi 10 phút | `MAX_HEARTS`, `REGEN_MS` |
| Booster | Mỗi loại 3 lượt; hoàn tác nước dùng booster thì trả lại lượt | Đột biến, Cấy ghép, Hồi sinh (chỉ dùng khi vừa thua, trả lại tim) |
| Sao | Bằng `par`: 3 sao · dư ≤ 2 nước: 2 sao · còn lại: 1 sao | Booster tính là một nước |
| Xu | 10 xu mỗi sao, cộng mỗi lần thắng | `COINS_PER_STAR`; chưa có chỗ tiêu; chơi lại vẫn được thưởng đủ |
| Tiến độ | Sao và kỷ lục tốt nhất mỗi level | Unity: lưu theo `id` level |
| Mở khoá | Theo thứ tự, **đang tắt** để test (`LOCK_IN_ORDER = false`) | |
| Gợi ý | Bộ giải chạy từ thế cờ hiện tại, chỉ mũi tên nước tiếp | Giới hạn 80.000 trạng thái; chậm nhất là level 21 (11 nước), khoảng 0,8 giây trên PC, trên điện thoại sẽ lâu hơn nên Unity cần chạy nền |

**Onboarding (2 lớp):**
1. **Thẻ hướng dẫn** hiện lần đầu gặp mỗi cơ chế (`MECHANICS`): ảnh trước/sau một lần vuốt, ảnh "sau" do chính luật tính ra.
2. **Hướng dẫn tương tác** ở 10 level (1, 2, 4, 9, 13, 22, 25, 27, 30, 33): làm tối bàn trừ ô cần chú ý, bàn tay chỉ hướng vuốt, bong bóng lời thoại; chỉ nhận đúng nước được chỉ; hoàn tác thì lùi bước.

---

## 6. Asset: cái gì giữ, cái gì thay

### 6.1 Giấy phép

| Nguồn | Dùng trong sản phẩm? | Lưu ý |
|---|---|---|
| Shikacat (Tripledot) | **Không** | Asset tách từ game khác, chỉ để thử nội bộ. Xoá khỏi máy build, không đưa lên store |
| Mixkit (tiếng mèo) | **Được** | Mixkit Sound Effects Free License: dùng trong game thương mại, không cần ghi nguồn; không phân phối lại file âm thanh đứng riêng. Kiểm tra lại điều khoản lúc phát hành |
| Baloo 2 (Google Fonts) | **Được** | SIL Open Font License. Unity: tạo font asset TextMeshPro, nhớ bộ ký tự tiếng Việt |
| Hình kẹo dẻo, khung bàn, hiệu ứng | **Được** | Tự vẽ bằng code trong prototype |

### 6.2 Danh sách art cần làm (thay hình tạm)

| Hạng mục | Prototype đang dùng | Cần cho bản Unity |
|---|---|---|
| Mèo 3 màu | Mèo Shikacat đỏ/xanh/vàng | Mèo của team, **mỗi màu một đặc điểm riêng ngoài màu** (nơ, tai, hoa văn) để người mù màu phân biệt được |
| Mèo vui | Mèo mắt sao/mắt tim (Shikacat) | Biểu cảm khi vào đúng hộp và khi thắng |
| Mèo ngủ | Mèo xám + bịt mắt vẽ thêm | Mèo ngủ **giữ nguyên màu**, nhắm mắt + chữ z |
| Virus, vi khuẩn | Tự vẽ (quả cầu gai, viên nang có râu) | Vẽ lại cùng phong cách |
| Khung bàn, ô | Khung gỗ tự vẽ + ô kem (`Tile_Board` Shikacat) | Khung 9-slice hoặc tự dựng theo đường viền liền (xem `outlinePath`) |
| Ô độc, màng | Ô xanh + xương cá (Shikacat), dải thạch xanh ngọc | Vẽ lại |
| Hộp mục tiêu | `Panel_Box_*` (Shikacat) | Hộp theo màu của team |
| UI | Tim, ổ khoá (Shikacat); nút, thẻ, xu tự vẽ | Bộ UI của team |
| Minh hoạ thắng/thua | Mèo trên phao, mèo khóc (Shikacat) | Minh hoạ của team |
| Nền | Nền xanh (Shikacat) | Nền của team |

### 6.3 Âm thanh

| Sự kiện | Prototype | Bản Unity |
|---|---|---|
| Mèo chia (ngẫu nhiên, cao độ theo màu) | 4 file Mixkit, đổi cao độ: xanh ×0,86, đỏ ×1, vàng ×1,16 | Giữ hoặc thu thêm biến thể |
| Mèo giận (nước không hợp lệ) | Mixkit | Giữ |
| Nút, popup, thắng, thua, nhạc nền, vào đúng hộp, khử 3 | Shikacat | **Thay** |
| Đẩy, thay thế, ô độc, virus ăn, thức dậy, booster, hồi sinh, xu bay, ka-ching | Tổng hợp Web Audio | Sound designer làm file thật |

---

## 7. VFX cần dựng lại (Unity Particle System / tween)

| Sự kiện | Hiệu ứng trong prototype |
|---|---|
| Chia | Mèo mẹ co giãn theo hướng vuốt; bản sao bật ra (đàn hồi) |
| Khử 3 | Mèo vỡ thành giọt màu, sóng tròn, chữ "Khử 3!", rung bàn |
| Ô độc | Khói xanh bốc lên; bọt sủi liên tục trên ô |
| Virus ăn | Vệt tím bắn ra; gai virus xoay chậm |
| Thức dậy | Lấp lánh, nảy lên, chữ z biến mất |
| Vào đúng hộp | Hộp sáng lên, lấp lánh, mèo đổi mắt sao |
| Thắng | Pháo giấy, cả ổ mèo nhảy + mắt sao; sao bay vào bảng thắng |
| Thua | Rung bàn, mèo xụ xuống |
| Booster | Đột biến: vòng cầu vồng · Cấy ghép: vệt sáng bay tới ô đích · Hồi sinh: loé vàng, sao bay lên, tim bay về |
| Xu | 4–12 đồng bay theo đường cong từ bảng thắng lên ô xu; ô xu nảy và đếm dần |
| Mèo lúc nghỉ | Thở phập phồng, chớp mắt; nghiêng người theo hướng vuốt |
| Hướng dẫn | Làm tối theo hình khung, bàn tay lướt, vệt chấm chạy |

---

## 8. Kế hoạch chuyển

1. **Đóng băng prototype:** commit `Prototype/`, gắn tag (ví dụ `meowtosis-proto-v1`), quay video từng màn hình làm tài liệu tham khảo.
2. **Xuất dữ liệu:** chạy script mục 3.2 ra `levels.json`; đổi sang file level Unity (mục 3.3), đặt `id` cho từng level.
3. **Domain C#:** chép luật (mục 4) và test; test vàng so `par` và lời giải với `levels.json`.
4. **Bộ giải + công cụ level:** chép bộ giải sang C# (Gợi ý, kiểm tra level trong test); bộ sinh level có thể giữ bằng Node hoặc viết lại thành Editor tool.
5. **Application:** phiên chơi, hoàn tác, booster, tim, xu, tiến độ theo `id` (theo khuôn `PuzzleSession`, command, `IProgressStore` trong `ARCHITECTURE.md`).
6. **Presentation:** bàn chơi, HUD, bản đồ chương, thẻ hướng dẫn, hướng dẫn tương tác, bảng thắng/thua (View câm + Presenter).
7. **Art, âm thanh, VFX của team** thay hình tạm (mục 6–7); xoá mọi asset Shikacat.
8. **Cân bằng lại** khi có playtest: độ khó từng chương, mốc sao, giá booster bằng xu.

---

## 9. Bộ công cụ của prototype

Đang nằm trong thư mục tạm của phiên Claude, **nên chép vào repo** (ví dụ `Prototype/tools/`) và đổi đường dẫn tuyệt đối thành đường dẫn tương đối:

| File | Việc |
|---|---|
| `harness.js` | Đọc khối LOGIC trong file HTML để chạy bằng Node |
| `gen-mech.js`, `gen-frame.js` | Bộ sinh level: đặt mèo/blocker ngẫu nhiên trên một khung, giữ level giải được và cơ chế thật sự cần |
| `check-frames.js`, `shapes.js` | Kiểm tra không có hai level trùng khung (kể cả xoay/lật) |
| `test-mi3.js` … `test-mi5.js`, `test-tut.js` | Test luật, booster, cơ chế mới, các bước hướng dẫn |
| `rate-mi3.js` | Đo độ khó: số nước tối ưu, tỷ lệ thắng/thua khi chơi ngẫu nhiên |
| `export-check.js` | Script xuất level ra JSON (mục 3.2) |

---

## 10. Các quyết định thiết kế đã chốt

| Quyết định | Lý do |
|---|---|
| Mọi level cùng mục tiêu **lấp kín**; cơ chế chỉ thêm điều kiện | Góp ý playtest: đổi mục tiêu giữa chừng làm người chơi ngợp |
| Bảng nhiệm vụ luôn hiện (`Lấp kín 3/9 · Hộp 1/2 · Vi khuẩn còn 2`) | Người chơi luôn biết còn thiếu gì |
| Mỗi level một khung riêng (36 khung, không trùng kể cả xoay/lật) | Mỗi màn có cảm giác mới |
| Không dùng khung vòng khuyên, chữ C | Không bao giờ lấp kín được (tối đa 6 mèo) |
| Màu là danh tính, trạng thái báo bằng kênh khác | Mèo ngủ bị xám làm người chơi không biết màu |
| Khử 3 và ô độc được dạy bằng hướng dẫn tương tác thay vì level nhỏ riêng | Trên khung nhỏ, lời giải ngắn nhất không bao giờ cần chúng (chúng làm mất mèo) |
| Hướng dẫn đi trọn lời giải phải bằng `par` (riêng level 9 chỉ dẫn 4/8 nước đầu, phần còn lại người chơi tự làm) | Người làm theo luôn được 3 sao |
| Tên game Meowtosis | Gợi mèo + nguyên phân, tách khỏi tên Mitoosis của Le Slo (game lấy cảm hứng cơ chế) |
