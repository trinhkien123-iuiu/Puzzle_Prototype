using System.Collections.Generic;
using System.IO;
using Puzzle.Application.Levels;
using UnityEditor;
using UnityEngine;

namespace Puzzle.Editor.Greybox
{
    /// <summary>
    /// Tạo (hoặc lấy lại nếu đã có) các asset cấu hình và LevelCatalog cho greybox.
    /// Asset đã tồn tại thì giữ nguyên để không mất chỉnh sửa tay trong Inspector.
    /// </summary>
    internal static class GreyboxAssets
    {
        public static T LoadOrCreate<T>(string folder, string fileName) where T : ScriptableObject
        {
            EnsureFolder(folder);
            string path = $"{folder}/{fileName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            var created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }

        /// <summary>Catalog chứa mọi level trong thư mục Levels (theo tên file) nếu catalog đang trống.</summary>
        public static LevelCatalog LoadOrCreateCatalog()
        {
            var catalog = LoadOrCreate<LevelCatalog>(GreyboxPaths.Config, "LevelCatalog");
            if (catalog.Count > 0) return catalog;

            var paths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:LevelAsset", new[] { GreyboxPaths.Levels }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            paths.Sort(System.StringComparer.Ordinal);

            var levels = new List<LevelAsset>();
            foreach (string path in paths) levels.Add(AssetDatabase.LoadAssetAtPath<LevelAsset>(path));
            catalog.ReplaceLevels(levels);
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        /// <summary>Tạo thư mục lồng nhau trong Assets nếu chưa có.</summary>
        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
