using System;
using System.Collections.Generic;
using System.Globalization;

namespace Puzzle.Application.Levels
{
    /// <summary>
    /// Đọc từng dòng của file level: dòng từ khoá ("shape L") và các dòng lưới đi sau nó.
    /// Tách khỏi <see cref="LevelLoader"/> để loader chỉ lo ý nghĩa, còn lớp này lo cú pháp.
    /// </summary>
    internal sealed class LevelTextReader
    {
        private readonly string[] _lines;
        private int _next;

        /// <summary>Số dòng (đếm từ 1) của dòng vừa đọc, để báo lỗi chỉ đúng chỗ.</summary>
        public int LineNumber => _next;

        /// <summary>Phần chữ sau từ khoá trên dòng vừa đọc, giữ nguyên khoảng trắng (cho câu hướng dẫn).</summary>
        public string RestOfLine { get; private set; } = string.Empty;

        public LevelTextReader(string text)
        {
            _lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        public bool TryNextKeyword(out string keyword, out string[] args)
        {
            while (_next < _lines.Length)
            {
                string line = Clean(_lines[_next++]);
                if (line.Length == 0) continue;
                if (IsGridLine(line)) throw Error("Grid line without a 'shape' or 'solution' keyword above it.");

                string[] words = line.Split(new[] { ' ', '\t', ':' }, StringSplitOptions.RemoveEmptyEntries);
                keyword = words[0].ToLowerInvariant();
                RestOfLine = line.Substring(words[0].Length).TrimStart(' ', '\t', ':').Trim();
                args = new string[words.Length - 1];
                Array.Copy(words, 1, args, 0, args.Length);
                return true;
            }
            keyword = null;
            args = null;
            return false;
        }

        /// <summary>Đọc các dòng lưới liền nhau ngay sau dòng từ khoá.</summary>
        public List<string> ReadGrid()
        {
            var rows = new List<string>();
            while (_next < _lines.Length)
            {
                string line = Clean(_lines[_next]);
                if (line.Length == 0 || !IsGridLine(line)) break;
                rows.Add(line);
                _next++;
            }
            if (rows.Count == 0) throw Error("Expected grid rows.");
            return rows;
        }

        public string RequireArg(string[] args, int index)
        {
            if (index >= args.Length) throw Error("Missing argument.");
            return args[index];
        }

        public int RequireInt(string[] args, int index)
        {
            string raw = RequireArg(args, index);
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                throw Error($"'{raw}' is not a number.");
            }
            return value;
        }

        public FormatException Error(string message) => new FormatException($"Line {LineNumber}: {message}");

        // Dòng lưới không có khoảng trắng bên trong; dòng từ khoá luôn có (hoặc là một từ chữ thường).
        private static bool IsGridLine(string line)
        {
            if (line.IndexOf(' ') >= 0 || line.IndexOf(':') >= 0) return false;
            return line.IndexOf('#') >= 0 || line.IndexOf('.') >= 0 || char.IsUpper(line[0]) || char.IsDigit(line[0]);
        }

        private static string Clean(string line)
        {
            int comment = line.IndexOf("//", StringComparison.Ordinal);
            if (comment >= 0) line = line.Substring(0, comment);
            return line.Trim();
        }
    }
}
