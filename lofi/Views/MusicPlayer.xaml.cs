using lofi.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace lofi.Views
{
    public partial class MusicPlayer : UserControl
    {
        private bool _dragging;
        private Point _dragStart;
        private Point _origin;

        public MusicPlayer()
        {
            InitializeComponent();
            DataContext = new MusicPlayerViewModel(); // 기본 서비스로 VM 구성
            Loaded += (s, e) =>
            {
                if (double.IsNaN(Canvas.GetLeft(this))) Canvas.SetLeft(this, 0);
                if (double.IsNaN(Canvas.GetTop(this))) Canvas.SetTop(this, 0);
            };
        }

        // Slider ValueChanged 핸들러 (필요 없으면 XAML에서 제거)
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // 예시) 드래그 중 시킹 반영 등. 바인딩만으로 충분하면 비워두셔도 됩니다.
            // var vm = DataContext as YourMusicViewModel;
            // vm?.SeekToSeconds(e.NewValue);
        }

        // ===== 드래그 =====
        private void OnDragStart(object sender, MouseButtonEventArgs e)
        {
            _dragging = true;
            _dragStart = e.GetPosition(null);
            _origin = new Point(Canvas.GetLeft(this), Canvas.GetTop(this));
            CaptureMouse();
        }

        private void OnDragMove(object sender, MouseEventArgs e)
        {
            if (!_dragging) return;

            var current = e.GetPosition(null);
            var dx = current.X - _dragStart.X;
            var dy = current.Y - _dragStart.Y;

            var left = double.IsNaN(_origin.X) ? 0 : _origin.X;
            var top = double.IsNaN(_origin.Y) ? 0 : _origin.Y;

            var canvas = FindParentCanvas(this);
            if (canvas != null)
            {
                double newLeft = left + dx;
                double newTop = top + dy;

                // 부모 Canvas 안으로 클램프 (원하면 제거 가능)
                newLeft = Math.Max(0, Math.Min(newLeft, Math.Max(0, canvas.ActualWidth - ActualWidth)));
                newTop = Math.Max(0, Math.Min(newTop, Math.Max(0, canvas.ActualHeight - ActualHeight)));

                Canvas.SetLeft(this, newLeft);
                Canvas.SetTop(this, newTop);
            }
            else
            {
                // Canvas가 아니라면 Margin으로 이동 (fallback)
                var m = Margin;
                Margin = new Thickness(m.Left + dx, m.Top + dy, m.Right - dx, m.Bottom - dy);
                _dragStart = current; // 누적 이동
            }
        }

        private void OnDragEnd(object sender, MouseButtonEventArgs e)
        {
            _dragging = false;
            if (IsMouseCaptured) ReleaseMouseCapture();
        }

        private static Canvas? FindParentCanvas(DependencyObject d)
        {
            DependencyObject? p = d;
            while (p != null && p is not Canvas)
                p = VisualTreeHelper.GetParent(p);
            return p as Canvas;
        }
        // MusicPlayer.xaml.cs

        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MusicPlayerViewModel vm) vm.BeginSeek();
        }

        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MusicPlayerViewModel vm) vm.EndSeek();
        }

    }
}
