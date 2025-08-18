using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace lofi.ViewModels
{
    public class MusicPlayerViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<string> Tracks { get; } = new();

        private string? _currentTrack;
        public string? CurrentTrack
        {
            get => _currentTrack;
            set { _currentTrack = value; OnPropertyChanged(); }
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(); OnPropertyChanged(nameof(PlayPauseLabel)); }
        }

        public string PlayPauseLabel => IsPlaying ? "⏸" : "▶";

        public ICommand PlayPauseCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PrevCommand { get; }

        public MusicPlayerViewModel()
        {
            // 예시: 초기 트랙 목록 (추후 파일 로딩 로직 연결)
            Tracks.Add("Track 1");
            Tracks.Add("Track 2");
            Tracks.Add("Track 3");

            //PlayPauseCommand = new RelayCommand(_ => TogglePlayPause());
            //NextCommand = new RelayCommand(_ => NextTrack());
            //PrevCommand = new RelayCommand(_ => PrevTrack());

            CurrentTrack = Tracks.Count > 0 ? Tracks[0] : null;
        }

        private void TogglePlayPause() => IsPlaying = !IsPlaying;
        private void NextTrack()
        {
            if (Tracks.Count == 0) return;
            var i = Tracks.IndexOf(CurrentTrack!);
            CurrentTrack = Tracks[(i + 1) % Tracks.Count];
        }
        private void PrevTrack()
        {
            if (Tracks.Count == 0) return;
            var i = Tracks.IndexOf(CurrentTrack!);
            CurrentTrack = Tracks[(i - 1 + Tracks.Count) % Tracks.Count];
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
