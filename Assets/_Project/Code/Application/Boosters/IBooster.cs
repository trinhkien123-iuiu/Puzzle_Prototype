using Puzzle.Application.Session;

namespace Puzzle.Application.Boosters
{
    /// <summary>
    /// Một booster. Không bao giờ tự sửa state: muốn đổi thì trả command trong <see cref="BoosterResult"/>
    /// để Presenter đưa vào session, nhờ vậy booster cũng undo được như thao tác tay.
    /// Hợp đồng: chỉ gọi <see cref="Use"/> khi <see cref="CanUse"/> trả true.
    /// </summary>
    public interface IBooster
    {
        bool CanUse(IPuzzleReader puzzle);
        BoosterResult Use(IPuzzleReader puzzle);
    }
}
