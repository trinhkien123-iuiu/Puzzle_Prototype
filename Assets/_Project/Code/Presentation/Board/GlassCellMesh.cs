using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Presentation.Board
{
    /// <summary>
    /// Gắn thêm dữ liệu cho shader kính (Puzzle/UI/StainedGlass) vào mesh của ô: uv1 = toạ độ trong ô 0..1,
    /// uv2 = (mặt nạ cạnh, mặt nạ góc) để shader vẽ dải sáng dọc mép vùng. Câm: chỉ nhận mặt nạ từ CellView.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public sealed class GlassCellMesh : BaseMeshEffect
    {
        private const AdditionalCanvasShaderChannels Channels =
            AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;

        private int _sides;
        private int _corners;

        /// <summary>Bit cạnh: 1 trái, 2 dưới, 4 phải, 8 trên. Bit góc lõm: 1 dưới-phải, 2 dưới-trái, 4 trên-phải, 8 trên-trái.</summary>
        public void SetMasks(int sides, int corners)
        {
            if (sides == _sides && corners == _corners) return;
            _sides = sides;
            _corners = corners;
            graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;
            Rect rect = graphic.rectTransform.rect;
            var vertex = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                vertex.uv1 = new Vector4((vertex.position.x - rect.xMin) / rect.width, (vertex.position.y - rect.yMin) / rect.height);
                vertex.uv2 = new Vector4(_sides, _corners);
                vh.SetUIVertex(vertex, i);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // Canvas mặc định không gửi uv1/uv2 xuống shader; bật lên để mặt nạ tới được shader.
            Canvas canvas = graphic != null ? graphic.canvas : null;
            if (canvas != null) canvas.additionalShaderChannels |= Channels;
        }
    }
}
