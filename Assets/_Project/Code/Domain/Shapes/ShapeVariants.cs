using System;
using System.Collections.Generic;

namespace Puzzle.Domain.Shapes
{
    /// <summary>
    /// Mọi biến thể xoay/lật khác nhau của một hình (tối đa 8, ít hơn nếu hình đối xứng)
    /// cùng dạng chính tắc của chúng. Hai hình "giống nhau khi cho xoay/lật"
    /// khi và chỉ khi có cùng <see cref="Canonical"/>, nên luật chỉ cần so một giá trị.
    /// Tính một lần rồi giữ lại; đừng tạo mới trong vòng lặp nóng nếu tránh được.
    /// </summary>
    public sealed class ShapeVariants
    {
        private readonly Polyomino[] _variants;

        public Polyomino Source { get; }

        /// <summary>Các biến thể phân biệt, đã sắp xếp; phần tử đầu là <see cref="Canonical"/>.</summary>
        public IReadOnlyList<Polyomino> All => _variants;

        /// <summary>Biến thể nhỏ nhất theo <see cref="Polyomino.CompareTo"/>: đại diện cho cả lớp hình.</summary>
        public Polyomino Canonical => _variants[0];

        public ShapeVariants(Polyomino source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            _variants = Enumerate(source);
        }

        /// <summary>Hình khác có trùng một biến thể của hình này không.</summary>
        public bool Matches(Polyomino other)
        {
            return other != null && Array.IndexOf(_variants, other) >= 0;
        }

        // 4 góc xoay của hình gốc + 4 góc xoay của hình lật = 8 phép đối xứng của hình vuông.
        // HashSet bỏ bản trùng khi hình tự đối xứng (ví dụ ô vuông 2x2 chỉ còn 1 biến thể).
        private static Polyomino[] Enumerate(Polyomino source)
        {
            var unique = new HashSet<Polyomino>();
            Polyomino current = source;
            Polyomino mirrored = source.Mirror();
            for (int turn = 0; turn < 4; turn++)
            {
                unique.Add(current);
                unique.Add(mirrored);
                current = current.Rotate90();
                mirrored = mirrored.Rotate90();
            }

            var sorted = new List<Polyomino>(unique);
            sorted.Sort();
            return sorted.ToArray();
        }
    }
}
