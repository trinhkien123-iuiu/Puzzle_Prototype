using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.Screens
{
    /// <summary>Hàng ô màu để chọn màu tô vùng. Câm: hiện màu, đánh dấu ô đang chọn, báo ô được bấm.</summary>
    public sealed class PaletteView : MonoBehaviour
    {
        [SerializeField, Tooltip("Các nút màu theo thứ tự; màu hiện trên Image của chính nút.")]
        private List<Button> _swatches = new List<Button>();

        [SerializeField, Tooltip("Khung đánh dấu màu đang chọn, được dời tới nút được chọn.")]
        private RectTransform _marker;

        /// <summary>Bấm vào ô màu thứ n (0-based).</summary>
        public event Action<int> SwatchClicked;

        public int SwatchCount => _swatches.Count;

        public void SetColors(IReadOnlyList<Color> colors)
        {
            for (int i = 0; i < _swatches.Count; i++)
            {
                bool used = i < colors.Count;
                _swatches[i].gameObject.SetActive(used);
                if (used) _swatches[i].targetGraphic.color = colors[i];
            }
        }

        /// <summary>Ô màu hiện bằng sprite khối thay cho màu phẳng (giữ màu gốc của sprite).</summary>
        public void SetSprites(IReadOnlyList<Sprite> sprites)
        {
            for (int i = 0; i < _swatches.Count && i < sprites.Count; i++)
            {
                if (sprites[i] == null || !(_swatches[i].targetGraphic is Image image)) continue;
                image.sprite = sprites[i];
                image.type = Image.Type.Simple;
                image.color = Color.white;
            }
        }

        public void MarkSelected(int index)
        {
            if (index < 0 || index >= _swatches.Count) return;
            _marker.SetParent(_swatches[index].transform, false);
            _marker.SetAsLastSibling();
        }

        private void Awake()
        {
            for (int i = 0; i < _swatches.Count; i++)
            {
                int index = i;
                _swatches[i].onClick.AddListener(() => SwatchClicked?.Invoke(index));
            }
        }
    }
}
