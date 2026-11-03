using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Application.Commands
{
    /// <summary>
    /// Nút Restart: đưa bàn về như lúc bắt đầu (mọi cạnh người chơi đặt về Unknown, bỏ hết màu).
    /// Là command nên lỡ bấm vẫn Undo được.
    /// </summary>
    public sealed class ResetBoardCommand : ICommand
    {
        private readonly StateChangeSet _changes;

        public ResetBoardCommand(PuzzleDefinition definition)
        {
            _changes = new StateChangeSet(definition);
        }

        public void Do(PuzzleState state)
        {
            _changes.Begin();
            foreach (Edge edge in state.Board.Edges) _changes.SetEdge(state, edge, EdgeState.Unknown);
            foreach (Cell cell in state.Board.Cells) _changes.SetPaint(state, cell, PuzzleState.NoPaint);
        }

        public void Undo(PuzzleState state) => _changes.Revert(state);
    }
}
