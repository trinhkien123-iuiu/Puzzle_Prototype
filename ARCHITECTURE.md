# PUZZLE PROTOTYPE — QUY TẮC KIẾN TRÚC

> Đọc file này TRƯỚC khi thêm code. Đây là bộ quy tắc **mới, viết riêng cho dự án này**, không kế thừa quy ước của dự án cũ.
> Mục tiêu: thêm tính năng mới **không phải sửa code cũ**, muốn đổi gì **biết ngay vào đâu**, dự án **không phình**.
> Khi code lệch khỏi file này: sửa code, hoặc sửa file này kèm lý do trong `Docs/decisions.md`.

- **Engine:** Unity 2022.3.62f3, URP 2D, màn hình dọc, thao tác một ngón
- **Thể loại:** puzzle chia vùng trên lưới (phân tích tham khảo: `Docs/research/glimmith-rules.md`)

---

## 0. Năm nguyên tắc gốc

1. **Logic là C# thuần, Unity chỉ là lớp hiển thị.** Luật, trạng thái, kiểm tra, gợi ý không biết đến `MonoBehaviour`. Nhờ vậy test được trong vài mili-giây và chạy được solver ngoài scene.
2. **Mở rộng bằng cách thêm file, không sửa file.** Luật mới, booster mới, hiệu ứng mới đều là một lớp mới cắm vào chỗ đã chừa sẵn.
3. **Một nguồn sự thật.** Trạng thái puzzle chỉ nằm trong `PuzzleState`. Mọi thứ khác (vùng, lỗi, hình vẽ) là **tính ra** từ nó.
4. **Mọi thay đổi trạng thái đi qua Command.** Nhờ vậy có undo/redo, replay, booster dùng chung đường với người chơi.
5. **Dữ liệu là asset, không phải code.** Level, thông số, màu, âm thanh chỉnh trong Inspector, không nằm trong `const`.

---

## 0b. SOLID — áp dụng cụ thể trong dự án

SOLID là thước đo khi review code. Mỗi nguyên tắc dưới đây có **quy tắc cụ thể** và **dấu hiệu vi phạm** để tự kiểm tra.

### S — Single Responsibility (mỗi lớp một lý do để thay đổi)

| Lớp | Một việc duy nhất |
|---|---|
| `RegionMap` | Tính vùng từ trạng thái cạnh |
| Mỗi `XxxRule` | Kiểm tra đúng một luật |
| `PuzzleValidator` | Chạy danh sách luật, gom lỗi; không chứa luật nào |
| `PuzzleSession` | Điều phối command + undo + phát sự kiện; không kiểm tra luật trực tiếp |
| `BoardView` | Vẽ bàn từ snapshot; không đọc input |
| `BoardInput` | Đổi chạm/kéo thành ý định; không vẽ, không biết luật |
| `XxxPresenter` | Nối một màn hình với Application; không vẽ |

**Dấu hiệu vi phạm:** tên lớp có "And" hoặc "Manager"; mô tả lớp cần chữ "và"; một thay đổi về hình ảnh bắt sửa lớp logic, hoặc ngược lại.

### O — Open/Closed (mở để mở rộng, đóng để sửa)

- Luật, booster, command, hiệu ứng, kho lưu tiến độ đều là **interface + nhiều cài đặt**. Thêm loại mới = thêm lớp mới, đăng ký ở **một chỗ** (`RuleRegistry`, `BoosterConfig`, `GameBootstrap`).
- **Cấm** `switch`/`if` theo loại luật hay loại booster ở bên ngoài lớp đó (ví dụ `if (rule is GeminiRule)` trong View). Hành vi riêng của loại nào thì nằm trong lớp của loại đó; nếu View cần vẽ khác nhau thì dùng `ClueView` riêng, chọn qua `ClueVisuals`.
- Chỗ được phép có `switch` theo loại: chỉ trong registry/factory, nơi đọc dữ liệu thành đối tượng.

