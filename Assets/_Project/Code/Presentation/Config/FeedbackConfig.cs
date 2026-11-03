using UnityEngine;

namespace Puzzle.Presentation.Config
{
    /// <summary>Thông số hiệu ứng và rung.</summary>
    [CreateAssetMenu(menuName = "Puzzle/Config/Feedback", fileName = "FeedbackConfig")]
    public sealed class FeedbackConfig : ScriptableObject
    {
        [SerializeField, Tooltip("Thời lượng hiệu ứng nhún bàn khi giải xong (giây).")]
        private float _celebrateSeconds = 0.6f;

        [SerializeField, Tooltip("Tỉ lệ phóng to lớn nhất của hiệu ứng giải xong.")]
        private float _celebrateScale = 1.08f;

        [SerializeField, Tooltip("Độ dài rung nhẹ mỗi thao tác (ms, Android).")]
        private int _lightPulseMs = 15;

        [SerializeField, Tooltip("Bật/tắt rung.")]
        private bool _hapticsEnabled = true;

        public float CelebrateSeconds => _celebrateSeconds;
        public float CelebrateScale => _celebrateScale;
        public int LightPulseMs => _lightPulseMs;
        public bool HapticsEnabled => _hapticsEnabled;
    }
}
