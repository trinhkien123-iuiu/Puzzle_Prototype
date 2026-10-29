using System;
using System.Text;
using Puzzle.Domain.Grid;
using Puzzle.Domain.Puzzle;

namespace Puzzle.Application.Progress
{
    /// <summary>
    /// Ván đang chơi dở, dạng lưu được (field public để JsonUtility và serializer khác đọc được).
    /// Mobile có thể bị kill khi ở nền nên ván dở phải khôi phục được; lịch sử undo thì không lưu.
    /// </summary>
    [Serializable]
    public sealed class SavedPuzzle
    {
        /// <summary>Id level trong LevelCatalog.</summary>
        public string LevelId;

        /// <summary>Mỗi ký tự là một cạnh theo chỉ số của BoardShape: '0' Unknown, '1' Border, '2' Joined.</summary>
        public string Edges;

        public bool IsEmpty => string.IsNullOrEmpty(LevelId);

        public static SavedPuzzle Capture(string levelId, PuzzleState state)
        {
            if (string.IsNullOrEmpty(levelId)) throw new ArgumentException("Level id is required.", nameof(levelId));
            if (state == null) throw new ArgumentNullException(nameof(state));

            var edges = new StringBuilder(state.Board.Edges.Count);
            for (int i = 0; i < state.Board.Edges.Count; i++)
            {
                edges.Append((char)('0' + (int)state.GetEdge(i)));
            }
            return new SavedPuzzle { LevelId = levelId, Edges = edges.ToString() };
        }

        /// <summary>
        /// Dựng lại state cho level này. Trả false khi bản lưu không khớp (level đã đổi sau khi cập nhật game
        /// hoặc dữ liệu hỏng): người gọi bắt đầu ván mới thay vì để game crash.
        /// </summary>
        public bool TryRestore(PuzzleDefinition definition, out PuzzleState state)
        {
            state = null;
            if (definition == null || Edges == null || Edges.Length != definition.Board.Edges.Count) return false;

            var restored = new PuzzleState(definition.Board);
            for (int i = 0; i < Edges.Length; i++)
            {
                int value = Edges[i] - '0';
                if (value < 0 || value > (int)EdgeState.Joined) return false;
                restored.SetEdge(definition.Board.Edges[i], (EdgeState)value);
            }

            // Ranh giới cho sẵn luôn thắng bản lưu, kể cả khi file lưu bị sửa tay.
            foreach (Edge given in definition.GivenBorders) restored.SetEdge(given, EdgeState.Border);
            state = restored;
            return true;
        }
    }
}
