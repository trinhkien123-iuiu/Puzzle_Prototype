using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Puzzle.Editor.Greybox
{
    /// <summary>
    /// Art tạm tự sinh bằng code (giấy da, kính màu, nét chấm, khung bo góc, nền đá) để greybox có cảm giác
    /// gần phong cách tham khảo; Banner, Scroll, Frame lấy từ Art/Glimmith nếu có. File đã có thì giữ nguyên,
    /// nên artist thay PNG cùng tên (hoặc kéo sprite khác vào prefab) là xong.
    /// </summary>
    internal sealed class GreyboxArt
    {
        private const string Folder = GreyboxPaths.Root + "/Art/Generated";
        private const string ReferenceFolder = GreyboxPaths.Root + "/Art/Glimmith";

        public Sprite Parchment { get; private set; }
        public Sprite Glass { get; private set; }
        public Sprite Dash { get; private set; }
        public Sprite Panel { get; private set; }
        public Sprite Stone { get; private set; }
        public Sprite Frame { get; private set; }

        public Sprite Banner { get; private set; }
        public Sprite Scroll { get; private set; }

        /// <summary>Material kính dựng lại từ material gốc; null nếu thiếu shader hoặc texture mask (khi đó ô dùng sprite Glass).</summary>
        public Material StainedGlass { get; private set; }
        public Texture2D GlassCells { get; private set; }

        /// <summary>Cùng shader kính nhưng bật viền vát kiểu khối ngọc, tắt dải sáng; dùng cho bàn kiểu Block Blast.</summary>
        public Material GemGlass { get; private set; }

        public static GreyboxArt LoadOrCreate()
        {
            GreyboxAssets.EnsureFolder(Folder);
            Sprite frame = LoadReference("Frame", Vector4.zero);
            Sprite glass = LoadReference("Glass", Vector4.zero, TextureWrapMode.Repeat);
            Sprite stone = LoadReference("Stone", Vector4.zero, TextureWrapMode.Repeat);
            var art = new GreyboxArt
            {
                Parchment = LoadOrCreate("Parchment", 256, ParchmentPixel, Vector4.zero, TextureWrapMode.Clamp),
                Glass = glass != null ? glass : LoadOrCreate("Glass", 256, GlassPixel, Vector4.zero, TextureWrapMode.Clamp),
                Dash = LoadOrCreate("Dash", 32, DashPixel, Vector4.zero, TextureWrapMode.Repeat, height: 8),
                Panel = LoadOrCreate("Panel", 64, PanelPixel, new Vector4(20, 20, 20, 20), TextureWrapMode.Clamp),
                Stone = stone != null ? stone : LoadOrCreate("Stone", 256, StonePixel, Vector4.zero, TextureWrapMode.Clamp),
                Frame = frame != null ? frame : LoadOrCreate("Frame", 64, FramePixel, new Vector4(20, 20, 20, 20), TextureWrapMode.Clamp),
            };
            // Băng-rôn và cuộn giấy lấy từ Art/Glimmith nếu có, không thì dùng khung bo góc tự sinh.
            art.Banner = LoadReference("Banner", new Vector4(200, 40, 200, 40)) ?? art.Panel;
            art.Scroll = LoadReference("Scroll", new Vector4(120, 90, 120, 90)) ?? art.Panel;
            art.GlassCells = LoadMask("GlassCells");
            Texture2D distortion = LoadMask("GlassDistortion");
            art.StainedGlass = LoadOrCreateGlassMaterial(art.GlassCells, distortion, "StainedGlass", null);
            art.GemGlass = LoadOrCreateGlassMaterial(art.GlassCells, distortion, "GemGlass", ConfigureGem);
            art.Blocks = BlockBlastArt.Load();
            return art;
        }

        /// <summary>Bộ giao diện kiểu Block Blast trong Art/BlockBlast; null nếu chưa có thì dùng art kính/giấy da.</summary>
        public BlockBlastArt Blocks { get; private set; }

        internal sealed class BlockBlastArt
        {
            private const string BlockFolder = GreyboxPaths.Root + "/Art/BlockBlast";
            private const int GemCount = 8;

            public Sprite[] Gems { get; private set; }
            public Color[] GemColors { get; private set; }
            public Sprite CellEmpty { get; private set; }
            public Sprite CellPreview { get; private set; }
            public Sprite BoardPanel { get; private set; }
            public Sprite Background { get; private set; }
            public Sprite Banner { get; private set; }
            public Sprite Button { get; private set; }
            public Sprite Panel { get; private set; }
            public Sprite SelectFrame { get; private set; }
            public Sprite Sparkle { get; private set; }
            public Sprite[] Praises { get; private set; }
            public Font Font { get; private set; }
            public AudioClip ChangeClip { get; private set; }
            public AudioClip SolvedClip { get; private set; }

            // Màu trung bình của từng khối ngọc (Gem1..8: vàng, lục, lam nhạt, cam, hồng, lam, đỏ, tím),
            // dùng cho chỗ cần màu phẳng như vùng sai pha đỏ, nét giữa vùng.
            private static readonly Color[] AverageGemColors =
            {
                new Color(0.95f, 0.82f, 0.26f), new Color(0.49f, 0.83f, 0.09f), new Color(0.35f, 0.73f, 1f),
                new Color(0.92f, 0.49f, 0.13f), new Color(0.91f, 0.33f, 0.77f), new Color(0.18f, 0.5f, 0.95f),
                new Color(0.86f, 0.24f, 0.38f), new Color(0.48f, 0.28f, 0.91f),
            };

            public static BlockBlastArt Load()
            {
                if (!File.Exists($"{BlockFolder}/Gem1.png")) return null;
                var gems = new Sprite[GemCount];
                for (int i = 0; i < GemCount; i++) gems[i] = Sprite($"Gem{i + 1}", Vector4.zero);
                return new BlockBlastArt
                {
                    Gems = gems,
                    GemColors = AverageGemColors,
                    CellEmpty = Sprite("CellEmpty", Vector4.zero),
                    CellPreview = Sprite("CellPreview", Vector4.zero),
                    BoardPanel = Sprite("BoardPanel", new Vector4(40, 40, 40, 40)),
                    Background = Sprite("Background", Vector4.zero),
                    Banner = Sprite("Banner", new Vector4(40, 12, 40, 12)),
                    Button = Sprite("Button", new Vector4(40, 12, 40, 12)),
                    Panel = Sprite("Panel", new Vector4(60, 60, 60, 60)),
                    SelectFrame = Sprite("SelectFrame", new Vector4(70, 70, 70, 70)),
                    Sparkle = Sprite("Sparkle", Vector4.zero),
                    Praises = new[]
                    {
                        Sprite("PraiseAmazing", Vector4.zero), Sprite("PraiseFantastic", Vector4.zero),
                        Sprite("PraiseGreat", Vector4.zero), Sprite("PraiseIncredible", Vector4.zero),
                        Sprite("PraiseTerrific", Vector4.zero),
                    },
                    Font = AssetDatabase.LoadAssetAtPath<Font>($"{BlockFolder}/Roboto-Bold.ttf"),
                    ChangeClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{BlockFolder}/Audio/put.ogg"),
                    SolvedClip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{BlockFolder}/Audio/c4.ogg"),
                };
            }

            private static Sprite Sprite(string name, Vector4 border)
            {
                string path = $"{BlockFolder}/{name}.png";
                if (!File.Exists(path)) return null;
                if (AssetImporter.GetAtPath(path) is TextureImporter importer &&
                    (importer.textureType != TextureImporterType.Sprite || importer.spriteBorder != border))
                {
                    ConfigureImporter(path, border, TextureWrapMode.Clamp);
                }
                return AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
        }

        /// <summary>Texture mask (dữ liệu, không phải màu): tuyến tính, lặp, có mipmap.</summary>
        private static Texture2D LoadMask(string name)
        {
            string path = $"{ReferenceFolder}/{name}.png";
            if (!File.Exists(path)) return null;
            if (AssetImporter.GetAtPath(path) is TextureImporter importer &&
                (importer.textureType != TextureImporterType.Default || importer.sRGBTexture || importer.wrapMode != TextureWrapMode.Repeat))
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = false;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.mipmapEnabled = true;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <param name="configure">Đặt tham số lần đầu tạo material; đã có thì giữ giá trị người chỉnh tay.</param>
        private static Material LoadOrCreateGlassMaterial(Texture2D cells, Texture2D distortion, string name,
            Action<Material> configure)
        {
            Shader shader = Shader.Find("Puzzle/UI/StainedGlass");
            if (shader == null || cells == null) return null;

            string path = $"{ReferenceFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                configure?.Invoke(material);
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetTexture("_DistortTex", distortion);
            EditorUtility.SetDirty(material);
            return material;
        }

        // Khối ngọc: mảnh pha lê sáng hơn, vân nứt nhẹ, vát sáng trên-trái / tối dưới-phải, viền tối mảnh, màu rực.
        private static void ConfigureGem(Material material)
        {
            material.SetFloat("_ShardMin", 0.82f);
            material.SetFloat("_ShardMax", 1.12f);
            material.SetFloat("_CrackDarkness", 0.18f);
            material.SetFloat("_EdgeGlow", 0f);
            material.SetFloat("_BevelWidth", 0.36f);
            material.SetFloat("_BevelLight", 0.8f);
            material.SetFloat("_BevelDark", 0.55f);
            material.SetFloat("_RimWidth", 0.06f);
            material.SetFloat("_RimDark", 0.75f);
            material.SetFloat("_Saturation", 1.25f);
        }

        /// <summary>Sprite tham khảo đặt tay trong Art/Glimmith; trả null nếu file chưa có.</summary>
        private static Sprite LoadReference(string name, Vector4 border, TextureWrapMode wrap = TextureWrapMode.Clamp)
        {
            string path = $"{ReferenceFolder}/{name}.png";
            if (!File.Exists(path)) return null;
            if (AssetImporter.GetAtPath(path) is TextureImporter importer &&
                (importer.textureType != TextureImporterType.Sprite || importer.wrapMode != wrap))
            {
                ConfigureImporter(path, border, wrap);
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite LoadOrCreate(string name, int width, Func<int, int, int, int, Color> pixel,
            Vector4 border, TextureWrapMode wrap, int height = 0)
        {
            height = height > 0 ? height : width;
            string path = $"{Folder}/{name}.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++) texture.SetPixel(x, y, pixel(x, y, width, height));
                }
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
                ConfigureImporter(path, border, wrap);
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void ConfigureImporter(string path, Vector4 border, TextureWrapMode wrap)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = border;
            importer.wrapMode = wrap;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        // Giấy da: màu kem, vân nhiễu nhẹ, tối dần về mép.
        private static Color ParchmentPixel(int x, int y, int w, int h)
        {
            float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.6f + Mathf.PerlinNoise(x * 0.2f, y * 0.2f) * 0.4f;
            float edge = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y)) / (w * 0.15f);
            float shade = Mathf.Lerp(0.82f, 1f, Mathf.Clamp01(edge)) * Mathf.Lerp(0.93f, 1.02f, noise);
            return new Color(0.96f * shade, 0.88f * shade, 0.74f * shade, 1f);
        }

        // Kính màu: ô Voronoi sáng tối khác nhau với đường viền mờ; tô màu bằng Image.color (nhân màu).
        private static Color GlassPixel(int x, int y, int w, int h)
        {
            const int Seeds = 14;
            float nearest = float.MaxValue, second = float.MaxValue;
            int nearestSeed = 0;
            for (int i = 0; i < Seeds; i++)
            {
                var seed = new Vector2(Hash(i * 2) * w, Hash(i * 2 + 1) * h);
                float distance = Vector2.Distance(new Vector2(x, y), seed);
                if (distance < nearest) { second = nearest; nearest = distance; nearestSeed = i; }
                else if (distance < second) second = distance;
            }
            float shade = Mathf.Lerp(0.78f, 1f, Hash(nearestSeed + 100));
            if (second - nearest < 2.5f) shade *= 0.82f;
            return new Color(shade, shade, shade, 1f);
        }

        // Nét chấm: nửa đầu đặc, nửa sau trong suốt; lặp theo chiều ngang khi Image để kiểu Tiled.
        private static Color DashPixel(int x, int y, int w, int h) => x < w * 0.55f ? Color.white : Color.clear;

        // Khung bo góc trắng có viền tối; 9-slice nên co giãn mọi cỡ mà góc không méo.
        private static Color PanelPixel(int x, int y, int w, int h)
        {
            const float Radius = 14f, Outline = 4f;
            float dx = Mathf.Max(0f, Mathf.Max(Radius - x, x - (w - 1 - Radius)));
            float dy = Mathf.Max(0f, Mathf.Max(Radius - y, y - (h - 1 - Radius)));
            float outside = Mathf.Sqrt(dx * dx + dy * dy) - Radius;
            if (outside > 0.5f) return Color.clear;
            bool rim = outside > -Outline || x < Outline || y < Outline || x > w - 1 - Outline || y > h - 1 - Outline;
            return rim ? new Color(0.25f, 0.22f, 0.2f, 1f) : Color.white;
        }

        // Khung rỗng (chỉ viền) để đánh dấu ô màu đang chọn mà không che màu bên dưới.
        private static Color FramePixel(int x, int y, int w, int h)
        {
            const int Thickness = 7;
            bool rim = x < Thickness || y < Thickness || x >= w - Thickness || y >= h - Thickness;
            return rim ? Color.white : Color.clear;
        }

        // Nền đá: khối xám lệch hàng như tường gạch, nhiễu nhẹ.
        private static Color StonePixel(int x, int y, int w, int h)
        {
            int row = y / 64;
            int offset = row % 2 == 0 ? 0 : 48;
            bool mortar = y % 64 < 3 || (x + offset) % 96 < 3;
            float tone = Mathf.Lerp(0.42f, 0.52f, Hash(row * 31 + (x + offset) / 96)) *
                         Mathf.Lerp(0.92f, 1.05f, Mathf.PerlinNoise(x * 0.08f, y * 0.08f));
            if (mortar) tone *= 0.65f;
            return new Color(tone, tone * 1.02f, tone * 1.06f, 1f);
        }

        private static float Hash(int n)
        {
            float value = Mathf.Sin(n * 127.1f + 311.7f) * 43758.5453f;
            return value - Mathf.Floor(value);
        }
    }
}
