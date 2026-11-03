using System;
using Puzzle.Application.Session;
using Puzzle.Presentation.Config;

namespace Puzzle.Presentation.Feedback
{
    /// <summary>
    /// Nghe sự kiện Changed và tự chọn phản hồi: rung nhẹ + tiếng đặt khối mỗi thay đổi,
    /// rung + nhún bàn + chữ khen khi vừa giải xong.
    /// Logic không gọi rung hay hiệu ứng trực tiếp.
    /// </summary>
    public sealed class FeedbackPresenter : IDisposable
    {
        private readonly IPuzzleReader _reader;
        private readonly IHaptics _haptics;
        private readonly CelebrationView _celebration;
        private readonly FeedbackConfig _config;
        private bool _wasSolved;

        public FeedbackPresenter(IPuzzleReader reader, IHaptics haptics, CelebrationView celebration, FeedbackConfig config)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _haptics = haptics;
            _celebration = celebration;
            _config = config;
            _wasSolved = reader.Snapshot.IsSolved;
            _reader.Changed += OnChanged;
        }

        public void Dispose() => _reader.Changed -= OnChanged;

        private void OnChanged(SessionSnapshot snapshot)
        {
            bool justSolved = snapshot.IsSolved && !_wasSolved;
            _wasSolved = snapshot.IsSolved;

            if (justSolved)
            {
                _haptics.Play(HapticPulse.Success);
                _celebration.Play(_config.CelebrateSeconds, _config.CelebrateScale);
                return;
            }
            _haptics.Play(HapticPulse.Light);
            _celebration.PlayChange();
        }
    }
}
