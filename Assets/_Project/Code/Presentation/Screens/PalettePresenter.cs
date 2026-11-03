using System;
using Puzzle.Presentation.Board;
using Puzzle.Presentation.Config;

namespace Puzzle.Presentation.Screens
{
    /// <summary>Nối bảng màu trên màn hình với màu cọ đang chọn (<see cref="PaintSelection"/>).</summary>
    public sealed class PalettePresenter : IDisposable
    {
        private readonly PaletteView _view;
        private readonly PaintSelection _selection;

        public PalettePresenter(PaletteView view, PaintSelection selection, PaletteConfig palette)
        {
            _view = view;
            _selection = selection ?? throw new ArgumentNullException(nameof(selection));

            _view.SetColors(palette.Paints);
            _view.SetSprites(palette.PaintSprites);
            _view.MarkSelected(_selection.Selected - 1);
            _view.SwatchClicked += OnSwatchClicked;
            _selection.Changed += OnSelectionChanged;
        }

        public void Dispose()
        {
            _view.SwatchClicked -= OnSwatchClicked;
            _selection.Changed -= OnSelectionChanged;
        }

        private void OnSwatchClicked(int index) => _selection.Select(index + 1);

        private void OnSelectionChanged(int paint) => _view.MarkSelected(paint - 1);
    }
}
