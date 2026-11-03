using Puzzle.Presentation.Board;
using Puzzle.Presentation.Screens;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Editor.Greybox
{
    /// <summary>
    /// Dựng prefab của bàn chơi: ô (mảnh kính), cạnh (nét liền + nét chấm), hình minh hoạ trong kho hình.
    /// Art nằm ngay trên Image của prefab: muốn đổi art thì kéo sprite khác vào prefab.
    /// </summary>
    internal sealed class GreyboxPrefabs
    {
        private readonly GreyboxUiFactory _ui;

        public GreyboxPrefabs(GreyboxUiFactory ui)
        {
            _ui = ui;
        }

        /// <summary>
        /// Ô trên bàn. Có shader thì ô là RawImage, mỗi ô lấy một phần hoạ tiết theo vị trí (uvRect) nên vân chạy liền
        /// qua cả vùng: kiểu kính Glimmith, hoặc kiểu ngọc liền vùng khi có bộ Block Blast.
        /// </summary>
        public CellView BuildCell()
        {
            Material material = _ui.Blocks ? _ui.Art.GemGlass : _ui.Art.StainedGlass;
            if (_ui.Blocks && material == null) return BuildBlockCell("CellView");

            RectTransform root = _ui.CreateRect("CellView", null);
            var fill = root.gameObject.AddComponent<RawImage>();
            fill.raycastTarget = false;
            var view = root.gameObject.AddComponent<CellView>();
            GreyboxUiFactory.Assign(view, "_fill", fill);
            if (material != null)
            {
                // Shader dựng lại từ material kính gốc: mảnh, vân nứt, dải sáng hoặc viền vát dọc mép vùng.
                fill.texture = _ui.Art.GlassCells;
                fill.material = material;
                GreyboxUiFactory.Assign(view, "_glass", root.gameObject.AddComponent<GlassCellMesh>());
            }
            else
            {
                fill.texture = _ui.Art.Glass.texture;
            }
            return Save(root, GreyboxPaths.BoardPrefabs, "CellView").GetComponent<CellView>();
        }

        /// <summary>Ô là một khối sprite (ô trống, khối ngọc); dùng cho hình trong kho hình, hoặc bàn khi thiếu shader.</summary>
        public CellView BuildBlockCell(string name)
        {
            RectTransform root = _ui.CreateRect(name, null);
            Image block = _ui.AddImage(root, Color.white, false, _ui.Art.Blocks.CellEmpty);
            var view = root.gameObject.AddComponent<CellView>();
            GreyboxUiFactory.Assign(view, "_fill", block);
            return Save(root, GreyboxPaths.BoardPrefabs, name).GetComponent<CellView>();
        }

        /// <summary>Tấm nền trơn dưới mỗi ô; chồng lên nhau thành viền đậm quanh mép bàn.</summary>
        public CellView BuildCellOutline()
        {
            RectTransform root = _ui.CreateRect("CellOutline", null);
            Image fill = _ui.AddImage(root, Color.black, false);
            var view = root.gameObject.AddComponent<CellView>();
            GreyboxUiFactory.Assign(view, "_fill", fill);
            return Save(root, GreyboxPaths.BoardPrefabs, "CellOutline").GetComponent<CellView>();
        }

        public EdgeView BuildEdge()
        {
            RectTransform root = _ui.CreateRect("EdgeView", null);
            Image line = _ui.AddImage(_ui.Stretch(_ui.CreateRect("Line", root)), Color.black, false);
            Image dots = _ui.AddImage(_ui.Stretch(_ui.CreateRect("Dots", root)), Color.black, false, _ui.Art.Dash);
            dots.type = Image.Type.Tiled;

            var view = root.gameObject.AddComponent<EdgeView>();
            GreyboxUiFactory.Assign(view, "_line", line);
            GreyboxUiFactory.Assign(view, "_dots", dots);
            return Save(root, GreyboxPaths.BoardPrefabs, "EdgeView").GetComponent<EdgeView>();
        }

        public ShapeIconView BuildShapeIcon(CellView cellPrefab)
        {
            RectTransform root = _ui.CreateRect("ShapeIcon", null);
            var layout = root.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = layout.preferredHeight = 170f;
            RectTransform cells = _ui.Stretch(_ui.CreateRect("Cells", root));

            var view = root.gameObject.AddComponent<ShapeIconView>();
            GreyboxUiFactory.Assign(view, "_cellLayer", cells);
            GreyboxUiFactory.Assign(view, "_cellPrefab", cellPrefab);
            return Save(root, GreyboxPaths.ScreenPrefabs, "ShapeIcon").GetComponent<ShapeIconView>();
        }

        public static GameObject Save(RectTransform root, string folder, string name)
        {
            GreyboxAssets.EnsureFolder(folder);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, $"{folder}/{name}.prefab");
            Object.DestroyImmediate(root.gameObject);
            return prefab;
        }
    }
}
