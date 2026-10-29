using System;
using System.Collections.Generic;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Regions;
using Puzzle.Domain.Rules;

namespace Puzzle.Application.Session
{
    /// <summary>
    /// Cửa duy nhất để thay đổi một ván: chạy command, giữ undo/redo, tính lại vùng và lỗi,
    /// rồi phát một sự kiện <see cref="Changed"/>. Không tự kiểm tra luật nào.
    /// </summary>
    public sealed class PuzzleSession : IPuzzleReader, IPuzzleCommands
    {
        private readonly PuzzleState _state;
        private readonly PuzzleValidator _validator;
        private readonly UndoHistory _history = new UndoHistory();

        public PuzzleDefinition Definition { get; }
        public SessionSnapshot Snapshot { get; private set; }

        public event Action<SessionSnapshot> Changed;

        /// <param name="startState">Trạng thái khôi phục từ bản lưu; null thì bắt đầu ván mới.</param>
        public PuzzleSession(PuzzleDefinition definition, PuzzleState startState = null)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            if (startState != null && startState.Board != definition.Board)
            {
                throw new ArgumentException("Start state belongs to a different board.", nameof(startState));
            }

            _state = startState?.Clone() ?? definition.CreateInitialState();
            _validator = new PuzzleValidator(definition.Rules);
            Snapshot = BuildSnapshot();
        }

        public bool Execute(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            // So với bản sao để bỏ qua command không đổi gì (ví dụ chạm vào cạnh cho sẵn),
            // tránh một bước undo "rỗng" mà người chơi bấm không thấy gì xảy ra.
            PuzzleState before = _state.Clone();
            command.Do(_state);
            if (_state.SameEdgesAs(before)) return false;

            _history.Record(command);
            Publish();
            return true;
        }

        public bool Undo()
        {
            if (!_history.TryTakeUndo(out ICommand command)) return false;
            command.Undo(_state);
            Publish();
            return true;
        }

        public bool Redo()
        {
            if (!_history.TryTakeRedo(out ICommand command)) return false;
            command.Do(_state);
            Publish();
            return true;
        }

        private void Publish()
        {
            Snapshot = BuildSnapshot();
            Changed?.Invoke(Snapshot);
        }

        // Chỉ chạy sau mỗi thao tác, không chạy mỗi frame (xem ARCHITECTURE.md mục 14 - Hiệu năng).
        private SessionSnapshot BuildSnapshot()
        {
            var regions = new RegionMap(_state);
            var violations = new List<RuleViolation>();
            _validator.Validate(new RuleContext(_state, regions), violations);
            return new SessionSnapshot(_state.Clone(), regions, violations, _history.CanUndo, _history.CanRedo);
        }
    }
}