**Dấu hiệu vi phạm:** thêm luật mới mà phải sửa một file không nằm trong thư mục của luật đó (ngoài registry và test).

### L — Liskov Substitution (lớp con thay được lớp cha mà không phá vỡ)

- Mọi `IRule` tuân **cùng một hợp đồng**: chỉ đọc `RuleContext`, không ném exception với trạng thái dở dang, trả lỗi qua `violations`, chạy nhanh (không giải puzzle bên trong).
- Mọi `ICommand`: `Undo()` sau `Do()` phải trả `PuzzleState` về **đúng như cũ**. Test chung `CommandContractTests` chạy cho mọi command.
- Mọi `IBooster`: `CanUse() == false` thì `CreateCommand()` không được gọi; không booster nào tự sửa state ngoài command.
- Mọi `IProgressStore`: lưu rồi đọc lại phải ra cùng dữ liệu. Bản `PlayerPrefs` và bản giả dùng trong test đều qua cùng một bộ test.

**Dấu hiệu vi phạm:** code gọi phải kiểm tra kiểu cụ thể trước khi dùng; một cài đặt ném `NotSupportedException`; test chạy được với cài đặt này mà hỏng với cài đặt khác.

### I — Interface Segregation (interface nhỏ, đúng nhu cầu)

- Tách theo người dùng: `IPuzzleReader` (đọc snapshot, cho View/Presenter) tách khỏi `IPuzzleCommands` (thực thi command, cho Input/Booster). Presenter chỉ hiển thị thì chỉ nhận `IPuzzleReader`.
- `IRule` chỉ có một hàm `Check`. Thông tin hiển thị (tên, mô tả, icon) **không** nằm trong `IRule`, mà nằm ở `ClueVisuals` (tầng Presentation).
- Interface quá 5 thành viên là dấu hiệu cần tách.

**Dấu hiệu vi phạm:** một cài đặt để trống hoặc ném lỗi ở vài hàm của interface; lớp nhận vào một interface lớn nhưng chỉ dùng một hàm.

### D — Dependency Inversion (phụ thuộc vào trừu tượng, không vào cụ thể)

