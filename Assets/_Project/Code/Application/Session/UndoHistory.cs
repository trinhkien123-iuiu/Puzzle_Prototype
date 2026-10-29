using System.Collections.Generic;

namespace Puzzle.Application.Session
{
    /// <summary>
    /// Hai ngăn xếp undo/redo. Chỉ giữ command, không tự gọi Do/Undo:
    /// việc áp lên state là của <see cref="PuzzleSession"/>.
    /// </summary>
    public sealed class UndoHistory
    {
        private readonly Stack<ICommand> _undo = new Stack<ICommand>();
        private readonly Stack<ICommand> _redo = new Stack<ICommand>();

        public bool CanUndo => _undo.Count > 0;
        public bool CanRedo => _redo.Count > 0;

        /// <summary>Ghi command vừa chạy. Thao tác mới làm mất nhánh redo, giống mọi trình soạn thảo.</summary>
        public void Record(ICommand command)
        {
            _undo.Push(command);
            _redo.Clear();
        }

        public bool TryTakeUndo(out ICommand command)
        {
            if (!_undo.TryPop(out command)) return false;
            _redo.Push(command);
            return true;
        }

        public bool TryTakeRedo(out ICommand command)
        {
            if (!_redo.TryPop(out command)) return false;
            _undo.Push(command);
            return true;
        }

        public void Clear()
        {
            _undo.Clear();
            _redo.Clear();
        }
    }
}
