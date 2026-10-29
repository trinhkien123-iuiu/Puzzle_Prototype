using System;
using System.Collections.Generic;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Domain.Regions
{
    /// <summary>
    /// Chia bàn thành các vùng từ trạng thái cạnh. Hai ô kề nhau cùng vùng khi cạnh giữa chúng
    /// không phải Border (Unknown cũng nối, vì người chơi chia bàn bằng cách vẽ ranh giới).
    /// Tính một lần, chỉ đọc; trạng thái đổi thì tạo RegionMap mới.
    /// </summary>
    public sealed class RegionMap
    {
        private readonly BoardShape _board;
        private readonly int[] _regionOfCell;
        private readonly Region[] _regions;

        public IReadOnlyList<Region> Regions => _regions;

        public int Count => _regions.Length;

        public RegionMap(PuzzleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            _board = state.Board;

            int[] parent = BuildUnionFind(state);
            _regionOfCell = AssignRegionIds(parent, out int regionCount);
            _regions = BuildRegions(state, regionCount);
        }

        /// <summary>Id vùng chứa ô; -1 nếu ô không thuộc bàn.</summary>
        public int RegionIdOf(Cell cell)
        {
            int index = _board.IndexOf(cell);
            return index < 0 ? -1 : _regionOfCell[index];
        }

        public Region RegionOf(Cell cell)
        {
            int id = RegionIdOf(cell);
            return id < 0 ? null : _regions[id];
        }

        // Union-find trên chỉ số ô: mỗi cạnh không phải Border gộp hai tập.
        // Bàn nhỏ (dưới vài trăm ô) nên nén đường đi là đủ, không cần hợp theo hạng.
        private int[] BuildUnionFind(PuzzleState state)
        {
            var parent = new int[_board.Cells.Count];
            for (int i = 0; i < parent.Length; i++) parent[i] = i;

            IReadOnlyList<Edge> edges = _board.Edges;
            for (int i = 0; i < edges.Count; i++)
            {
                if (state.GetEdge(i) == EdgeState.Border) continue;
                int rootA = Find(parent, _board.IndexOf(edges[i].A));
                int rootB = Find(parent, _board.IndexOf(edges[i].B));
                if (rootA != rootB) parent[rootB] = rootA;
            }
            return parent;
        }

        // Id vùng đánh theo thứ tự ô đầu tiên gặp (dòng rồi cột) để kết quả ổn định
        // giữa các lần tính: View có thể giữ màu theo id mà không nhấp nháy.
        private static int[] AssignRegionIds(int[] parent, out int regionCount)
        {
            var regionOfCell = new int[parent.Length];
            var idOfRoot = new Dictionary<int, int>();
            for (int i = 0; i < parent.Length; i++)
            {
                int root = Find(parent, i);
                if (!idOfRoot.TryGetValue(root, out int id))
                {
                    id = idOfRoot.Count;
                    idOfRoot[root] = id;
                }
                regionOfCell[i] = id;
            }
            regionCount = idOfRoot.Count;
            return regionOfCell;
        }

        private Region[] BuildRegions(PuzzleState state, int regionCount)
        {
            var cellsPerRegion = new List<Cell>[regionCount];
            var unknownInner = new int[regionCount];
            for (int r = 0; r < regionCount; r++) cellsPerRegion[r] = new List<Cell>();

            for (int i = 0; i < _regionOfCell.Length; i++)
            {
                cellsPerRegion[_regionOfCell[i]].Add(_board.Cells[i]);
            }

            // Cạnh Unknown mà hai bên cùng vùng = chỗ người chơi còn có thể cắt vùng ra.
            IReadOnlyList<Edge> edges = _board.Edges;
            for (int i = 0; i < edges.Count; i++)
            {
                if (state.GetEdge(i) != EdgeState.Unknown) continue;
                int region = _regionOfCell[_board.IndexOf(edges[i].A)];
                if (region == _regionOfCell[_board.IndexOf(edges[i].B)]) unknownInner[region]++;
            }

            var regions = new Region[regionCount];
            for (int r = 0; r < regionCount; r++)
            {
                regions[r] = new Region(r, cellsPerRegion[r].ToArray(), unknownInner[r]);
            }
            return regions;
        }

        private static int Find(int[] parent, int index)
        {
            while (parent[index] != index)
            {
                parent[index] = parent[parent[index]]; // nén nửa đường: trỏ thẳng lên ông
                index = parent[index];
            }
            return index;
        }
    }
}
