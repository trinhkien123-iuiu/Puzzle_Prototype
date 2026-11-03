using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.Screens
{
    /// <summary>Popup "Giải xong". Câm: hiện/ẩn, đặt chữ, báo khi bấm nút.</summary>
    public sealed class SolvedPopupView : MonoBehaviour
    {
        [SerializeField, Tooltip("Gốc của popup; bật/tắt để hiện/ẩn.")]
        private GameObject _root;

        [SerializeField] private Text _message;
        [SerializeField] private Text _buttonLabel;
        [SerializeField] private Button _next;

        public event Action NextClicked;

        public void Show(string message, string buttonLabel)
        {
            _message.text = message;
            _buttonLabel.text = buttonLabel;
            _root.SetActive(true);
        }

        public void Hide() => _root.SetActive(false);

        private void Awake() => _next.onClick.AddListener(() => NextClicked?.Invoke());
    }
}
