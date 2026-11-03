using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Regions;

namespace Puzzle.Application.Commands
{
    /// <summary>
    /// Nhấn giữ một ô: xoá vùng chứa nó, tức tách mọi ô của vùng (cạnh về Unknown) và bỏ màu.
    /// Thay cho chuột phải của bản PC. Cạnh cho sẵn của đề được giữ nguyên.
    /// </summary>
    public sealed class ClearRegionCommand : ICommand
    {
        private readonly Cell _cell;
        private readonly StateChangeSet _changes;

        public ClearRegionCommand(PuzzleDefinition definition, Cell cell)
        {
            _cell = cell;
            _changes = new StateChangeSet(definition);
        }

        // Vùng tính lại lúc Do (kể cả khi redo): redo luôn chạy trên đúng state lúc đầu nên ra cùng vùng.
        public void Do(PuzzleState state)
        {
            _changes.Begin();
            var regions = new RegionMap(state);
            Region region = regions.RegionOf(_cell);
            if (region == null) return;

            foreach (Edge edge in state.Board.Edges)
            {
                if (regions.RegionIdOf(edge.A) == region.Id || regions.RegionIdOf(edge.B) == region.Id)
                {
                    _changes.SetEdge(state, edge, EdgeState.Unknown);
                }
            }
            foreach (Cell cell in region.Cells) _changes.SetPaint(state, cell, PuzzleState.NoPaint);
        }

        public void Undo(PuzzleState state) => _changes.Revert(state);
    }
}
