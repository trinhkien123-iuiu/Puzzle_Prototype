using System;
using Puzzle.Application.Boosters;
using Puzzle.Application.Levels;
using Puzzle.Application.Session;
using Puzzle.Domain.Puzzle;
using Puzzle.Presentation.Board;
using Puzzle.Presentation.Feedback;
using Puzzle.Presentation.Input;
using Puzzle.Presentation.Screens;

namespace Puzzle.Bootstrap
{
    /// <summary>
    /// Mọi đối tượng sống trong một level: phiên chơi và các presenter nối nó với view.
    /// Đổi level = Dispose scope cũ, tạo scope mới; nhờ vậy không presenter nào còn nghe session cũ.
    /// </summary>
    internal sealed class LevelScope : IDisposable
    {
        private readonly BoardPresenter _board;
        private readonly BoardActionPresenter _actions;
        private readonly HudPresenter _hud;
        private readonly SolvedPopupPresenter _popup;
        private readonly FeedbackPresenter _feedback;
        private readonly BoardInput _input;

        public string LevelId { get; }
        public PuzzleSession Session { get; }

        /// <summary>Người chơi bấm nút trên popup giải xong.</summary>
        public event Action NextRequested;

        /// <summary>Ván vừa được giải (phát một lần).</summary>
        public event Action Solved;

        public LevelScope(LevelDocument level, PuzzleState startState, string title, bool isLastLevel,
            GameViews views, GameServices services)
        {
            LevelId = level.Id;
            Session = new PuzzleSession(level.Puzzle, startState);
            _input = views.BoardInput;

            _board = new BoardPresenter(Session, views.Board, services.LayoutConfig, services.Palette);
            _actions = new BoardActionPresenter(views.BoardInput, Session, Session, services.Paint);
            _hud = new HudPresenter(views.Hud, Session, Session, services.Hint, services.Check,
                new HudInfo(title, level.Difficulty, level.Tip));
            _popup = new SolvedPopupPresenter(views.Popup, Session, isLastLevel);
            _feedback = new FeedbackPresenter(Session, services.Haptics, views.Celebration, services.FeedbackConfig);
            new RuleLegendPresenter(views.Legend, level.RuleSpecs, services.ClueVisuals, services.Palette);

            _hud.HighlightRequested += _board.ShowHighlight;
            _input.DragPreviewChanged += _board.ShowDragPreview;
            _popup.NextRequested += OnNextRequested;
            Session.Changed += OnChanged;
        }

        public void Dispose()
        {
            Session.Changed -= OnChanged;
            _popup.NextRequested -= OnNextRequested;
            _input.DragPreviewChanged -= _board.ShowDragPreview;
            _hud.HighlightRequested -= _board.ShowHighlight;

            _feedback.Dispose();
            _popup.Dispose();
            _hud.Dispose();
            _actions.Dispose();
            _board.Dispose();
        }

        private void OnNextRequested() => NextRequested?.Invoke();

        private void OnChanged(SessionSnapshot snapshot)
        {
            if (snapshot.IsSolved) Solved?.Invoke();
        }
    }
}
