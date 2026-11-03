using System.Collections.Generic;
using Puzzle.Presentation.Screens;
using UnityEngine;
using UnityEngine.UI;
using L = Puzzle.Editor.Greybox.GreyboxLayout;

namespace Puzzle.Editor.Greybox
{
    /// <summary>
    /// Dựng prefab giao diện quanh bàn: HUD (tiêu đề, băng-rôn hướng dẫn, nút), kho hình, bảng màu, thanh Hoàn thành.
    /// Bố cục lấy từ <see cref="GreyboxLayout"/>.
    /// </summary>
    internal sealed class GreyboxScreenPrefabs
    {
        private const int PaintSlots = 9;
        private static readonly Color ButtonBlue = new Color(0.16f, 0.45f, 0.95f);
        private static readonly Color ButtonOrange = new Color(0.98f, 0.5f, 0.12f);
        private static readonly Color ButtonPurple = new Color(0.6f, 0.3f, 0.92f);
        private static readonly Color ButtonGreen = new Color(0.1f, 0.78f, 0.2f);
        private readonly GreyboxUiFactory _ui;

        public GreyboxScreenPrefabs(GreyboxUiFactory ui)
        {
            _ui = ui;
        }

        public HudView BuildHud()
        {
            RectTransform root = _ui.Stretch(_ui.CreateRect("Hud", null));
            var view = root.gameObject.AddComponent<HudView>();

            RectTransform top = _ui.Band(_ui.CreateRect("TitleBar", root), true, L.TitleTop, L.TitleHeight, L.Margin);
            Text title = _ui.AddText(_ui.CreateRect("Title", top), "Màn 1", 54, Color.white, TextAnchor.MiddleLeft);
            _ui.Stretch((RectTransform)title.transform);
            Text difficulty = _ui.AddText(_ui.CreateRect("Difficulty", top), "Độ khó 1", 44, Color.white, TextAnchor.MiddleRight);
            _ui.Stretch((RectTransform)difficulty.transform);

            RectTransform banner = _ui.Band(_ui.CreateRect("Banner", root), true, L.BannerTop, L.BannerHeight, L.Margin);
            _ui.AddImage(banner, Color.white, false, _ui.Blocks ? _ui.Art.Blocks.Banner : _ui.Art.Banner);
            // Băng-rôn kính có hai đầu nhọn trang trí nên chừa lề rộng; thanh nghiêng Block Blast chỉ cần lề nhỏ.
            float bannerInset = _ui.Blocks ? 70f : 200f;
            Text message = _ui.AddText(_ui.Stretch(_ui.CreateRect("Message", banner), bannerInset, 30, bannerInset, 30), "", 36, _ui.Ink);

            GreyboxUiFactory.Assign(view, "_title", title);
            GreyboxUiFactory.Assign(view, "_difficulty", difficulty);
            GreyboxUiFactory.Assign(view, "_message", message);
            BuildButtonRow(root, view);
            return GreyboxPrefabs.Save(root, GreyboxPaths.ScreenPrefabs, "Hud").GetComponent<HudView>();
        }

        public RuleLegendView BuildRuleLegend(ShapeIconView iconPrefab)
        {
            RectTransform root = _ui.Stretch(_ui.CreateRect("RuleLegend", null));
            _ui.AddImage(root, Color.white, false, _ui.Blocks ? _ui.Art.Blocks.Panel : _ui.Art.Scroll);
            var view = root.gameObject.AddComponent<RuleLegendView>();

            // Cuộn giấy có hai lõi cuộn hai bên nên chừa lề rộng; khung Block Blast chỉ cần lề theo viền.
            float side = _ui.Blocks ? 50f : 120f;
            // Khung Block Blast không có lõi cuộn trên/dưới nên dồn chữ lên, chừa chỗ cho hình to hơn.
            bool blocks = _ui.Blocks;
            Text title = _ui.AddText(_ui.Band(_ui.CreateRect("Title", root), true, blocks ? 14f : 44f, 56f), "Kho hình", 48, _ui.Ink);
            RectTransform descriptionRect = _ui.Band(_ui.CreateRect("Description", root), true, blocks ? 70f : 108f, blocks ? 60f : 72f, side);
            Text description = _ui.AddText(descriptionRect, "", blocks ? 30 : 36, _ui.Ink);

            RectTransform row = _ui.Stretch(_ui.CreateRect("Shapes", root), side, blocks ? 22f : 30f, side, blocks ? 138f : 190f);
            var group = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            group.spacing = 30f;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandWidth = group.childForceExpandHeight = false;

            GreyboxUiFactory.Assign(view, "_title", title);
            GreyboxUiFactory.Assign(view, "_description", description);
            GreyboxUiFactory.Assign(view, "_iconRow", row);
            GreyboxUiFactory.Assign(view, "_iconPrefab", iconPrefab);
            return GreyboxPrefabs.Save(root, GreyboxPaths.ScreenPrefabs, "RuleLegend").GetComponent<RuleLegendView>();
        }

