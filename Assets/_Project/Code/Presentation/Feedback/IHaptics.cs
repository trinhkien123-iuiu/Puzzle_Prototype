namespace Puzzle.Presentation.Feedback
{
    /// <summary>Rung máy. Tách interface vì Android, iOS và Editor rung theo cách khác nhau (Editor không rung).</summary>
    public interface IHaptics
    {
        void Play(HapticPulse pulse);
    }
}
