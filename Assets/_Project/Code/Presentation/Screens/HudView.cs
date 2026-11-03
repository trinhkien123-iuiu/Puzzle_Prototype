using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.Screens
{
    /// <summary>
    /// HUD màn chơi: tiêu đề, độ khó, băng-rôn hướng dẫn và các nút Undo/Restart/Gợi ý/Kiểm tra.
    /// Câm: chỉ phơi hàm hiển thị và sự kiện nút.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        [SerializeField] private Text _title;
        [SerializeField] private Text _difficulty;
        [SerializeField, Tooltip("Chữ trên băng-rôn hướng dẫn.")] private Text _message;
        [SerializeField] private Button _undo;
        [SerializeField] private Button _restart;
        [SerializeField] private Button _hint;
        [SerializeField] private Button _check;

        public event Action UndoClicked;
        public event Action RestartClicked;
        public event Action HintClicked;
        public event Action CheckClicked;

        public void SetTitle(string text) => _title.text = text;
        public void SetDifficulty(string text) => _difficulty.text = text;
        public void SetMessage(string text) => _message.text = text;

        public void SetButtons(bool canUndo, bool canRestart, bool canHint, bool canCheck)
        {
            _undo.interactable = canUndo;
            _restart.interactable = canRestart;
            _hint.interactable = canHint;
            _check.interactable = canCheck;
        }

        private void Awake()
        {
            _undo.onClick.AddListener(() => UndoClicked?.Invoke());
            _restart.onClick.AddListener(() => RestartClicked?.Invoke());
            _hint.onClick.AddListener(() => HintClicked?.Invoke());
            _check.onClick.AddListener(() => CheckClicked?.Invoke());
        }
    }
}
