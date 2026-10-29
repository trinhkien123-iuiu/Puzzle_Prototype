using System;
using System.Collections.Generic;
using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Application.Commands
{
    /// <summary>
    /// Một lần kéo ngón qua các ô: mỗi cặp ô liên tiếp trên đường kéo được nối (Joined).
    /// Cả lần kéo là một command nên một lần Undo hoàn tác cả nét kéo.
    /// </summary>
    public sealed class MergeCellsCommand : ICommand
    {
        private readonly Cell[] _path;
        private readonly EdgeChangeSet _changes;

        /// <param name="path">Các ô theo thứ tự ngón đi qua. Cặp không kề nhau (ngón lướt quá nhanh) bị bỏ qua;
        /// Input nên tự nội suy để đường kéo liền mạch.</param>
        public MergeCellsCommand(PuzzleDefinition definition, IEnumerable<Cell> path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            _path = new List<Cell>(path).ToArray();
            _changes = new EdgeChangeSet(definition);
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
                if (board.Contains(edge)) _changes.Set(state, edge, EdgeState.Joined);
            }
        }

        public void Undo(PuzzleState state) => _changes.Revert(state);
    }
}
