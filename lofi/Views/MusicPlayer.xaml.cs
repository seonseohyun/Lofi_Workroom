using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace lofi.Views
{
    public partial class MusicPlayer : UserControl
{
    private const double MinW = 260;
    private const double MinH = 180;

    public MusicPlayer()
    {
        InitializeComponent();
    }

    // 부모 Canvas 찾기
    private Canvas? HostCanvas()
    {
        DependencyObject d = this;
        while (d != null && d is not Canvas)
            d = VisualTreeHelper.GetParent(d);
        return d as Canvas;
    }

    // === 변 리사이즈 ===
    private void ResizeLeft_OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        double newW = System.Math.Max(MinW, ActualWidth - e.HorizontalChange);
        double dx = ActualWidth - newW;
        Width = newW;

        var canvas = HostCanvas();
        if (canvas != null)
        {
            double left = Canvas.GetLeft(this);
            if (double.IsNaN(left)) left = 0;
            Canvas.SetLeft(this, left + dx);
        }
        e.Handled = true;
    }

    private void ResizeRight_OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        Width = System.Math.Max(MinW, ActualWidth + e.HorizontalChange);
        e.Handled = true;
    }

    private void ResizeTop_OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        double newH = System.Math.Max(MinH, ActualHeight - e.VerticalChange);
        double dy = ActualHeight - newH;
        Height = newH;

        var canvas = HostCanvas();
        if (canvas != null)
        {
            double top = Canvas.GetTop(this);
            if (double.IsNaN(top)) top = 0;
            Canvas.SetTop(this, top + dy);
        }
        e.Handled = true;
    }

    private void ResizeBottom_OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        Height = System.Math.Max(MinH, ActualHeight + e.VerticalChange);
        e.Handled = true;
    }

    // === 코너 리사이즈(조합) ===
    private void ResizeTopLeft_OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        ResizeLeft_OnDragDelta(sender, e);
        ResizeTop_OnDragDelta(sender, e);
    }

    private void ResizeTopRight_OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        ResizeRight_OnDragDelta(sender, e);
        ResizeTop_OnDragDelta(sender, e);
    }

    private void ResizeBottomLeft_OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        ResizeLeft_OnDragDelta(sender, e);
        ResizeBottom_OnDragDelta(sender, e);
    }

    private void ResizeBottomRight_OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        ResizeRight_OnDragDelta(sender, e);
        ResizeBottom_OnDragDelta(sender, e);
    }
}
}