using System.Collections.Generic;
using Puzzle.Bootstrap;
using Puzzle.Presentation.Board;
using Puzzle.Presentation.Config;
using Puzzle.Presentation.Feedback;
using Puzzle.Presentation.Input;
using Puzzle.Presentation.Screens;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using L = Puzzle.Editor.Greybox.GreyboxLayout;

namespace Puzzle.Editor.Greybox
{
    /// <summary>
    /// Menu một lần: dựng scene Game chơi được (art tạm, config, catalog, prefab, scene, nối tham chiếu).
    /// Kết quả là asset bình thường, chỉnh tay được; chạy lại sẽ ghi đè scene Game và prefab greybox.
    /// </summary>
    internal static class GreyboxSceneBuilder
    {
        [MenuItem("Tools/Puzzle/Setup Greybox Scene")]
        private static void Setup()
        {
            bool overwrite = AssetDatabase.LoadAssetAtPath<SceneAsset>(GreyboxPaths.GameScene) == null ||
                EditorUtility.DisplayDialog("Setup Greybox Scene",
                    "Scene Game và prefab greybox sẽ bị ghi đè. LevelCatalog và art được giữ nguyên; có Art/BlockBlast thì " +
                    "các trường giao diện của PaletteConfig/BoardLayoutConfig được đặt theo bộ Block Blast.",
                    "Dựng lại", "Huỷ");
            if (!overwrite || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        /// <summary>Dựng scene không hỏi gì (cho công cụ tự động); scene đang mở chưa lưu sẽ bị bỏ.</summary>
        internal static void Build()
        {
            var ui = new GreyboxUiFactory(GreyboxArt.LoadOrCreate());
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Camera camera = CreateCamera();
            CreateEventSystem();
            RectTransform safeArea = CreateCanvasWithSafeArea(ui);
            CreateBootstrap(ui, camera, BuildScreen(ui, safeArea));

            GreyboxAssets.EnsureFolder(GreyboxPaths.Scenes);
            EditorSceneManager.SaveScene(scene, GreyboxPaths.GameScene);
            RegisterSceneAndPortrait();
            AssetDatabase.SaveAssets();
            Debug.Log("Greybox scene ready: " + GreyboxPaths.GameScene + ". Set Game view to a portrait size and press Play.");
        }

        // Thứ tự con trong SafeArea = thứ tự vẽ: bàn, kho hình, bảng màu, HUD, rồi thanh Hoàn thành trên cùng.
        private static SceneViews BuildScreen(GreyboxUiFactory ui, RectTransform safeArea)
        {
            var boardPrefabs = new GreyboxPrefabs(ui);
            var screenPrefabs = new GreyboxScreenPrefabs(ui);
            CellView cellPrefab = boardPrefabs.BuildCell();

            SceneViews views = CreateBoard(ui, boardPrefabs, cellPrefab, safeArea);
            // Hình trong kho hình vẽ bằng khối ngọc rời (sprite) khi có bộ Block Blast.
            CellView iconCell = ui.Blocks ? boardPrefabs.BuildBlockCell("ShapeIconCell") : cellPrefab;
            views.Legend = Instantiate(screenPrefabs.BuildRuleLegend(boardPrefabs.BuildShapeIcon(iconCell)), safeArea);
            ui.Band((RectTransform)views.Legend.transform, true, L.LegendTop, L.LegendHeight, L.Margin);
            views.Palette = Instantiate(screenPrefabs.BuildPalette(), safeArea);
            ui.Band((RectTransform)views.Palette.transform, false, L.PaletteBottom, L.PaletteHeight, L.Margin);
            views.Hud = Instantiate(screenPrefabs.BuildHud(), safeArea);
            views.Popup = Instantiate(screenPrefabs.BuildSolvedBar(), safeArea);
            ui.Band((RectTransform)views.Popup.transform, false, L.PaletteBottom, L.PaletteHeight, L.Margin);
            return views;
        }

        private static T Instantiate<T>(T prefab, Transform parent) where T : Component
        {
            return (T)PrefabUtility.InstantiatePrefab(prefab, parent);
        }

        private static Camera CreateCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            var camera = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>(); // âm thanh đặt khối / giải xong cần một listener trong scene
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            go.transform.position = new Vector3(0f, 0f, -10f);
            return camera;
        }

        // Dùng StandaloneInputModule (Input Manager cũ) vì project chưa cài Input System; nó nhận cả chạm lẫn chuột.
        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static RectTransform CreateCanvasWithSafeArea(GreyboxUiFactory ui)
        {
            RectTransform canvasRect = ui.CreateRect("Canvas", null);
            var canvas = canvasRect.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Shader kính đọc uv1/uv2 (toạ độ trong ô, mặt nạ mép vùng).
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;

            // Khớp theo chiều rộng: máy dọc dài hơn (20:9) chỉ thêm chỗ theo chiều cao cho bàn.
            var scaler = canvasRect.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(L.ReferenceWidth, L.ReferenceHeight);
            scaler.matchWidthOrHeight = 0f;
            canvasRect.gameObject.AddComponent<GraphicRaycaster>();

            // Nền phủ cả màn (kể cả vùng tai thỏ), nằm ngoài SafeArea.
            if (ui.Blocks)
            {
                // Ảnh nền ngang của Block Blast: phủ kín màn dọc, giữ tỉ lệ (cắt bớt hai bên) thay vì kéo méo.
                Image background = ui.AddImage(ui.CreateRect("Background", canvasRect), Color.white, false, ui.Art.Blocks.Background);
                var fitter = background.gameObject.AddComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = background.sprite.rect.width / background.sprite.rect.height;
            }
            else
            {
                Image background = ui.AddImage(ui.Stretch(ui.CreateRect("Background", canvasRect)), new Color(0.9f, 0.88f, 0.86f), false, ui.Art.Stone);
                background.type = Image.Type.Tiled; // gạch lặp theo kích thước gốc, không bị kéo méo theo màn
            }

            RectTransform safeArea = ui.Stretch(ui.CreateRect("SafeArea", canvasRect));
            safeArea.gameObject.AddComponent<SafeAreaFitter>();
            return safeArea;
        }

        private static SceneViews CreateBoard(GreyboxUiFactory ui, GreyboxPrefabs prefabs, CellView cellPrefab,
            RectTransform safeArea)
        {
            RectTransform area = ui.Stretch(ui.CreateRect("BoardArea", safeArea), L.Margin, L.BoardBottom, L.Margin, L.BoardTop);
            // Tấm nền bàn (giấy da, hoặc khung xanh đậm Block Blast), cũng là vùng nhận chạm của bàn.
            ui.AddImage(area, Color.white, true, ui.Blocks ? ui.Art.Blocks.BoardPanel : ui.Art.Parchment);
            // Bàn chừa lề trong tấm nền; cạnh nằm lớp trên ô để nét ranh giới không bị ô che.
            float inset = ui.Blocks ? 36f : 24f;
            RectTransform board = ui.Stretch(ui.CreateRect("Board", area), inset, inset, inset, inset);
            RectTransform outlines = ui.Stretch(ui.CreateRect("OutlineLayer", board));
            RectTransform cells = ui.Stretch(ui.CreateRect("CellLayer", board));
            RectTransform edges = ui.Stretch(ui.CreateRect("EdgeLayer", board));

            var views = new SceneViews
            {
                Board = board.gameObject.AddComponent<BoardView>(),
                Input = area.gameObject.AddComponent<BoardInput>(),
                Celebration = area.gameObject.AddComponent<CelebrationView>(),
            };
            GreyboxUiFactory.Assign(views.Board, "_cellLayer", cells);
            GreyboxUiFactory.Assign(views.Board, "_edgeLayer", edges);
            GreyboxUiFactory.Assign(views.Board, "_cellPrefab", cellPrefab);
            if (!ui.Blocks)
            {
                // Viền đậm quanh mép bàn chỉ có ở kiểu kính; khối Block Blast đứng trên khung riêng.
                GreyboxUiFactory.Assign(views.Board, "_outlineLayer", outlines);
                GreyboxUiFactory.Assign(views.Board, "_outlinePrefab", prefabs.BuildCellOutline());
            }
            GreyboxUiFactory.Assign(views.Board, "_edgePrefab", prefabs.BuildEdge());
            GreyboxUiFactory.Assign(views.Celebration, "_target", area);
            if (ui.Blocks) AddPraiseAndSound(ui, views.Celebration, area);
            return views;
        }

        // Chữ khen (đang tắt) + ánh sáng loé giữa bàn, và nguồn âm thanh cho CelebrationView.
        private static void AddPraiseAndSound(GreyboxUiFactory ui, CelebrationView celebration, RectTransform area)
        {
            GreyboxArt.BlockBlastArt blocks = ui.Art.Blocks;
            RectTransform layer = ui.Stretch(ui.CreateRect("Praise", area));

            RectTransform flash = ui.CreateRect("Flash", layer);
            flash.sizeDelta = new Vector2(620f, 620f);
            ui.AddImage(flash, new Color(1f, 0.95f, 0.7f), false, blocks.Sparkle);
            flash.gameObject.SetActive(false);

            var praises = new List<Object>();
            foreach (Sprite sprite in blocks.Praises)
            {
                if (sprite == null) continue;
                RectTransform praise = ui.CreateRect(sprite.name, layer);
                praise.sizeDelta = sprite.rect.size * 1.3f;
                ui.AddImage(praise, Color.white, false, sprite);
                praise.gameObject.AddComponent<CanvasGroup>();
                praise.gameObject.SetActive(false);
                praises.Add(praise);
            }

            var audio = celebration.gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            GreyboxUiFactory.AssignList(celebration, "_praises", praises.ToArray());
            GreyboxUiFactory.Assign(celebration, "_flash", flash);
            GreyboxUiFactory.Assign(celebration, "_audio", audio);
            GreyboxUiFactory.Assign(celebration, "_changeClip", blocks.ChangeClip);
            GreyboxUiFactory.Assign(celebration, "_solvedClip", blocks.SolvedClip);
        }

        // Có bộ Block Blast thì ghi đè các trường giao diện của config (sprite khối, màu tô theo khối, nét giữa vùng);
        // các trường khác người chỉnh tay vẫn giữ nguyên.
        private static PaletteConfig ApplyBlockPalette(GreyboxUiFactory ui, PaletteConfig palette)
        {
            if (!ui.Blocks) return palette;
            GreyboxArt.BlockBlastArt blocks = ui.Art.Blocks;
            var serialized = new SerializedObject(palette);
            SetColors(serialized.FindProperty("_paints"), blocks.GemColors);
            SetSprites(serialized.FindProperty("_paintSprites"), blocks.Gems);
            serialized.FindProperty("_emptySprite").objectReferenceValue = blocks.CellEmpty;
            serialized.FindProperty("_previewSprite").objectReferenceValue = blocks.CellPreview;
            // Có shader ngọc thì bàn vẽ bằng màu (mỗi vùng là một viên ngọc liền); bảng màu và kho hình vẫn dùng sprite.
            bool gemShader = ui.Art.GemGlass != null;
            serialized.FindProperty("_boardUsesSprites").boolValue = !gemShader;
            serialized.FindProperty("_unpaintedCell").colorValue = gemShader ? new Color(0.17f, 0.24f, 0.45f) : Color.white;
            serialized.FindProperty("_dragPreview").colorValue = new Color(0.45f, 0.62f, 1f);
            serialized.FindProperty("_dots").colorValue = Color.clear; // ô trống tự có viền riêng, không cần nét chấm
            serialized.FindProperty("_border").colorValue = gemShader ? new Color(0.04f, 0.06f, 0.16f) : new Color(1f, 1f, 1f, 0.9f);
            serialized.FindProperty("_givenBorder").colorValue = new Color(1f, 0.82f, 0.25f);
            serialized.FindProperty("_highlight").colorValue = new Color(1f, 1f, 0.55f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(palette);
            return palette;
        }

        private static BoardLayoutConfig ApplyBlockLayout(GreyboxUiFactory ui, BoardLayoutConfig layout)
        {
            if (!ui.Blocks) return layout;
            var serialized = new SerializedObject(layout);
            // Shader ngọc: ô sát nhau để vùng liền một viên, nét mảnh giữa hai vùng. Sprite rời: khe như lưới Block Blast.
            bool gemShader = ui.Art.GemGlass != null;
            serialized.FindProperty("_cellGap").floatValue = gemShader ? 0f : 8f;
            serialized.FindProperty("_borderThickness").floatValue = gemShader ? 4f : 8f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(layout);
            return layout;
        }

        private static void SetColors(SerializedProperty list, Color[] colors)
        {
            list.arraySize = colors.Length;
            for (int i = 0; i < colors.Length; i++) list.GetArrayElementAtIndex(i).colorValue = colors[i];
        }

        private static void SetSprites(SerializedProperty list, Sprite[] sprites)
        {
            list.arraySize = sprites.Length;
            for (int i = 0; i < sprites.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        private static void CreateBootstrap(GreyboxUiFactory ui, Camera camera, SceneViews views)
        {
            var bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
            var references = new Dictionary<string, Object>
            {
                ["_catalog"] = GreyboxAssets.LoadOrCreateCatalog(),
                ["_layoutConfig"] = ApplyBlockLayout(ui, GreyboxAssets.LoadOrCreate<BoardLayoutConfig>(GreyboxPaths.Config, "BoardLayoutConfig")),
                ["_palette"] = ApplyBlockPalette(ui, GreyboxAssets.LoadOrCreate<PaletteConfig>(GreyboxPaths.Config, "PaletteConfig")),
                ["_inputConfig"] = GreyboxAssets.LoadOrCreate<InputConfig>(GreyboxPaths.Config, "InputConfig"),
                ["_feedbackConfig"] = GreyboxAssets.LoadOrCreate<FeedbackConfig>(GreyboxPaths.Config, "FeedbackConfig"),
                ["_clueVisuals"] = GreyboxAssets.LoadOrCreate<ClueVisuals>(GreyboxPaths.Config, "ClueVisuals"),
                ["_camera"] = camera,
                ["_board"] = views.Board,
                ["_boardInput"] = views.Input,
                ["_hud"] = views.Hud,
                ["_popup"] = views.Popup,
                ["_celebration"] = views.Celebration,
                ["_legend"] = views.Legend,
                ["_paletteView"] = views.Palette,
            };
            foreach (KeyValuePair<string, Object> reference in references)
            {
                GreyboxUiFactory.Assign(bootstrap, reference.Key, reference.Value);
            }
        }

        // Scene Game đứng đầu Build Settings; khoá màn dọc theo ARCHITECTURE.md mục 14.
        private static void RegisterSceneAndPortrait()
        {
            var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(GreyboxPaths.GameScene, true) };
            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (existing.path != GreyboxPaths.GameScene) scenes.Add(existing);
            }
            EditorBuildSettings.scenes = scenes.ToArray();
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        }

        private sealed class SceneViews
        {
            public BoardView Board;
            public BoardInput Input;
            public CelebrationView Celebration;
            public RuleLegendView Legend;
            public PaletteView Palette;
            public HudView Hud;
            public SolvedPopupView Popup;
        }
    }
}
