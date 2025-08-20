using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using lofi.Models;
using lofi.Services;

namespace lofi.ViewModels
{
    public class MusicPlayerViewModel : ObservableObject
    {
        private readonly MusicService _music;
        private readonly DispatcherTimer _timer;

        public ObservableCollection<Track> Tracks { get; } = new();

        // === 바인딩 프로퍼티 ===
        private string _trackTitle = "[VM READY]";   // 실행하면 제목에 바로 떠야 정상 (나중에 지워도 됨)

        public string TrackTitle
        {
            get => _trackTitle;
            private set => SetProperty(ref _trackTitle, value);

        }

        private ImageSource? _coverImage;
        public ImageSource? CoverImage
        {
            get => _coverImage;
            private set => SetProperty(ref _coverImage, value);
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            private set => SetProperty(ref _isPlaying, value);
        }

        private double _durationSeconds;
        public double DurationSeconds
        {
            get => _durationSeconds;
            private set
            {
                if (SetProperty(ref _durationSeconds, value))
                    DurationText = FormatSeconds(value);
            }
        }

        private double _positionSeconds;
        public double PositionSeconds
        {
            get => _positionSeconds;
            set
            {
                if (SetProperty(ref _positionSeconds, value))
                {
                    // ★ 드래그 중일 때만 실제 플레이어에 쓰기
                    if (_isSeeking)
                        _music.Position = TimeSpan.FromSeconds(Math.Max(0, value));

                    PositionText = FormatSeconds(_positionSeconds);
                }
            }
        }
        public void BeginSeek() => _isSeeking = true;
        public void EndSeek()
        {
            _isSeeking = false;
            // 드래그 끝난 시점에 한 번만 위치 확정(선택 사항)
            _music.Position = TimeSpan.FromSeconds(Math.Max(0, _positionSeconds));
        }


        private string _durationText = "00:00";
        public string DurationText
        {
            get => _durationText;
            private set => SetProperty(ref _durationText, value);
        }

        private string _positionText = "00:00";
        public string PositionText
        {
            get => _positionText;
            private set => SetProperty(ref _positionText, value);
        }

