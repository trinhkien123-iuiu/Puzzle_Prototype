using System;
using System.Collections.Generic;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Application.Commands
{
    /// <summary>
    /// Phần chung của mọi command: đổi cạnh/màu ô, nhớ giá trị cũ để hoàn tác, và bỏ qua cạnh cho sẵn của đề.
    /// Gom ở đây để command nào cũng tuân hợp đồng undo giống nhau.
    /// </summary>
    internal sealed class StateChangeSet
    {
        private readonly PuzzleDefinition _definition;
        private readonly List<(Edge Edge, EdgeState Before)> _edges = new List<(Edge, EdgeState)>();
        private readonly List<(Cell Cell, int Before)> _paint = new List<(Cell, int)>();

        public StateChangeSet(PuzzleDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>Gọi ở đầu mỗi Do: redo tính lại thay đổi từ state hiện tại.</summary>
        public void Begin()
        {
            _edges.Clear();
            _paint.Clear();
        }

        public void SetEdge(PuzzleState state, Edge edge, EdgeState target)
        {
            if (_definition.IsGivenBorder(edge)) return;
            EdgeState before = state.GetEdge(edge);
            if (before == target) return;
            _edges.Add((edge, before));
            state.SetEdge(edge, target);
        }

        public void SetPaint(PuzzleState state, Cell cell, int paint)
        {
            int before = state.GetPaint(cell);
            if (before == paint) return;
            _paint.Add((cell, before));
            state.SetPaint(cell, paint);
        }

        // Hoàn tác theo thứ tự ngược để giá trị bị đổi hai lần trong cùng command vẫn về đúng giá trị đầu.
        public void Revert(PuzzleState state)
        {
            for (int i = _paint.Count - 1; i >= 0; i--) state.SetPaint(_paint[i].Cell, _paint[i].Before);
            for (int i = _edges.Count - 1; i >= 0; i--) state.SetEdge(_edges[i].Edge, _edges[i].Before);
        }
    }
}
