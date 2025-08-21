// lofi.Views/PomodoroTimer.xaml.cs
using lofi.ViewModels;
using System.Windows.Controls;

namespace lofi.Views
{
    public partial class PomodoroTimer : UserControl
    {
        public PomodoroTimer()
        {
            InitializeComponent();
            DataContext ??= new MainViewModel();  
        }
    }
}
