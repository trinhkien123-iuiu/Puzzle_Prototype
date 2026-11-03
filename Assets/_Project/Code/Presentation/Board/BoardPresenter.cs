using System;
using System.Collections.Generic;
using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Regions;
using Puzzle.Domain.Rules;
using Puzzle.Presentation.Config;
using UnityEngine;

namespace Puzzle.Presentation.Board
{
    /// <summary>
    /// Nối BoardView với phiên chơi: dựng bố cục, quyết định màu từng ô/cạnh từ snapshot.
    /// Mọi quyết định "vẽ thế nào" nằm ở đây để BoardView không có if nghiệp vụ.
    /// </summary>
    public sealed class BoardPresenter : IDisposable
    {
        private const float DotThicknessRatio = 0.3f;
        private const float GlassCellsPerTile = 3f;

        private readonly IPuzzleReader _puzzle;
        private readonly BoardView _view;
        private readonly BoardLayoutConfig _layoutConfig;
        private readonly PaletteConfig _palette;
        private readonly HashSet<Cell> _highlight = new HashSet<Cell>();
        private readonly HashSet<Cell> _preview = new HashSet<Cell>();
        private readonly HashSet<int> _brokenRegions = new HashSet<int>();
        private Color[] _regionColors = Array.Empty<Color>();
        private Sprite[] _regionSprites = Array.Empty<Sprite>();

        // Bảng màu có sprite khối (kiểu Block Blast): ô vẽ bằng sprite, các ô trong vùng đứng tách nhau như khối.
        private bool UsesSprites => _palette.BoardUsesSprites;

        public BoardPresenter(IPuzzleReader puzzle, BoardView view, BoardLayoutConfig layoutConfig, PaletteConfig palette)
        {
            _puzzle = puzzle ?? throw new ArgumentNullException(nameof(puzzle));
            _view = view;
            _layoutConfig = layoutConfig;
            _palette = palette;

            _puzzle.Changed += OnChanged;
            _view.Resized += Rebuild;
            Rebuild();
        }

        /// <summary>Làm nổi bật các ô (kết quả booster) tới lần thay đổi kế tiếp.</summary>
        public void ShowHighlight(IReadOnlyList<Cell> cells)
        {
            _highlight.Clear();
            foreach (Cell cell in cells) _highlight.Add(cell);
            Render(_puzzle.Snapshot);
        }

        /// <summary>Tô đường đang kéo; danh sách rỗng để xoá.</summary>
        public void ShowDragPreview(IReadOnlyList<Cell> cells)
        {
            _preview.Clear();
            foreach (Cell cell in cells) _preview.Add(cell);
            Render(_puzzle.Snapshot);
        }

        public void Dispose()
        {
            _puzzle.Changed -= OnChanged;
            _view.Resized -= Rebuild;
        }

        private void OnChanged(SessionSnapshot snapshot)
        {
            _highlight.Clear();
            Render(snapshot);
        }

        private void Rebuild()
        {
            BoardShape board = _puzzle.Definition.Board;
            var layout = new BoardLayout(board, _view.AvailableSize, _layoutConfig.MaxCellSize);
            _view.Build(layout, board.Cells.Count, board.Edges.Count);
            WarnIfCellsTooSmall(layout);
            Render(_puzzle.Snapshot);
        }

        private void Render(SessionSnapshot snapshot)
        {
            BoardLayout layout = _view.Layout;
            if (layout == null) return;
            ComputeRegionColors(snapshot);

            BoardShape board = _puzzle.Definition.Board;
            float outline = _layoutConfig.BorderThickness / 2f;
            for (int i = 0; i < board.Cells.Count; i++)
            {
                Cell cell = board.Cells[i];
                Rect rect = layout.CellRect(cell, _layoutConfig.CellGap);
                EdgeMasks(snapshot, board, cell, out int sides, out int corners);
                Sprite sprite = CellSprite(snapshot, cell);
                _view.ShowCell(i, rect, CellTint(snapshot, cell, sprite), sprite, GlassUv(cell), sides, corners);
                var padded = new Rect(rect.x - outline, rect.y - outline, rect.width + outline * 2f, rect.height + outline * 2f);
                _view.ShowCellOutline(i, padded, _palette.Border);
            }
            for (int i = 0; i < board.Edges.Count; i++)
            {
                RenderEdge(snapshot, layout, i, board.Edges[i]);
            }
        }

