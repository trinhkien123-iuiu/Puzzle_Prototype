using System;
using System.Collections.Generic;
using Puzzle.Application.Boosters;
using Puzzle.Application.Commands;
using Puzzle.Application.Session;
using Puzzle.Domain.Grid;

namespace Puzzle.Presentation.Screens
{
    /// <summary>
    /// Nối HUD với phiên chơi và hai booster. Booster trả command thì đưa vào session (undo được),
    /// trả ô cần làm nổi bật thì báo ra ngoài qua <see cref="HighlightRequested"/> để bàn tô.
    /// Băng-rôn mặc định hiện câu hướng dẫn của level; thông báo tạm thời (lỗi, booster) thay chỗ nó.
    /// </summary>
    public sealed class HudPresenter : IDisposable
    {
        private const string DefaultTip = "Kéo ngón qua các ô để gộp thành vùng. Mỗi vùng phải là một hình trong kho.";

        private readonly HudView _view;
        private readonly IPuzzleReader _reader;
        private readonly IPuzzleCommands _commands;
        private readonly IBooster _hint;
        private readonly IBooster _check;
        private readonly string _tip;

        public event Action<IReadOnlyList<Cell>> HighlightRequested;

        public HudPresenter(HudView view, IPuzzleReader reader, IPuzzleCommands commands,
            IBooster hint, IBooster check, HudInfo info)
        {
            _view = view;
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _hint = hint;
            _check = check;
            _tip = string.IsNullOrEmpty(info.Tip) ? DefaultTip : info.Tip;

            _view.UndoClicked += OnUndo;
            _view.RestartClicked += OnRestart;
            _view.HintClicked += OnHint;
            _view.CheckClicked += OnCheck;
            _reader.Changed += OnChanged;

            _view.SetTitle(info.Title);
            _view.SetDifficulty($"Độ khó {info.Difficulty}");
            _view.SetMessage(_tip);
            RefreshButtons();
        }

        public void Dispose()
        {
            _view.UndoClicked -= OnUndo;
            _view.RestartClicked -= OnRestart;
            _view.HintClicked -= OnHint;
            _view.CheckClicked -= OnCheck;
            _reader.Changed -= OnChanged;
        }

        private void OnUndo() => _commands.Undo();

        private void OnRestart() => _commands.Execute(new ResetBoardCommand(_reader.Definition));

        private void OnHint()
        {
            if (!_hint.CanUse(_reader)) return;
            BoosterResult result = _hint.Use(_reader);
            if (result.Command != null) _commands.Execute(result.Command);
            HighlightRequested?.Invoke(result.HighlightedCells);
            _view.SetMessage("Gợi ý: cạnh giữa hai ô vàng đã được sửa.");
        }

        private void OnCheck()
        {
            if (!_check.CanUse(_reader)) return;
            BoosterResult result = _check.Use(_reader);
            HighlightRequested?.Invoke(result.HighlightedCells);
            _view.SetMessage(result.HighlightedCells.Count == 0
                ? "Chưa có gì sai, cứ tiếp tục!"
                : "Vùng tô vàng đang gộp sai hoặc có ranh giới đặt sai.");
        }

        private void OnChanged(SessionSnapshot snapshot)
        {
            RefreshButtons();
            if (snapshot.IsSolved) _view.SetMessage("Hoàn thành! Mọi vùng đều khớp kho hình.");
            else if (snapshot.HasBroken) _view.SetMessage("Vùng màu đỏ không thể thành hình nào trong kho. Giữ để xoá hoặc Hoàn tác.");
            else _view.SetMessage(_tip);
        }

        private void RefreshButtons()
        {
            SessionSnapshot snapshot = _reader.Snapshot;
            _view.SetButtons(snapshot.CanUndo, !snapshot.IsSolved, _hint.CanUse(_reader), _check.CanUse(_reader));
        }
    }
}
