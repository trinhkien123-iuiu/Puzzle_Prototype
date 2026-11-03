using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Puzzle.Editor.Greybox
{
    /// <summary>
    /// Tạo các khối uGUI greybox (khung, chữ, nút) cho menu dựng scene. Chỉ dùng trong Editor;
    /// kết quả được lưu thành prefab/scene để chỉnh tay sau đó.
    /// </summary>
    internal sealed class GreyboxUiFactory
    {
        private static readonly Color DarkInk = new Color(0.2f, 0.17f, 0.14f);
        private static readonly Color CreamPaper = new Color(0.98f, 0.95f, 0.88f);
        private static readonly Color TextOutline = new Color(0.05f, 0.08f, 0.2f, 0.9f);

        private readonly Font _font;

        public GreyboxArt Art { get; }

        /// <summary>Có bộ Block Blast: chữ trắng viền tối trên khung xanh đậm; không thì chữ mực trên giấy.</summary>
        public bool Blocks => Art.Blocks != null;

        public Color Ink => Blocks ? Color.white : DarkInk;
        public Color Paper => Blocks ? Color.white : CreamPaper;

        public GreyboxUiFactory(GreyboxArt art)
        {
            Art = art;
            _font = Blocks && art.Blocks.Font != null ? art.Blocks.Font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        public RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = LayerMask.NameToLayer("UI");
            var rect = (RectTransform)go.transform;
            if (parent != null) rect.SetParent(parent, false);
            return rect;
        }

        /// <summary>Kéo giãn theo cha, chừa lề trái/dưới/phải/trên.</summary>
        public RectTransform Stretch(RectTransform rect, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        /// <summary>
        /// Dải ngang bám mép trên (fromTop = true) hoặc mép dưới của cha, cách mép <paramref name="offset"/>,
        /// cao <paramref name="height"/>, chừa lề hai bên <paramref name="margin"/>.
        /// </summary>
        public RectTransform Band(RectTransform rect, bool fromTop, float offset, float height, float margin = 0f)
        {
            float anchorY = fromTop ? 1f : 0f;
            rect.anchorMin = new Vector2(0f, anchorY);
            rect.anchorMax = new Vector2(1f, anchorY);
            rect.pivot = new Vector2(0.5f, anchorY);
            rect.sizeDelta = new Vector2(-2f * margin, height);
            rect.anchoredPosition = new Vector2(0f, fromTop ? -offset : offset);
            return rect;
        }

        /// <summary>Image; sprite có viền 9-slice thì tự để kiểu Sliced để co giãn không méo góc.</summary>
        public Image AddImage(RectTransform rect, Color color, bool raycast, Sprite sprite = null)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = raycast;
            return image;
        }

        public Text AddText(RectTransform rect, string content, int size, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var text = rect.gameObject.AddComponent<Text>();
            text.font = _font;
            text.text = content;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow; // chữ dài hơn khung vẫn hiện, không bị cắt mất
            text.raycastTarget = false;
            if (Blocks)
            {
                var outline = rect.gameObject.AddComponent<Outline>();
                outline.effectColor = TextOutline;
                outline.effectDistance = new Vector2(2f, -2f);
            }
            return text;
        }

        /// <summary>Nút: khung bo góc màu kem chữ mực, hoặc nút nghiêng tô màu <paramref name="tint"/> kiểu Block Blast.</summary>
        public Button CreateButton(string name, Transform parent, string label, out Text labelText, Color? tint = null)
        {
            RectTransform rect = CreateRect(name, parent);
            Image background = AddImage(rect, tint ?? Paper, true, Blocks ? Art.Blocks.Button : Art.Panel);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            labelText = AddText(Stretch(CreateRect("Label", rect), 8, 4, 8, 4), label, 44, Ink);
            return button;
        }

        /// <summary>Gán field [SerializeField] theo tên; báo lỗi ngay nếu tên field đã bị đổi trong code.</summary>
        public static void Assign(Object target, string field, Object value)
        {
            SerializedProperty property = Find(target, field, out SerializedObject serialized);
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Gán field kiểu danh sách tham chiếu theo tên.</summary>
        public static void AssignList(Object target, string field, Object[] values)
        {
            SerializedProperty property = Find(target, field, out SerializedObject serialized);
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedProperty Find(Object target, string field, out SerializedObject serialized)
        {
            serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.GetType().Name} has no serialized field '{field}'.");
            }
            return property;
        }
    }
}
