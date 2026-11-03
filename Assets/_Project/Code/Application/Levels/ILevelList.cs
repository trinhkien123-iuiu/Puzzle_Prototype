namespace Puzzle.Application.Levels
{
    /// <summary>
    /// Danh sách id level theo thứ tự chơi. Tách khỏi <see cref="LevelCatalog"/> (ScriptableObject)
    /// để logic chọn level test được không cần asset.
    /// </summary>
    public interface ILevelList
    {
        int Count { get; }
        string IdAt(int index);
        int IndexOf(string levelId);
    }
}
