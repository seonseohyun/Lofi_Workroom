using System;
using System.Windows.Media;

namespace lofi.Services
{
    public class MusicService : IDisposable
    {
        private readonly MediaPlayer _player = new();

        public event EventHandler? MediaOpened;
        public event EventHandler? MediaEnded;

        public MusicService()
        {
            _player.MediaOpened += (s, e) => MediaOpened?.Invoke(this, EventArgs.Empty);
            _player.MediaEnded += (s, e) => MediaEnded?.Invoke(this, EventArgs.Empty);
            _player.Volume = 1.0;
        }

        public void Open(Uri uri)
        {
            _player.Stop();   // 현재 재생 중지
            _player.Close();  // 현재 소스 언로드
            _player.Open(uri);
        }
        public void Play() => _player.Play();
        public void Pause() => _player.Pause();
        public void Stop()
        {
            _player.Stop();
            _player.Close(); // 이 줄이 중요: Close로 소스를 해제해야 '겹침'이 사라집니다.
        }

        public bool HasDuration => _player.NaturalDuration.HasTimeSpan;
        public TimeSpan Duration => _player.NaturalDuration.HasTimeSpan
            ? _player.NaturalDuration.TimeSpan
            : TimeSpan.Zero;

        public TimeSpan Position
        {
            get => _player.Position;
            set => _player.Position = value;
        }

        public double Volume
        {
            get => _player.Volume;
            set => _player.Volume = Math.Clamp(value, 0.0, 1.0);
        }

        public void Dispose() => _player.Close();
    }
}
