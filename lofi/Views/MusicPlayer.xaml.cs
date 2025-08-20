using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using lofi.ViewModels;

namespace lofi.Views
{
    public partial class MusicPlayer : UserControl
    {
        // 이 VM만 보게 고정
        private readonly MusicPlayerViewModel _vm = new();

        public MusicPlayer()
        {
            InitializeComponent();

            // 디자이너에서는 무거운 초기화 피하고, 런타임에는 루트 Grid에 로컬값으로 DC 고정
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                // ★ 부모/스타일이 덮어써도 Root 이하에서는 절대 안 바뀜
                Root.DataContext = _vm;

                // 누가 바꾸면 즉시 되돌리기(한 번만 고정하고 끝낼 거면 아래 3줄은 생략해도 됨)
                Root.DataContextChanged += (s, e) =>
                {
                    if (!ReferenceEquals(Root.DataContext, _vm))
                        Root.DataContext = _vm;
                };

                Loaded += (_, __) =>
                {
                    Debug.WriteLine($"[MusicPlayer] DC={Root.DataContext?.GetType().Name ?? "null"}, Tracks={_vm.Tracks.Count}, Title={_vm.TrackTitle}");
                    if (double.IsNaN(Canvas.GetLeft(this))) Canvas.SetLeft(this, 0);
                    if (double.IsNaN(Canvas.GetTop(this))) Canvas.SetTop(this, 0);
                };
            }
            else
            {
                // 디자이너에서도 바인딩 깨지지 않게 임시 고정
                Root.DataContext = _vm;
            }
        }

        // 슬라이더 드래그(시킹)만 전달
        private void Slider_PreviewMouseDown(object sender, MouseButtonEventArgs e) => _vm.BeginSeek();
        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e) => _vm.EndSeek();
    }
}
