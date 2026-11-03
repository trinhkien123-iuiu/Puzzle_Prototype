using System;
using Puzzle.Application.Session;

namespace Puzzle.Presentation.Screens
{
    /// <summary>Hiện popup khi ván vừa được giải; bấm nút thì xin chuyển level qua <see cref="NextRequested"/>.</summary>
    public sealed class SolvedPopupPresenter : IDisposable
    {
        private readonly SolvedPopupView _view;
        private readonly IPuzzleReader _reader;
        private readonly bool _isLastLevel;

        public event Action NextRequested;

        public SolvedPopupPresenter(SolvedPopupView view, IPuzzleReader reader, bool isLastLevel)
        {
            _view = view;
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _isLastLevel = isLastLevel;

            _reader.Changed += OnChanged;
            _view.NextClicked += OnNextClicked;
            OnChanged(_reader.Snapshot);
        }

        public void Dispose()
        {
            _reader.Changed -= OnChanged;
            _view.NextClicked -= OnNextClicked;
            _view.Hide();
        }

        private void OnChanged(SessionSnapshot snapshot)
        {
            if (!snapshot.IsSolved)
            {
                _view.Hide();
                return;
            }
            _view.Show(
                _isLastLevel ? "Hoàn thành mọi màn thử!" : "Hoàn thành!",
                _isLastLevel ? "Chơi lại từ đầu" : "Tiếp tục");
        }

        private void OnNextClicked() => NextRequested?.Invoke();
    }
}
