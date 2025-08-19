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

        public void Open(Uri source) => _player.Open(source);
        public void Play() => _player.Play();
        public void Pause() => _player.Pause();
        public void Stop() => _player.Stop();

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