        private void RenderEdge(SessionSnapshot snapshot, BoardLayout layout, int index, Edge edge)
        {
            EdgeState state = snapshot.State.GetEdge(index);
            float gap = _layoutConfig.CellGap;
            float thickness = _layoutConfig.BorderThickness;
            RegionMap regions = snapshot.Regions;
            int regionA = regions.RegionIdOf(edge.A);
            int regionB = regions.RegionIdOf(edge.B);

            // Như game gốc: mỗi vùng đã gộp/đã tô có viền đậm bao quanh, kể cả khi người chơi không tự đặt ranh giới.
            if (state == EdgeState.Border ||
                (regionA != regionB && (IsGrouped(snapshot, regionA) || IsGrouped(snapshot, regionB))))
            {
                Color color = _puzzle.Definition.IsGivenBorder(edge) ? _palette.GivenBorder : _palette.Border;
                _view.ShowEdge(index, layout.EdgeRect(edge, thickness, thickness), edge.IsVertical, EdgeLook.Solid, color);
            }
            else if (regionA == regionB)
            {
                // Cùng vùng: kính liền, không nét. Còn khe giữa ô thì lấp bằng màu vùng.
                if (gap > 0f && !UsesSprites)
                {
                    _view.ShowEdge(index, layout.EdgeRect(edge, gap, -gap), edge.IsVertical, EdgeLook.Solid, CellColor(snapshot, edge.A));
                }
                else
                {
                    _view.ShowEdge(index, layout.EdgeRect(edge, 0f, 0f), edge.IsVertical, EdgeLook.Hidden, Color.clear);
                }
            }
            else
            {
                Rect rect = layout.EdgeRect(edge, thickness * DotThicknessRatio, -thickness);
                _view.ShowEdge(index, rect, edge.IsVertical, EdgeLook.Dotted, _palette.Dots);
            }
        }

        private static bool IsGrouped(SessionSnapshot snapshot, int regionId)
        {
            Region region = snapshot.Regions.Regions[regionId];
            return region.Area > 1 || snapshot.State.GetPaint(region.Cells[0]) != PuzzleState.NoPaint;
        }

        // Mặt nạ cho dải sáng dọc mép vùng (như atlas T_EdgeGradients/T_VertexGradients của game gốc).
        // Vùng đã gộp/đã tô: mép là mép vùng; ô lẻ chưa tô: mép là bốn cạnh của chính ô. Lưới Y hướng xuống, mặt nạ theo hướng màn hình.
        private static void EdgeMasks(SessionSnapshot snapshot, BoardShape board, Cell cell, out int sides, out int corners)
        {
            sides = corners = 0;
            int id = snapshot.Regions.RegionIdOf(cell);
            if (!IsGrouped(snapshot, id))
            {
                sides = 15; // ô lẻ chưa gộp: tự là một viên riêng (vát đủ bốn phía); kính trắng thì không lộ gì
                return;
            }

            bool Same(int dx, int dy)
            {
                Cell other = cell.Offset(dx, dy);
                return board.Contains(other) && snapshot.Regions.RegionIdOf(other) == id;
            }

            bool left = Same(-1, 0), right = Same(1, 0), up = Same(0, -1), down = Same(0, 1);
            if (!left) sides |= 1;
            if (!down) sides |= 2;
            if (!right) sides |= 4;
            if (!up) sides |= 8;
            // Góc lõm: hai cạnh kề cùng vùng nhưng ô chéo khác vùng.
            if (down && right && !Same(1, 1)) corners |= 1;
            if (down && left && !Same(-1, 1)) corners |= 2;
            if (up && right && !Same(1, -1)) corners |= 4;
            if (up && left && !Same(-1, -1)) corners |= 8;
        }

