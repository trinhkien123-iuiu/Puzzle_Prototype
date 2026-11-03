using System;
using System.IO;
using Puzzle.Application.Levels;
using Puzzle.Domain.Rules;
using UnityEditor.AssetImporters;

namespace Puzzle.Editor.Levels
{
    /// <summary>
    /// Nhập file level <c>.pzl</c> thành <see cref="LevelAsset"/>. Đọc thử ngay khi nhập để file sai định dạng
    /// báo lỗi đỏ trong Console lúc lưu file, không đợi tới lúc chơi.
    /// </summary>
    [ScriptedImporter(1, "pzl")]
    public sealed class LevelFileImporter : ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            string text = File.ReadAllText(ctx.assetPath);
            string fallbackId = Path.GetFileNameWithoutExtension(ctx.assetPath);
            var asset = UnityEngine.ScriptableObject.CreateInstance<LevelAsset>();

            try
            {
                LevelDocument document = new LevelLoader(RuleRegistry.CreateDefault()).Parse(text, fallbackId);
                asset.Configure(document.Id, document.Difficulty, text);
            }
            catch (Exception error) when (error is FormatException || error is ArgumentException)
            {
                ctx.LogImportError($"{ctx.assetPath}: {error.Message}");
                asset.Configure(fallbackId, 1, text);
            }

            ctx.AddObjectToAsset("level", asset);
            ctx.SetMainObject(asset);
        }
    }
}