        // 현재 인덱스(트랙 변경 트리거)
        private int _currentIndex = -1;
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (SetProperty(ref _currentIndex, value))
                {
                    if (value >= 0 && value < Tracks.Count)
                        OpenAndPlay(Tracks[value]); // ← 여기서 호출
                }
            }
        }

        // === 커맨드 ===
        public IRelayCommand PrevCommand { get; }
        public IRelayCommand PlayPauseCommand { get; }
        public IRelayCommand NextCommand { get; }

        private bool _isSeeking; // ← 드래그 중 여부

        // === 생성자 ===
        public MusicPlayerViewModel()
            : this(new MusicService()) { }

        public MusicPlayerViewModel(MusicService music)
        {
            _music = music;
            _music.MediaOpened += (s, e) =>
            {
                // 미디어 길이 확보
                DurationSeconds = _music.Duration.TotalSeconds;
                // 일부 코덱에서 MediaOpened 직후 Position이 0.0 보장 아님 → 안전하게 0으로
                PositionSeconds = 0;
            };
            _music.MediaEnded += (s, e) => Next();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _timer.Tick += (s, e) =>
            {
                // ★ 플레이어 → UI 로만 반영 (플레이어에 다시 쓰지 않음)
                var pos = _music.Position.TotalSeconds;
                if (!_isSeeking && Math.Abs(pos - _positionSeconds) > 0.05)
                {
                    _positionSeconds = pos;                 // setter 우회
                    OnPropertyChanged(nameof(PositionSeconds));
                    PositionText = FormatSeconds(pos);
                }

                if (_music.HasDuration)
                    DurationSeconds = _music.Duration.TotalSeconds;
            };

            _timer.Start();

            PrevCommand = new RelayCommand(Prev);
            PlayPauseCommand = new RelayCommand(PlayPause);
            NextCommand = new RelayCommand(Next);

            // 리소스에서 트랙 로드
            LoadFromContentFiles(new (string, string?)[] {
                ("lazy sunday chill lofi.mp3", "lazy sunday chill lofi.png"),
                ("lofi girl.mp3", "lofi girl.png"),
                ("lofi hiphop.mp3", "lofi hiphop.png"),
                ("rainy lofi.mp3", "rainy lofi.png"),
                ("spring vibes.mp3", "spring vibes.png"),
                ("stormy night coding.mp3", "stormy night coding.png"),
                ("warm breeze lofi.mp3", "warm breeze lofi.png")
            });

            if (Tracks.Any())
                CurrentIndex = 0; // 첫 곡 자동 재생
        }

        // === 핵심 메서드 ===
        private void OpenAndPlay(Track t)
        {
            try
            {
                _music.Open(new Uri(t.AudioPackUri, UriKind.RelativeOrAbsolute)); // 변경
                _music.Play();
                _timer.Start();
                IsPlaying = true;
                TrackTitle = t.Title;
                CoverImage = LoadBitmapOrNull(t.CoverPackUri);
            }
            catch
            {
                IsPlaying = false; // 실패 시 상태 정리
            }
        }

        private void PlayPause()
        {
            if (!Tracks.Any()) return;

            if (IsPlaying)
            {
                _music.Pause();
                IsPlaying = false;
            }
            else
            {
                _music.Play();
                IsPlaying = true;
            }
        }

        private void Next()
        {
            if (!Tracks.Any()) return;
            var next = (CurrentIndex + 1) % Tracks.Count;
            CurrentIndex = next;
        }

        private void Prev()
        {
            if (!Tracks.Any()) return;
            var prev = (CurrentIndex - 1 + Tracks.Count) % Tracks.Count;
            CurrentIndex = prev;
        }

        // === 유틸 ===
        // MusicPlayerViewModel.cs
        private void LoadFromContentFiles((string audio, string? cover)[] files)
        {
            string baseDir = AppContext.BaseDirectory;
            foreach (var (audio, cover) in files)
            {
                var title = System.IO.Path.GetFileNameWithoutExtension(audio).Replace('_', ' ');

                // 오디오
                string audioPath = System.IO.Path.Combine(baseDir, "Resources", "Music", audio);
                if (!System.IO.File.Exists(audioPath)) continue; // 없으면 건너뛰기
                string audioUri = new Uri(audioPath, UriKind.Absolute).AbsoluteUri;

                // 커버(있을 때만)
                string? coverUri = null;
                if (!string.IsNullOrWhiteSpace(cover))
                {
                    string coverPath = System.IO.Path.Combine(baseDir, "Resources", "Music", cover);
                    if (System.IO.File.Exists(coverPath))
                        coverUri = new Uri(coverPath, UriKind.Absolute).AbsoluteUri;
                }

                Tracks.Add(new Track { Title = title, AudioPackUri = audioUri, CoverPackUri = coverUri });
            }
        }

        private static string FormatSeconds(double seconds)
        {
            if (seconds < 0 || double.IsNaN(seconds) || double.IsInfinity(seconds))
                return "00:00";
            var ts = TimeSpan.FromSeconds(seconds);
            return $"{(int)ts.TotalMinutes:00}:{ts.Seconds:00}";
        }

        // MusicPlayerViewModel.cs
        private static ImageSource? LoadBitmapOrNull(string? uri)
        {
            if (string.IsNullOrWhiteSpace(uri)) return null;
            try
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(uri, UriKind.RelativeOrAbsolute); // file:/// 또는 pack://
                bi.CacheOption = BitmapCacheOption.OnLoad;                  // 즉시 로드
                bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bi.EndInit();
                bi.Freeze();
                return bi;
            }
            catch { return null; }
        }
    }
}