        public PaletteView BuildPalette()
        {
            RectTransform root = _ui.Stretch(_ui.CreateRect("Palette", null));
            var group = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            group.spacing = 14f;
            group.childAlignment = TextAnchor.MiddleCenter;
            group.childControlWidth = group.childControlHeight = false;
            group.childForceExpandWidth = group.childForceExpandHeight = false;

            var swatches = new List<Object>();
            for (int i = 0; i < PaintSlots; i++)
            {
                RectTransform swatch = _ui.CreateRect($"Swatch{i + 1}", root);
                swatch.sizeDelta = new Vector2(96f, 96f);
                Sprite swatchSprite = _ui.Blocks ? _ui.Art.Blocks.Gems[i % _ui.Art.Blocks.Gems.Length] : _ui.Art.Panel;
                Image image = _ui.AddImage(swatch, Color.white, true, swatchSprite); // ô màu phẳng viền tối, hoặc khối ngọc
                var button = swatch.gameObject.AddComponent<Button>();
                button.targetGraphic = image;
                swatches.Add(button);
            }

            RectTransform marker = _ui.Stretch(_ui.CreateRect("SelectedMarker", root), -10, -10, -10, -10);
            Sprite markerSprite = _ui.Blocks ? _ui.Art.Blocks.SelectFrame : _ui.Art.Frame;
            _ui.AddImage(marker, _ui.Blocks ? new Color(1f, 0.93f, 0.4f) : new Color(1f, 0.95f, 0.55f), false, markerSprite);
            marker.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;

            var view = root.gameObject.AddComponent<PaletteView>();
            GreyboxUiFactory.AssignList(view, "_swatches", swatches.ToArray());
            GreyboxUiFactory.Assign(view, "_marker", marker);
            return GreyboxPrefabs.Save(root, GreyboxPaths.ScreenPrefabs, "Palette").GetComponent<PaletteView>();
        }

        /// <summary>Thanh "Hoàn thành" thay chỗ bảng màu khi giải xong, có nút Tiếp tục (như nút Continue của game tham khảo).</summary>
        public SolvedPopupView BuildSolvedBar()
        {
            RectTransform root = _ui.Stretch(_ui.CreateRect("SolvedBar", null));
            var view = root.gameObject.AddComponent<SolvedPopupView>();

            RectTransform panel = _ui.Stretch(_ui.CreateRect("Root", root));
            _ui.AddImage(panel, _ui.Paper, true, _ui.Blocks ? _ui.Art.Blocks.Banner : _ui.Art.Panel);
            Text message = _ui.AddText(_ui.Stretch(_ui.CreateRect("Message", panel), 30, 0, 520, 0), "Hoàn thành!", 54,
                _ui.Ink, TextAnchor.MiddleLeft);

            Button next = _ui.CreateButton("Next", panel, "Tiếp tục", out Text label, _ui.Blocks ? ButtonGreen : (Color?)null);
            _ui.Stretch((RectTransform)next.transform, 540, 14, 20, 14);
            if (!_ui.Blocks) next.targetGraphic.color = new Color(0.55f, 0.85f, 0.6f);

            GreyboxUiFactory.Assign(view, "_root", panel.gameObject);
            GreyboxUiFactory.Assign(view, "_message", message);
            GreyboxUiFactory.Assign(view, "_buttonLabel", label);
            GreyboxUiFactory.Assign(view, "_next", next);
            panel.gameObject.SetActive(false);
            return GreyboxPrefabs.Save(root, GreyboxPaths.ScreenPrefabs, "SolvedBar").GetComponent<SolvedPopupView>();
        }

        private void BuildButtonRow(RectTransform root, HudView view)
        {
            RectTransform row = _ui.Band(_ui.CreateRect("Buttons", root), false, L.ButtonsBottom, L.ButtonsHeight, L.Margin);
            var group = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            group.spacing = 20f;
            group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandWidth = group.childForceExpandHeight = true;

            // Kiểu Block Blast: mỗi nút một màu như nút Ok/Retry/Replay của game; không có bộ đó thì nút giấy.
            bool blocks = _ui.Blocks;
            GreyboxUiFactory.Assign(view, "_undo", _ui.CreateButton("Undo", row, "Hoàn tác", out _, blocks ? ButtonBlue : (Color?)null));
            GreyboxUiFactory.Assign(view, "_restart", _ui.CreateButton("Restart", row, "Chơi lại", out _, blocks ? ButtonOrange : (Color?)null));
            GreyboxUiFactory.Assign(view, "_hint", _ui.CreateButton("Hint", row, "Gợi ý", out _, blocks ? ButtonPurple : (Color?)null));
            GreyboxUiFactory.Assign(view, "_check", _ui.CreateButton("Check", row, "Kiểm tra", out _, blocks ? ButtonGreen : (Color?)null));
        }
    }
}
