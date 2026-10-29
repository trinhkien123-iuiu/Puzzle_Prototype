using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Application.Commands
{
    /// <summary>
    /// Đặt một cạnh về trạng thái cho trước. Chạm cạnh để đặt/xoá ranh giới (Input chọn Border hoặc Unknown),
    /// và booster Gợi ý cũng dùng command này để đưa một cạnh về đúng lời giải.
    /// </summary>
    public sealed class SetEdgeCommand : ICommand
    {
        private readonly Edge _edge;
        private readonly EdgeState _target;
        private readonly EdgeChangeSet _changes;

        public Edge Edge => _edge;
        public EdgeState Target => _target;

        public SetEdgeCommand(PuzzleDefinition definition, Edge edge, EdgeState target)
        {
            _edge = edge;
            _target = target;
            _changes = new EdgeChangeSet(definition);
        }

        /// <summary>Lệnh cho một lần chạm cạnh: đang là ranh giới thì xoá, chưa thì đặt.</summary>
        public static SetEdgeCommand ToggleBorder(PuzzleDefinition definition, PuzzleState current, Edge edge)
        {
            EdgeState target = current.GetEdge(edge) == EdgeState.Border ? EdgeState.Unknown : EdgeState.Border;
            return new SetEdgeCommand(definition, edge, target);
        }

        public void Do(PuzzleState state)
        {
            _changes.Begin();
            _changes.Set(state, _edge, _target);
        }

        public void Undo(PuzzleState state) => _changes.Revert(state);
    }
}
