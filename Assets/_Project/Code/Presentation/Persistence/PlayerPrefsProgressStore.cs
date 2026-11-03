using System;
using Puzzle.Application.Progress;
using UnityEngine;

namespace Puzzle.Presentation.Persistence
{
    /// <summary>
    /// Lưu tiến độ vào PlayerPrefs dưới dạng JSON. Gọi PlayerPrefs.Save ngay trong <see cref="Save"/>
    /// vì trên mobile app có thể bị kill khi đang ở nền, không đợi Unity tự ghi lúc thoát.
    /// </summary>
    public sealed class PlayerPrefsProgressStore : IProgressStore
    {
        private readonly string _key;

        public PlayerPrefsProgressStore(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("Storage key is required.", nameof(key));
            _key = key;
        }

        public PlayerProgress Load()
        {
            string json = PlayerPrefs.GetString(_key, string.Empty);
            if (string.IsNullOrEmpty(json)) return new PlayerProgress();
            try
            {
                return JsonUtility.FromJson<PlayerProgress>(json) ?? new PlayerProgress();
            }
            catch (ArgumentException error)
            {
                // Dữ liệu hỏng thì bắt đầu lại còn hơn crash ngay màn đầu.
                Debug.LogWarning($"Progress data is corrupt, starting fresh: {error.Message}");
                return new PlayerProgress();
            }
        }

        public void Save(PlayerProgress progress)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            PlayerPrefs.SetString(_key, JsonUtility.ToJson(progress));
            PlayerPrefs.Save();
        }

        /// <summary>Xoá dữ liệu đã lưu (dùng cho test và nút reset debug).</summary>
        public void Clear()
        {
            PlayerPrefs.DeleteKey(_key);
            PlayerPrefs.Save();
        }
    }
}
