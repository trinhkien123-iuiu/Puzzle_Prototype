using System;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzle.Presentation.Config
{
    /// <summary>
    /// Cách giải thích từng luật cho người chơi, tra theo id luật (khoá trong RuleRegistry).
    /// Thêm luật mới = thêm một mục ở đây, không sửa code hiển thị.
    /// </summary>
    [CreateAssetMenu(menuName = "Puzzle/Config/Clue Visuals", fileName = "ClueVisuals")]
    public sealed class ClueVisuals : ScriptableObject
    {
        [Serializable]
        private sealed class Entry
        {
            [Tooltip("Id luật, ví dụ shape-bank.")] public string RuleId;
            [Tooltip("Tên ngắn hiện trên khung luật.")] public string Title;
            [TextArea(2, 4), Tooltip("Một câu giải thích luật.")] public string Description;
        }

        [SerializeField]
        private List<Entry> _entries = new List<Entry>
        {
            new Entry
            {
                RuleId = "shape-bank",
                Title = "Kho hình",
                Description = "Chia bàn thành các vùng; mỗi vùng phải đúng một hình dưới đây (được xoay, lật).",
            },
        };

        public bool TryGet(string ruleId, out string title, out string description)
        {
            foreach (Entry entry in _entries)
            {
                if (entry.RuleId != ruleId) continue;
                title = entry.Title;
                description = entry.Description;
                return true;
            }
            title = ruleId;
            description = string.Empty;
            return false;
        }
    }
}
