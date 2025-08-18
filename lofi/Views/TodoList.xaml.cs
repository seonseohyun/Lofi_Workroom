using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace lofi.Views
{
    public partial class TodoList : UserControl
    {
        private const double MinW = 160;
        private const double MinH = 120;

        public TodoList()
        {
            InitializeComponent();              // �� �ݵ�� �ʿ�!
            Loaded += (s, e) =>
            {
                // ����: NaN ��ǥ/������ ���� (����)
                if (double.IsNaN(Width) || Width <= 0) Width = 240;
                if (double.IsNaN(Height) || Height <= 0) Height = 340;
                if (double.IsNaN(Canvas.GetLeft(this))) Canvas.SetLeft(this, 0);
                if (double.IsNaN(Canvas.GetTop(this))) Canvas.SetTop(this, 0);
            };
        }

        // ===== ���� ��ƿ: �θ� Canvas ã�� =====
        private Canvas? FindHostCanvas()
        {
            DependencyObject d = this;
            while (d != null && d is not Canvas)
                d = VisualTreeHelper.GetParent(d);
            return d as Canvas;
        }

        // ===== ���� �������� =====
        private void ResizeLeft_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            double newW = Math.Max(MinW, this.ActualWidth - e.HorizontalChange);
            double dx = this.ActualWidth - newW;
            this.Width = newW;

            var canvas = FindHostCanvas();
            if (canvas != null)
            {
                double left = Canvas.GetLeft(this);
                if (double.IsNaN(left)) left = 0;
                Canvas.SetLeft(this, left + dx);
            }
            e.Handled = true;
        }

        // ===== ���� �������� =====
        private void ResizeRight_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            this.Width = Math.Max(MinW, this.ActualWidth + e.HorizontalChange);
            e.Handled = true;
        }

        // ===== ��� �������� =====
        private void ResizeTop_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            double newH = Math.Max(MinH, this.ActualHeight - e.VerticalChange);
            double dy = this.ActualHeight - newH;
            this.Height = newH;

            var canvas = FindHostCanvas();
            if (canvas != null)
            {
                double top = Canvas.GetTop(this);
                if (double.IsNaN(top)) top = 0;
                Canvas.SetTop(this, top + dy);
            }
            e.Handled = true;
        }

        // ===== �ϴ� �������� =====
        private void ResizeBottom_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            this.Height = Math.Max(MinH, this.ActualHeight + e.VerticalChange);
            e.Handled = true;
        }

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
