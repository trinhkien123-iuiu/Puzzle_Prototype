using System;
using System.Collections.Generic;
using Puzzle.Application.Commands;
using Puzzle.Application.Session;
using Puzzle.Domain.Grid;
using Puzzle.Presentation.Board;

namespace Puzzle.Presentation.Input
{
    /// <summary>
    /// Đổi ý định của ngón tay thành command và đưa vào phiên chơi (bảng cử chỉ ở ARCHITECTURE.md mục 14).
    /// </summary>
    public sealed class BoardActionPresenter : IDisposable
    {
        private readonly BoardInput _input;
        private readonly IPuzzleReader _reader;
        private readonly IPuzzleCommands _commands;
        private readonly PaintSelection _paint;

        public BoardActionPresenter(BoardInput input, IPuzzleReader reader, IPuzzleCommands commands,
            PaintSelection paint)
        {
            _paint = paint ?? throw new ArgumentNullException(nameof(paint));
            _input = input;
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));

            _input.CellsDragged += OnCellsDragged;
            _input.EdgeTapped += OnEdgeTapped;
            _input.CellHeld += OnCellHeld;
        }

        public void Dispose()
        {
            _input.CellsDragged -= OnCellsDragged;
            _input.EdgeTapped -= OnEdgeTapped;
            _input.CellHeld -= OnCellHeld;
        }

        private void OnCellsDragged(IReadOnlyList<Cell> path)
        {
            if (_reader.Snapshot.IsSolved) return;
            _commands.Execute(new MergeCellsCommand(_reader.Definition, path, _paint.Selected));
        }

        private void OnEdgeTapped(Edge edge)
        {
            if (_reader.Snapshot.IsSolved) return;
            _commands.Execute(SetEdgeCommand.ToggleBorder(_reader.Definition, _reader.Snapshot.State, edge));
        }

        private void OnCellHeld(Cell cell)
        {
            if (_reader.Snapshot.IsSolved) return;
            _commands.Execute(new ClearRegionCommand(_reader.Definition, cell));
        }
    }
}
