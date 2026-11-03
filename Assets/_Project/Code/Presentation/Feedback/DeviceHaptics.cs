using Puzzle.Presentation.Config;
using UnityEngine;

namespace Puzzle.Presentation.Feedback
{
    /// <summary>
    /// Rung trên máy thật. Android: rung ngắn qua Vibrator (JNI) vì <see cref="Handheld.Vibrate"/> luôn rung ~0,4s,
    /// quá dài cho mỗi thao tác. Dùng Handheld.Vibrate cho lúc giải xong, việc này cũng khiến Unity tự thêm quyền VIBRATE.
    /// Editor và nền tảng khác: không làm gì.
    /// </summary>
    public sealed class DeviceHaptics : IHaptics
    {
        private readonly FeedbackConfig _config;

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject _vibrator;
#endif

        public DeviceHaptics(FeedbackConfig config)
        {
            _config = config;
        }

        public void Play(HapticPulse pulse)
        {
            if (!_config.HapticsEnabled) return;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            if (pulse == HapticPulse.Success)
            {
                Handheld.Vibrate();
                return;
            }
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            VibrateAndroid(_config.LightPulseMs);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void VibrateAndroid(long milliseconds)
        {
            try
            {
                if (_vibrator == null)
                {
                    using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }
                _vibrator?.Call("vibrate", milliseconds);
            }
            catch (AndroidJavaException error)
            {
                // Máy không có bộ rung hoặc bị chặn quyền: bỏ qua, rung chỉ là phản hồi phụ.
                Debug.LogWarning($"Vibration unavailable: {error.Message}");
            }
        }
#endif
    }
}
