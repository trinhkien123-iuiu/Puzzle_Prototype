namespace Puzzle.Application.Progress
{
    /// <summary>
    /// Nơi lưu tiến độ. Cài đặt thật (PlayerPrefs, file) nằm ở Presentation; test dùng bản trong bộ nhớ.
    /// Hợp đồng: <see cref="Save"/> rồi <see cref="Load"/> trả về dữ liệu tương đương.
    /// </summary>
    public interface IProgressStore
    {
        /// <summary>Đọc tiến độ; chưa có gì thì trả tiến độ trống, không trả null.</summary>
        PlayerProgress Load();

        /// <summary>Ghi ngay (gọi khi app vào nền), không đợi lần sau.</summary>
        void Save(PlayerProgress progress);
    }
}
