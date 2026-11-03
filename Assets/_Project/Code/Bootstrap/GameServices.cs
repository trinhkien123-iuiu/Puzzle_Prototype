using Puzzle.Application.Boosters;
using Puzzle.Presentation.Board;
using Puzzle.Presentation.Config;
using Puzzle.Presentation.Feedback;

namespace Puzzle.Bootstrap
{
    /// <summary>Dịch vụ và cấu hình dùng chung cho mọi level, tạo một lần trong <see cref="GameBootstrap"/>.</summary>
    internal sealed class GameServices
    {
        public BoardLayoutConfig LayoutConfig;
        public PaletteConfig Palette;
        public FeedbackConfig FeedbackConfig;
        public ClueVisuals ClueVisuals;
        public IBooster Hint;
        public IBooster Check;
        public IHaptics Haptics;

        /// <summary>Màu cọ đang chọn; giữ nguyên khi qua màn.</summary>
        public PaintSelection Paint;
    }
}
