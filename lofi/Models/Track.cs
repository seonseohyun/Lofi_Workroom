using System;

namespace lofi.Models
{
    public class Track
    {
        public string Title { get; set; } = string.Empty;     // UI 표시 제목
        public string AudioPackUri { get; set; } = string.Empty; // pack://application:,,,/Assets/Music/xxx.mp3
        public string? CoverPackUri { get; set; }             // (옵션) 커버 이미지 pack URI
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
    }
}
