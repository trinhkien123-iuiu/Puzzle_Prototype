using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Regions;

namespace Puzzle.Application.Commands
{
    /// <summary>
    /// Nhấn giữ một ô: xoá vùng chứa nó, tức đưa mọi cạnh chạm vào vùng (bên trong và đường bao) về Unknown.
    /// Thay cho chuột phải của bản PC. Cạnh cho sẵn của đề được giữ nguyên.
    /// </summary>
    public sealed class ClearRegionCommand : ICommand
    {
        private readonly Cell _cell;
        private readonly EdgeChangeSet _changes;

        public ClearRegionCommand(PuzzleDefinition definition, Cell cell)
        {
            _cell = cell;
            _changes = new EdgeChangeSet(definition);
        }

        // Vùng tính lại lúc Do (kể cả khi redo): redo luôn chạy trên đúng state lúc đầu nên ra cùng vùng.
        public void Do(PuzzleState state)
        {
            _changes.Begin();
            var regions = new RegionMap(state);
            int regionId = regions.RegionIdOf(_cell);
            if (regionId < 0) return;

            foreach (Edge edge in state.Board.Edges)
            {
                if (regions.RegionIdOf(edge.A) == regionId || regions.RegionIdOf(edge.B) == regionId)
                {
                    _changes.Set(state, edge, EdgeState.Unknown);
                }
            }
        }

        public void Undo(PuzzleState state) => _changes.Revert(state);
    }
}
