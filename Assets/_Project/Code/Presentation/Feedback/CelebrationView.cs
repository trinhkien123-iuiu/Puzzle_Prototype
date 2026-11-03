using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzle.Presentation.Feedback
{
    /// <summary>
    /// Hiệu ứng khi giải xong: nhún bàn, nảy một chữ khen ngẫu nhiên kèm ánh sáng loé, phát âm thanh.
    /// Mỗi thay đổi trên bàn phát một tiếng "đặt khối". Phần chữ khen và âm thanh là tuỳ chọn.
    /// </summary>
    public sealed class CelebrationView : MonoBehaviour
    {
        private const float PraisePopSeconds = 0.35f;
        private const float PraiseHoldSeconds = 1.1f;
        private const float PraiseFadeSeconds = 0.35f;

        [SerializeField, Tooltip("Đối tượng được nhún (thường là khung bàn).")]
        private RectTransform _target;

        [SerializeField, Tooltip("Tuỳ chọn: các chữ khen (đang tắt); giải xong bật ngẫu nhiên một chữ.")]
        private List<RectTransform> _praises = new List<RectTransform>();

        [SerializeField, Tooltip("Tuỳ chọn: ánh sáng loé xoay sau chữ khen.")]
        private RectTransform _flash;

        [SerializeField] private AudioSource _audio;

        [SerializeField, Tooltip("Tiếng mỗi lần bàn thay đổi (gộp, xoá, tô).")]
        private AudioClip _changeClip;

        [SerializeField, Tooltip("Tiếng khi vừa giải xong.")]
        private AudioClip _solvedClip;

        private Coroutine _running;
        private Coroutine _praiseRunning;

        public void Play(float seconds, float peakScale)
        {
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(Pulse(seconds, peakScale));
            PlayClip(_solvedClip);
            if (_praises.Count > 0)
            {
                if (_praiseRunning != null) StopCoroutine(_praiseRunning);
                _praiseRunning = StartCoroutine(Praise(_praises[Random.Range(0, _praises.Count)]));
            }
        }

        public void PlayChange() => PlayClip(_changeClip);

        private void PlayClip(AudioClip clip)
        {
            if (_audio != null && clip != null) _audio.PlayOneShot(clip);
        }

        private IEnumerator Pulse(float seconds, float peakScale)
        {
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                // Nửa chu kỳ sin: 0 → 1 → 0, nên kết thúc đúng ở cỡ gốc.
                float scale = Mathf.Lerp(1f, peakScale, Mathf.Sin(t / seconds * Mathf.PI));
                _target.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            _target.localScale = Vector3.one;
            _running = null;
        }

        // Chữ nảy từ 0 lên quá cỡ một chút rồi về 1, giữ một lúc, rồi mờ dần; ánh sáng loé xoay phía sau.
        private IEnumerator Praise(RectTransform praise)
        {
            HideAllPraise();
            praise.gameObject.SetActive(true);
            var group = praise.GetComponent<CanvasGroup>();
            if (_flash != null) _flash.gameObject.SetActive(true);

            float total = PraisePopSeconds + PraiseHoldSeconds + PraiseFadeSeconds;
            for (float t = 0f; t < total; t += Time.unscaledDeltaTime)
            {
                float pop = Mathf.Clamp01(t / PraisePopSeconds);
                float scale = pop < 1f ? EaseOutBack(pop) : 1f;
                praise.localScale = new Vector3(scale, scale, 1f);

                float fade = 1f - Mathf.Clamp01((t - PraisePopSeconds - PraiseHoldSeconds) / PraiseFadeSeconds);
                if (group != null) group.alpha = fade;
                if (_flash != null)
                {
                    _flash.localRotation = Quaternion.Euler(0f, 0f, -t * 90f);
                    _flash.localScale = Vector3.one * (scale * 1.4f * fade);
                }
                yield return null;
            }
            HideAllPraise();
            _praiseRunning = null;
        }

        private static float EaseOutBack(float x)
        {
            const float C1 = 1.70158f, C3 = C1 + 1f;
            return 1f + C3 * Mathf.Pow(x - 1f, 3f) + C1 * Mathf.Pow(x - 1f, 2f);
        }

        private void HideAllPraise()
        {
            foreach (RectTransform praise in _praises)
            {
                if (praise != null) praise.gameObject.SetActive(false);
            }
            if (_flash != null) _flash.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (_target != null) _target.localScale = Vector3.one;
            HideAllPraise();
            _running = null;
            _praiseRunning = null;
        }
    }
}
