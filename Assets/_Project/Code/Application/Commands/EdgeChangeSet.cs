using System;
using System.Collections.Generic;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Application.Commands
{
    /// <summary>
    /// Phần chung của mọi command đổi cạnh: đặt trạng thái mới, nhớ trạng thái cũ để hoàn tác,
    /// và bỏ qua cạnh cho sẵn của đề. Gom ở đây để command nào cũng tuân hợp đồng undo giống nhau.
    /// </summary>
    internal sealed class EdgeChangeSet
    {
        private readonly PuzzleDefinition _definition;
        private readonly List<(Edge Edge, EdgeState Before)> _changes = new List<(Edge, EdgeState)>();

        public EdgeChangeSet(PuzzleDefinition definition)
        {
            _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <summary>Gọi ở đầu mỗi Do: redo tính lại thay đổi từ state hiện tại.</summary>
        public void Begin() => _changes.Clear();

        public void Set(PuzzleState state, Edge edge, EdgeState target)
        {
            if (_definition.IsGivenBorder(edge)) return;
            EdgeState before = state.GetEdge(edge);
            if (before == target) return;
            _changes.Add((edge, before));
            state.SetEdge(edge, target);
        }

        // Hoàn tác theo thứ tự ngược để cạnh bị đổi hai lần trong cùng command vẫn về đúng giá trị đầu.
        public void Revert(PuzzleState state)
        {
            for (int i = _changes.Count - 1; i >= 0; i--)
            {
                state.SetEdge(_changes[i].Edge, _changes[i].Before);
            }
        }
    }
}
