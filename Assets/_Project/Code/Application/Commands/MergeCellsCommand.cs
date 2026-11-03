using System;
using System.Collections.Generic;
using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Regions;

namespace Puzzle.Application.Commands
{
    /// <summary>
    /// Một lần kéo ngón qua các ô: mỗi cặp ô liên tiếp trên đường kéo được gộp (Joined),
    /// rồi cả vùng vừa gộp được tô một màu. Cả lần kéo là một command nên một lần Undo hoàn tác cả nét kéo.
    /// </summary>
    public sealed class MergeCellsCommand : ICommand
    {
        private readonly Cell[] _path;
        private readonly int _paint;
        private readonly StateChangeSet _changes;

        /// <param name="path">Các ô theo thứ tự ngón đi qua. Cặp không kề nhau (ngón lướt quá nhanh) bị bỏ qua;
        /// Input nên tự nội suy để đường kéo liền mạch.</param>
        /// <param name="paint">Màu tô cho cả vùng sau khi gộp (chỉ số bảng màu, khác 0).</param>
        public MergeCellsCommand(PuzzleDefinition definition, IEnumerable<Cell> path, int paint)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            _path = new List<Cell>(path).ToArray();
            _paint = paint;
            _changes = new StateChangeSet(definition);
        }

        public void Do(PuzzleState state)
        {
            _changes.Begin();
            BoardShape board = state.Board;
            for (int i = 1; i < _path.Length; i++)
            {
                Cell from = _path[i - 1];
                Cell to = _path[i];
                if (!from.IsAdjacentTo(to)) continue;
                var edge = new Edge(from, to);
                if (board.Contains(edge)) _changes.SetEdge(state, edge, EdgeState.Joined);
            }
            PaintRegionsOnPath(state);
        }

        public void Undo(PuzzleState state) => _changes.Revert(state);

        // Tô lại cả vùng (không chỉ các ô trên đường kéo) để vùng gộp từ hai vùng màu khác nhau thành một màu.
        private void PaintRegionsOnPath(PuzzleState state)
        {
            var regions = new RegionMap(state);
            foreach (Cell start in _path)
            {
                Region region = regions.RegionOf(start);
                if (region == null) continue;
                foreach (Cell cell in region.Cells) _changes.SetPaint(state, cell, _paint);
            }
        }
    }
}
