using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Threading;

namespace lofi.ViewModels
{
    public partial class PomodoroViewModel : ObservableObject
    {
        private readonly DispatcherTimer _timer;

        // === 설정 값 ===
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SessionTotal))]
        [NotifyPropertyChangedFor(nameof(Progress))]
        private int workSeconds = 25 * 60;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SessionTotal))]
        [NotifyPropertyChangedFor(nameof(Progress))]
        private int shortBreakSeconds = 5 * 60;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SessionTotal))]
        [NotifyPropertyChangedFor(nameof(Progress))]
        private int longBreakSeconds = 15 * 60;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RoundLabel))]
        [NotifyPropertyChangedFor(nameof(IsLongBreak))]
        [NotifyPropertyChangedFor(nameof(SessionTotal))]
        [NotifyPropertyChangedFor(nameof(Progress))]
        private int roundsPerLongBreak = 4;

        [ObservableProperty] private int ringThickness = 14;

        // 다음 세션 자동 시작 옵션
        [ObservableProperty] private bool autoStartNext = true;

        // === 런타임 상태 ===
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PlayPauseIcon))]
        private bool isRunning;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentLabel))]
        [NotifyPropertyChangedFor(nameof(IsLongBreak))]
        [NotifyPropertyChangedFor(nameof(SessionTotal))]
        [NotifyPropertyChangedFor(nameof(Progress))]
        private bool isWork = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RoundLabel))]
        [NotifyPropertyChangedFor(nameof(IsLongBreak))]
        [NotifyPropertyChangedFor(nameof(SessionTotal))]
        [NotifyPropertyChangedFor(nameof(Progress))]
        private int currentRound = 1; // 1..roundsPerLongBreak

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(TimeText))]
        [NotifyPropertyChangedFor(nameof(Progress))]
        private int remaining; // 현재 세션 남은 초

        // === 계산 프로퍼티 ===
        public double Progress => SessionTotal <= 0 ? 0 : 1.0 - (double)Remaining / SessionTotal;
        public string TimeText => TimeSpan.FromSeconds(Math.Max(0, Remaining)).ToString(@"mm\:ss");
        public string CurrentLabel => isWork ? "Focus" : (IsLongBreak ? "Long Break" : "Break");
        public string RoundLabel => isWork ? $"Round {CurrentRound}/{RoundsPerLongBreak}" : "";
        public string PlayPauseIcon => IsRunning ? "⏸" : "▶";
        public int SessionTotal => isWork ? WorkSeconds : (IsLongBreak ? LongBreakSeconds : ShortBreakSeconds);
        public bool IsLongBreak => !isWork && CurrentRound == RoundsPerLongBreak;

        public PomodoroViewModel()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (_, __) => Tick();
            Remaining = SessionTotal;
        }

        // === 커맨드 ===
        [RelayCommand]
        private void Toggle()
        {
            if (IsRunning) { IsRunning = false; _timer.Stop(); }
            else { IsRunning = true; _timer.Start(); }
        }

        [RelayCommand]
        private void Reset()
        {
            IsRunning = false; _timer.Stop();
            IsWork = true;
            CurrentRound = 1;
            Remaining = SessionTotal;
        }

        [RelayCommand]
        private void Skip() => EndSessionAndGoNext();

        [RelayCommand]
        private void OpenSettings()
        {
            // 설정 패널 열기(별도 구현)
        }

        // === 내부 로직 ===
        private void Tick()
        {
            if (Remaining > 0)
            {
                Remaining--;
                return;
            }
            EndSessionAndGoNext();
        }

        private void EndSessionAndGoNext()
        {
            // TODO: 세션 종료 로그/사운드/토스트 등 후처리

            if (isWork)
            {
                // Work → Break (라운드 결정은 Break 종료 시 처리)
                IsWork = false;
            }
            else
            {
                // Break → Work (긴 휴식 뒤엔 라운드 리셋)
                if (IsLongBreak) CurrentRound = 1;
                else CurrentRound = Math.Min(CurrentRound + 1, RoundsPerLongBreak);
                IsWork = true;
            }

            Remaining = SessionTotal;

            if (AutoStartNext)
            {
                if (!IsRunning) { IsRunning = true; _timer.Start(); }
            }
            else
            {
                if (IsRunning) { IsRunning = false; _timer.Stop(); }
            }
        }

        // === 설정값 변경 시, 실행 중이 아니면 남은 시간도 맞춰주기 ===
        partial void OnWorkSecondsChanged(int value)
        {
            if (!IsRunning && IsWork) Remaining = SessionTotal;
        }
        partial void OnShortBreakSecondsChanged(int value)
        {
            if (!IsRunning && !IsWork && !IsLongBreak) Remaining = SessionTotal;
        }
        partial void OnLongBreakSecondsChanged(int value)
        {
            if (!IsRunning && !IsWork && IsLongBreak) Remaining = SessionTotal;
        }
        partial void OnIsWorkChanged(bool value)
        {
            if (!IsRunning) Remaining = SessionTotal;
        }
        partial void OnCurrentRoundChanged(int value)
        {
            if (!IsRunning) Remaining = SessionTotal;
        }
    }
}