- Tầng trên phụ thuộc **interface** do tầng dưới định nghĩa: Application định nghĩa `IProgressStore`, `IClock`, `IRandom`; cài đặt cụ thể (`PlayerPrefsProgressStore`, `UnityClock`) nằm ở Presentation/Bootstrap.
- Mọi phụ thuộc được **truyền vào qua constructor** (C# thuần) hoặc `Construct(...)` (MonoBehaviour), gọi từ `GameBootstrap`. Không `new` dịch vụ bên trong lớp dùng nó, không Singleton, không `static` giữ trạng thái.
- Domain không phụ thuộc gì cả, nên thời gian, ngẫu nhiên, log đều đi qua interface nếu Domain cần.

**Dấu hiệu vi phạm:** `new PlayerPrefsProgressStore()` nằm trong Presenter; `Time.time` hoặc `UnityEngine.Random` xuất hiện trong Domain/Application; test phải mở scene mới chạy được.

### Cân bằng: không over-engineering

- Chỉ tạo interface khi có **ít nhất 2 cài đặt** (kể cả bản giả trong test) hoặc khi nó là ranh giới giữa các tầng. Lớp nội bộ một cài đặt thì để lớp cụ thể.
- Không tạo lớp trừu tượng "để sau này dùng". Khi cài đặt thứ hai xuất hiện mới tách.
- Prototype ưu tiên **chạy được và đổi được nhanh**; SOLID phục vụ mục tiêu đó, không phải ngược lại.

---

## 1. Các tầng và hướng phụ thuộc

```
  Presentation   Puzzle.Presentation   View (MonoBehaviour, câm) + Presenter (C# thuần)
        │
        ▼
  Application    Puzzle.Application    PuzzleSession, Command, Booster, tiến độ, luồng màn chơi
        │
        ▼
  Domain         Puzzle.Domain         Lưới, hình, luật, kiểm tra, solver — KHÔNG có UnityEngine
```

Phụ thuộc **chỉ đi xuống**. Tầng dưới không bao giờ biết tầng trên.

| Assembly | Tham chiếu được | Ghi chú |
|---|---|---|
| `Puzzle.Domain` | không gì | Bật `noEngineReferences: true` trong asmdef để Unity chặn `using UnityEngine` |
| `Puzzle.Application` | Domain | Có thể dùng `UnityEngine` cho ScriptableObject cấu hình, nhưng không dùng `MonoBehaviour` |
| `Puzzle.Presentation` | Domain, Application | Nơi duy nhất có View, Input, Animation, Audio |
| `Puzzle.Bootstrap` | tất cả | Chỉ chứa composition root (mục 5) |
| `Puzzle.Editor` | tất cả | Chỉ nền tảng Editor |
| `Puzzle.Tests` | Domain, Application | EditMode test |

---

## 2. Cấu trúc thư mục

```
Assets/_Project/
├── Code/
│   ├── Domain/                    Puzzle.Domain
│   │   ├── Grid/                  Cell, Edge, EdgeState, BoardShape
│   │   ├── Shapes/                Polyomino, ShapeVariants (xoay/lật)
│   │   ├── Regions/               RegionMap (tính vùng bằng union-find)
│   │   ├── Rules/                 IRule, RuleContext, RuleViolation
│   │   │   └── <TenLuat>/         MỖI LUẬT MỘT THƯ MỤC: XxxRule.cs (+ XxxClue.cs nếu có ký hiệu)
│   │   ├── Puzzle/                PuzzleDefinition, PuzzleState, PuzzleValidator
│   │   └── Solving/               Solver, HintFinder
│   ├── Application/               Puzzle.Application
│   │   ├── Session/               PuzzleSession, ICommand, UndoHistory
│   │   ├── Commands/              MergeCellsCommand, SetEdgeCommand, ClearCommand…
│   │   ├── Boosters/              IBooster + mỗi booster một file
│   │   ├── Levels/                LevelCatalog, LevelLoader (asset → PuzzleDefinition)
│   │   └── Progress/              IProgressStore, PlayerProgress
│   ├── Presentation/              Puzzle.Presentation
│   │   ├── Board/                 BoardView, CellView, EdgeView, ClueView
│   │   ├── Input/                 BoardInput (chạm/kéo → ý định)
│   │   ├── Screens/               mỗi màn hình/popup: XxxView.cs + XxxPresenter.cs
│   │   ├── Feedback/              hiệu ứng, rung, âm thanh theo sự kiện
│   │   └── Pooling/               pool cho ô/cạnh/hiệu ứng
│   ├── Bootstrap/                 Puzzle.Bootstrap: GameBootstrap (composition root)
│   └── Editor/                    Puzzle.Editor: Level Editor, validator dữ liệu
├── Tests/                         Puzzle.Tests
├── Config/                        ScriptableObject cấu hình, CHIA THEO MẢNG (mục 7)
├── Levels/<pack>/                 LevelPack + LevelAsset
├── Prefabs/{Board,Screens,Feedback}/
├── Art/<tính năng>/               Board/, Clues/, Screens/, Feedback/, Shared/
├── Audio/{Sfx,Music}/
├── Shaders/                       Shader Graph
└── Scenes/                        Boot, Game (prototype chỉ cần 2 scene)
```

Ngoài repo: asset tham khảo từ game khác (`D:\GitHub\Glimmith_Reference`). Không đưa lên git: `Library/`, `*.csproj`, `*.sln`, ảnh chụp tạm.

---

## 3. Domain: lưới, luật, kiểm tra

**Mô hình:**

| Khái niệm | Kiểu | Ghi chú |
|---|---|---|
| `Cell` | `readonly struct (x, y)` | Bất biến, dùng làm khoá |
| `Edge` | `readonly struct (Cell a, Cell b)` chuẩn hoá `a < b` | |
| `EdgeState` | enum `Unknown / Border / Joined` | |
| `BoardShape` | lớp bất biến | Ô nào tồn tại (bàn có thể khuyết) |
| `PuzzleDefinition` | lớp bất biến | Bàn + luật + ký hiệu + lời giải |
| `PuzzleState` | lớp có thể đổi | Trạng thái từng cạnh + màu tô; chỉ đổi qua Command |
| `RegionMap` | tính ra | Không lưu, không cache ngoài `PuzzleSession` |

**Luật:**

```csharp
/// Một luật puzzle. Chỉ đọc trạng thái, không bao giờ sửa.
public interface IRule
{
    void Check(in RuleContext ctx, List<RuleViolation> violations);
}
```

- `RuleContext` gom mọi thứ luật cần: bàn, `RegionMap`, ký hiệu, `ShapeVariants`. Luật cần thêm dữ liệu → thêm vào `RuleContext`, không cho luật tự đi lấy.
- `RuleViolation` chỉ rõ **luật nào, ô/cạnh/vùng nào**. Presentation dùng nó để tô lỗi; booster "Kiểm tra" dùng nó để báo.
- Luật **phân biệt** "chưa xong" và "sai chắc chắn" (`Severity.Incomplete / Broken`) để không báo đỏ khi người chơi đang vẽ dở.
- Danh sách luật có trong game nằm ở **một chỗ duy nhất**: `RuleRegistry` (map tên → hàm tạo, dùng khi đọc level).

**Checklist thêm luật mới** (không làm khác):
1. Tạo thư mục `Domain/Rules/<TenLuat>/` với `<TenLuat>Rule.cs`.
2. Đăng ký vào `RuleRegistry`.
3. Thêm test `Tests/Rules/<TenLuat>RuleTests.cs`: ít nhất 1 lời giải đúng, 1 lời giải sai, 1 trạng thái dở dang.
4. Nếu có ký hiệu trên bàn: thêm `ClueView` + mục trong `ClueVisuals.asset`.
5. Ghi luật vào `Docs/design/rules.md`.

Nếu thêm luật mà phải sửa `PuzzleValidator`, `BoardView` hay một luật khác → thiết kế đang sai, dừng lại và sửa `RuleContext`.

---

## 4. Application: phiên chơi, command, booster

- `PuzzleSession` là **cửa duy nhất** để thay đổi puzzle: `Execute(ICommand)`, `Undo()`, `Redo()`. Sau mỗi lệnh nó tính lại vùng, gọi `PuzzleValidator`, rồi phát **một** sự kiện `Changed(SessionSnapshot)`.
- `PuzzleSession` cài hai interface tách biệt (mục 0b, chữ I): `IPuzzleReader` (snapshot + sự kiện `Changed`) và `IPuzzleCommands` (`Execute`, `Undo`, `Redo`). Ai chỉ cần đọc thì chỉ nhận `IPuzzleReader`.
- `SessionSnapshot` là dữ liệu chỉ đọc (vùng, lỗi, đã giải chưa). View vẽ từ snapshot, không hỏi lại session.
- `ICommand { void Do(PuzzleState); void Undo(PuzzleState); }`. Command nhỏ, mỗi command một thao tác của người chơi.
- Booster cài `IBooster { bool CanUse(IPuzzleReader); ICommand CreateCommand(IPuzzleReader); }`. Booster **trả về command**, không tự sửa state, nên luôn undo được và dùng chung luồng kiểm tra.
- Gợi ý lấy từ `HintFinder` (Domain), dựa trên lời giải có sẵn trong level hoặc solver.
- Lưu tiến độ qua interface `IProgressStore` (định nghĩa ở Application). Cài đặt `PlayerPrefsProgressStore` nằm ở Presentation; đổi sang file/cloud không sửa code gọi.
- Application cần thời gian hoặc ngẫu nhiên thì dùng `IClock`, `IRandom`, không gọi thẳng `Time` hay `UnityEngine.Random`.

---

## 5. Composition root: nối mọi thứ ở một chỗ

- **Một** MonoBehaviour `GameBootstrap` trong scene Game tạo các đối tượng C# (session, booster, catalog, store) và trao cho Presenter qua constructor.
- Không Singleton, không `static` giữ trạng thái, không `FindObjectOfType`, không `GameObject.Find`. Muốn biết ai phụ thuộc ai: đọc `GameBootstrap`.
- Không cần thư viện DI. Nếu `GameBootstrap` vượt ~150 dòng thì tách thành vài `Installer` nhỏ theo mảng.
- MonoBehaviour chỉ dành cho: View, Input, Feedback, Bootstrap. Không có "XxxManager : MonoBehaviour" chứa logic.

---

## 6. Presentation: View câm + Presenter

- **View** (`MonoBehaviour`, trên prefab): giữ tham chiếu con bằng `[SerializeField]`, phơi hàm hiển thị (`SetLevelTitle`, `ShowHintCount`) và sự kiện thô (`HintClicked`). **Không có `if` nghiệp vụ.**
- **Presenter** (C# thuần): nghe session/booster, quyết định hiển thị gì, gọi View; nghe sự kiện View, gọi Application. Test được không cần scene.
- Presenter đăng ký sự kiện trong constructor/`Enable` và **luôn hủy đăng ký** trong `Dispose`/`Disable`.
- **Thứ cố định** (HUD, popup, khung): prefab đặt sẵn trong scene, chỉnh tay được.
- **Thứ theo dữ liệu** (ô, cạnh, ký hiệu của level): sinh từ prefab qua pool, vị trí tính từ `BoardLayoutConfig`. Không `new GameObject()` rồi gắn component bằng code.
- Hiệu ứng và âm thanh nằm ở `Feedback/`, nghe sự kiện `Changed` và tự quyết hiệu ứng nào. Logic không gọi hiệu ứng trực tiếp.

---

## 7. Cấu hình: chia nhỏ theo mảng

Không có một file cấu hình ôm hết. Mỗi mảng một ScriptableObject, nằm ở `Config/`:

| Asset | Chứa |
|---|---|
| `BoardLayoutConfig` | cỡ ô, khoảng cách, viền, lề màn hình |
| `InputConfig` | ngưỡng kéo, thời gian giữ, rung |
| `ClueVisuals` | luật/ký hiệu → sprite, màu |
| `PaletteConfig` | bảng màu tô vùng, màu lỗi |
| `FeedbackConfig` | thời lượng hiệu ứng, âm thanh theo sự kiện |
| `BoosterConfig` | danh sách booster, số lượng ban đầu |
| `LevelCatalog` | thứ tự pack và level |

Mỗi trường có `[Tooltip]`. Thêm mảng mới → thêm asset mới, không nhồi vào asset cũ.

---

## 8. Level

- Mỗi level là một `LevelAsset` gồm: bàn, luật + tham số, ký hiệu, **lời giải bắt buộc**, độ khó.
- Test tự động duyệt mọi level: **lời giải phải thoả mọi luật** và (khi có solver) **lời giải là duy nhất**. Level hỏng thì test đỏ, không đợi người chơi phát hiện.
- Làm level bằng Level Editor (`Tools/Puzzle/Level Editor`) hoặc nhập từ văn bản ASCII (định dạng ghi ở `Docs/design/level-format.md`).
- Không dùng level của game khác.

---

## 9. Quy ước code

- Theo quy ước C# chuẩn của .NET: `PascalCase` cho kiểu, hàm, thuộc tính, sự kiện; `_camelCase` cho field private; `camelCase` cho biến cục bộ và tham số; `I` cho interface. Ngoặc nhọn xuống dòng, thụt 4 dấu cách.
- Mỗi file một kiểu chính, tên file = tên kiểu. Namespace = assembly + thư mục (`Puzzle.Domain.Rules`).
- Ưu tiên bất biến: `readonly struct`, field `readonly`, trả về `IReadOnlyList`.
- **Comment giải thích vì sao, không lặp lại code.** `///` cho API public. Được comment trong thân hàm khi thuật toán không hiển nhiên (ví dụ union-find, cách chuẩn hoá hình). Ghi chú dài về thiết kế để ở `Docs/`.
- Tên nói rõ ý: `TryMergeRegions`, không `Handle`, `Process`, `DoStuff`.
- Không cấp phát trong vòng lặp nóng: truyền `List` vào để điền (`Check(ctx, violations)`), cache biến thể hình.
- Bất đồng bộ dùng `async`/`await` hoặc coroutine nhưng thống nhất một kiểu cho cả dự án (ghi trong `Docs/decisions.md`).
- Code cheat/debug bọc `#if UNITY_EDITOR || DEVELOPMENT_BUILD`.
- Không `Resources.Load`, không đường dẫn asset bằng chuỗi.

---

## 10. Test

- Mọi luật, command, booster, solver có test EditMode trong `Puzzle.Tests`.
- Test chạy trước mỗi commit lớn. Test đỏ thì không commit.
- Bug tìm thấy → viết test tái hiện trước, sửa sau.

---

## 11. Chống phình to

| Dấu hiệu | Xử lý |
|---|---|
| File > 300 dòng | Tách theo trách nhiệm |
| Hàm > 40 dòng | Tách hàm |
| Lớp có chữ `Manager`, `Helper`, `Utils` | Đặt lại tên theo việc nó làm; nếu không đặt được → lớp đang ôm nhiều việc |
| Hai luật/booster giống nhau > 10 dòng | Đưa phần chung vào Domain |
| Thêm package/plugin | Ghi lý do vào `Docs/decisions.md` trước |
| Asset không dùng | Xoá (git giữ lịch sử). Không giữ thư mục archive |

**Phạm vi prototype (đóng băng tới khi qua playtest):** 2 scene, 3–5 luật, ~30 level, 2 booster (Gợi ý, Kiểm tra), không shop, không meta. Ý tưởng mới ghi vào `Docs/backlog.md`, **không code**.

---

## 12. MUỐN ĐỔI X → VÀO ĐÂU

| Muốn đổi… | Vào đâu |
|---|---|
| Thêm / sửa luật | `Code/Domain/Rules/<TenLuat>/` + `RuleRegistry` + test |
| Thao tác người chơi (gộp, đặt cạnh, xoá) | `Code/Application/Commands/` + `Code/Presentation/Input/` |
| Booster | `Code/Application/Boosters/` + `Config/BoosterConfig` |
| Ký hiệu luật trên bàn | `Config/ClueVisuals`, sprite ở `Art/Clues/` |
| Kích thước ô, viền, bố cục bàn | `Config/BoardLayoutConfig` |
| Màu tô vùng, màu lỗi | `Config/PaletteConfig` |
| Hiệu ứng, âm thanh khi gộp / giải xong | `Code/Presentation/Feedback/` + `Config/FeedbackConfig` |
| Màn hình / popup | `Prefabs/Screens/` + `XxxView` + `XxxPresenter` |
| Level, thứ tự pack | `Levels/<pack>/`, `Config/LevelCatalog` |
| Lưu tiến độ | `Code/Application/Progress/` |
| Ai nối với ai | `Code/Bootstrap/GameBootstrap.cs` |

Có loại thay đổi mới chưa có trong bảng → thêm dòng.

---

## 13. Tài liệu & git

```
Docs/
├── research/       phân tích game tham khảo
├── design/         rules.md, level-format.md, boosters.md
├── decisions.md    quyết định kiến trúc: ngày · chọn gì · vì sao · bỏ phương án nào
└── backlog.md      ý tưởng để sau
```

- Đổi kiến trúc → sửa file này trong **cùng commit**, ghi lý do vào `Docs/decisions.md`.
- Commit nhỏ, mỗi commit một chủ đề, test xanh. Không commit asset của game khác.
