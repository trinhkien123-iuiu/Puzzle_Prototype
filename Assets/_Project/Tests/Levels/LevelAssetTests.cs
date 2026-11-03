#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using Puzzle.Application.Levels;
using Puzzle.Domain.Puzzle;
using Puzzle.Domain.Regions;
using Puzzle.Domain.Rules;
using UnityEditor;

namespace Puzzle.Tests.Levels
{
    /// <summary>
    /// Duyệt MỌI level trong project (ARCHITECTURE.md mục 8): đọc được, id không trùng, lời giải thoả mọi luật.
    /// Level hỏng thì test đỏ, không đợi người chơi phát hiện.
    /// </summary>
    public class LevelAssetTests
    {
        private static IEnumerable<string> LevelPaths()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:LevelAsset"))
            {
                yield return AssetDatabase.GUIDToAssetPath(guid);
            }
        }

        [Test]
        public void ProjectHasLevels()
        {
            CollectionAssert.IsNotEmpty(LevelPaths());
        }

        [TestCaseSource(nameof(LevelPaths))]
        public void Level_SolutionSatisfiesEveryRule(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<LevelAsset>(path);
            PuzzleDefinition puzzle = new LevelLoader(RuleRegistry.CreateDefault()).Parse(asset.Source, asset.Id).Puzzle;
            PuzzleState solved = puzzle.CreateSolutionState();
            var violations = new List<RuleViolation>();

            bool ok = new PuzzleValidator(puzzle.Rules).Validate(new RuleContext(solved, new RegionMap(solved)), violations);

            Assert.IsTrue(ok, $"{path}: {string.Join(", ", violations)}");
        }

        [Test]
        public void LevelIds_AreUnique()
        {
            var seen = new HashSet<string>();
            foreach (string path in LevelPaths())
            {
                string id = AssetDatabase.LoadAssetAtPath<LevelAsset>(path).Id;
                Assert.IsTrue(seen.Add(id), $"Duplicate level id '{id}' in {path}");
            }
        }
    }
}
#endif
