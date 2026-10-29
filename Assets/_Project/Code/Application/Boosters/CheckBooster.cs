using System.Collections.Generic;
using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Regions;
using Puzzle.Domain.Solving;

namespace Puzzle.Application.Boosters
{
    /// <summary>
    /// Kiểm tra: làm nổi bật mọi vùng đang chứa một cạnh đặt ngược với lời giải. Không đổi state.
    /// So với lời giải thay vì chỉ dùng lỗi luật, vì một vùng có thể thoả luật lúc này mà vẫn dẫn tới ngõ cụt.
    /// Trả danh sách rỗng nghĩa là "chưa có gì sai".
    /// </summary>
    public sealed class CheckBooster : IBooster
    {
        public bool CanUse(IPuzzleReader puzzle) => !puzzle.Snapshot.IsSolved;

        public BoosterResult Use(IPuzzleReader puzzle)
        {
            SessionSnapshot snapshot = puzzle.Snapshot;
            var mistakes = new List<Edge>();
            new MistakeFinder(puzzle.Definition).Collect(snapshot.State, mistakes);

            var wrongRegions = new HashSet<int>();
            foreach (Edge edge in mistakes)
            {
                wrongRegions.Add(snapshot.Regions.RegionIdOf(edge.A));
                wrongRegions.Add(snapshot.Regions.RegionIdOf(edge.B));
            }

            return new BoosterResult(null, CellsOf(snapshot.Regions, wrongRegions));
        }

        private static List<Cell> CellsOf(RegionMap regions, HashSet<int> regionIds)
        {
            var cells = new List<Cell>();
            foreach (Region region in regions.Regions)
            {
                if (regionIds.Contains(region.Id)) cells.AddRange(region.Cells);
            }
            return cells;
        }
    }
}