        // Hoạ tiết kính lặp sau mỗi GlassCellsPerTile ô; ô lấy phần ứng với vị trí của nó trên lưới.
        private static Rect GlassUv(Cell cell)
        {
            const float Step = 1f / GlassCellsPerTile;
            return new Rect(cell.X * Step, -(cell.Y + 1) * Step, Step, Step);
        }

        private Sprite CellSprite(SessionSnapshot snapshot, Cell cell)
        {
            if (!UsesSprites) return null;
            if (_preview.Contains(cell) && _palette.PreviewSprite != null) return _palette.PreviewSprite;
            return _regionSprites[snapshot.Regions.RegionIdOf(cell)];
        }

        // Ô vẽ bằng sprite thì giữ màu gốc của sprite (trắng), chỉ nhuộm khi làm nổi bật hoặc vùng sai.
        private Color CellTint(SessionSnapshot snapshot, Cell cell, Sprite sprite)
        {
            if (sprite == null) return CellColor(snapshot, cell);
            if (_highlight.Contains(cell)) return _palette.Highlight;
            if (_preview.Contains(cell)) return Color.white;
            return _brokenRegions.Contains(snapshot.Regions.RegionIdOf(cell)) ? _palette.Broken(Color.white) : Color.white;
        }

        private Color CellColor(SessionSnapshot snapshot, Cell cell)
        {
            if (_preview.Contains(cell)) return _palette.DragPreview;
            if (_highlight.Contains(cell)) return _palette.Highlight;
            return _regionColors[snapshot.Regions.RegionIdOf(cell)];
        }

        // Vùng lấy màu người chơi đã tô; vùng gộp bằng Gợi ý chưa có màu thì tự chọn theo ô đầu tiên.
        // Vùng sai chắc chắn pha thêm đỏ; vùng còn dở giữ nguyên màu để người chơi không bị báo lỗi khi đang gộp.
        private void ComputeRegionColors(SessionSnapshot snapshot)
        {
            RegionMap regions = snapshot.Regions;
            if (_regionColors.Length != regions.Count) _regionColors = new Color[regions.Count];
            if (_regionSprites.Length != regions.Count) _regionSprites = new Sprite[regions.Count];

            _brokenRegions.Clear();
            foreach (RuleViolation violation in snapshot.Violations)
            {
                if (violation.RegionId >= 0 && violation.Severity == Severity.Broken) _brokenRegions.Add(violation.RegionId);
            }

            foreach (Region region in regions.Regions)
            {
                Cell anchor = region.Cells[0];
                int paint = snapshot.State.GetPaint(anchor);
                Color color = paint != PuzzleState.NoPaint ? _palette.PaintColor(paint)
                    : region.Area > 1 ? _palette.RegionColor(anchor.X * 7 + anchor.Y * 3)
                    : _palette.UnpaintedCell;
                _regionColors[region.Id] = _brokenRegions.Contains(region.Id) ? _palette.Broken(color) : color;
                _regionSprites[region.Id] = paint != PuzzleState.NoPaint ? _palette.PaintSprite(paint)
                    : region.Area > 1 ? _palette.RegionSprite(anchor.X * 7 + anchor.Y * 3)
                    : _palette.EmptySprite;
            }
        }

        private void WarnIfCellsTooSmall(BoardLayout layout)
        {
            float dpi = Screen.dpi > 0f ? Screen.dpi : 160f;
            float cellDp = layout.CellSize * _view.CanvasScale * 160f / dpi;
            if (cellDp < _layoutConfig.MinCellDp)
            {
                Debug.LogWarning($"Board cells are {cellDp:0}dp, below the {_layoutConfig.MinCellDp}dp touch target.");
            }
        }
    }
}
