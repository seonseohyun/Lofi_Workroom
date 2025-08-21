using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace lofi.ViewModels
{
    public enum ThemeMode { Light, Dark }

    public partial class MainViewModel : ObservableObject
    {
        private readonly ImageSource _lightThemeImage =
            new BitmapImage(new Uri("pack://application:,,,/lofi;component/Resources/light.png"));
        private readonly ImageSource _darkThemeImage  =
            new BitmapImage(new Uri("pack://application:,,,/lofi;component/Resources/dark.png"));

        private readonly ImageSource _lightModeButtonImage =
            new BitmapImage(new Uri("pack://application:,,,/lofi;component/Resources/lightmodebtn.png"));
        private readonly ImageSource _darkModeButtonImage  =
            new BitmapImage(new Uri("pack://application:,,,/lofi;component/Resources/darkmodebtn.png"));

        [ObservableProperty] private ImageSource _currentBackgroundImageSource;
        [ObservableProperty] private ImageSource _toggleButtonImageSource;
        [ObservableProperty] private TodoListViewModel _todoListViewModel = new();
        [ObservableProperty] private MusicPlayerViewModel _musicPlayerViewModel = new();   // 이미 쓰고 있으면 유지
        [ObservableProperty] private PomodoroViewModel _pomodoroViewModel = new();    
        private ThemeMode _theme = ThemeMode.Light;

        public MainViewModel()
        {
            ApplyTheme(_theme);
        }

        [RelayCommand]
        private void ToggleTheme()
        {
            _theme = _theme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;
            ApplyTheme(_theme);
            TodoListViewModel.UpdateTheme(_theme);
        }

        private void ApplyTheme(ThemeMode theme)
        {
            if (theme == ThemeMode.Light)
            {
                CurrentBackgroundImageSource = _lightThemeImage;
                ToggleButtonImageSource = _darkModeButtonImage;  // 버튼은 반대 모드 아이콘
            }
            else
            {
                CurrentBackgroundImageSource = _darkThemeImage;
                ToggleButtonImageSource = _lightModeButtonImage;
            }
        }
    }
}
