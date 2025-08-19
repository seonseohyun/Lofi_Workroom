using System.Windows;

namespace lofi
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainViewModel();
        }

        private void MusicPlayer_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
