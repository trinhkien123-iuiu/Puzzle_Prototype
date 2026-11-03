using Puzzle.Presentation.Board;
using Puzzle.Presentation.Feedback;
using Puzzle.Presentation.Input;
using Puzzle.Presentation.Screens;

namespace Puzzle.Bootstrap
{
    /// <summary>Các view đặt sẵn trong scene Game, gom lại để truyền vào <see cref="LevelScope"/> gọn một tham số.</summary>
    internal sealed class GameViews
    {
        public BoardView Board;
        public BoardInput BoardInput;
        public HudView Hud;
        public SolvedPopupView Popup;
        public CelebrationView Celebration;
        public RuleLegendView Legend;
        public PaletteView Palette;
    }
}
