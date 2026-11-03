namespace Puzzle.Editor.Greybox
{
    /// <summary>Đường dẫn nơi menu greybox ghi asset. Chỉ Editor dùng; runtime không tra asset theo chuỗi.</summary>
    internal static class GreyboxPaths
    {
        public const string Root = "Assets/_Project";
        public const string Config = Root + "/Config";
        public const string Levels = Root + "/Levels";
        public const string BoardPrefabs = Root + "/Prefabs/Board";
        public const string ScreenPrefabs = Root + "/Prefabs/Screens";
        public const string Scenes = Root + "/Scenes";
        public const string GameScene = Scenes + "/Game.unity";
    }
}
