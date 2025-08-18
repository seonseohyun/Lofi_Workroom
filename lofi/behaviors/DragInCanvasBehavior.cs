using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace lofi.Behavior
{
    public static class DragInCanvasBehavior
    {
        // ===== Public attached props =====
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled", typeof(bool), typeof(DragInCanvasBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));
        public static void SetIsEnabled(DependencyObject d, bool value) => d.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject d) => (bool)d.GetValue(IsEnabledProperty);

        public static readonly DependencyProperty BringToFrontProperty =
            DependencyProperty.RegisterAttached(
                "BringToFront", typeof(bool), typeof(DragInCanvasBehavior),
                new PropertyMetadata(true));
        public static void SetBringToFront(DependencyObject d, bool value) => d.SetValue(BringToFrontProperty, value);
        public static bool GetBringToFront(DependencyObject d) => (bool)d.GetValue(BringToFrontProperty);

        public static readonly DependencyProperty ClampToParentProperty =
            DependencyProperty.RegisterAttached(
                "ClampToParent", typeof(bool), typeof(DragInCanvasBehavior),
                new PropertyMetadata(true));
        public static void SetClampToParent(DependencyObject d, bool value) => d.SetValue(ClampToParentProperty, value);
        public static bool GetClampToParent(DependencyObject d) => (bool)d.GetValue(ClampToParentProperty);

        public static readonly DependencyProperty IgnoreInteractiveChildrenProperty =
            DependencyProperty.RegisterAttached(
                "IgnoreInteractiveChildren", typeof(bool), typeof(DragInCanvasBehavior),
                new PropertyMetadata(true));
        public static void SetIgnoreInteractiveChildren(DependencyObject d, bool value) => d.SetValue(IgnoreInteractiveChildrenProperty, value);
        public static bool GetIgnoreInteractiveChildren(DependencyObject d) => (bool)d.GetValue(IgnoreInteractiveChildrenProperty);

        /// <summary>
        /// 드래그 시작을 허용하는 영역(Grip) 표시용. 이 속성이 True인 요소(또는 그 자식)에서만 드래그 시작.
        /// </summary>
        public static readonly DependencyProperty IsDragGripProperty =
            DependencyProperty.RegisterAttached(
                "IsDragGrip", typeof(bool), typeof(DragInCanvasBehavior),
                new PropertyMetadata(false));
        public static void SetIsDragGrip(DependencyObject d, bool value) => d.SetValue(IsDragGripProperty, value);
        public static bool GetIsDragGrip(DependencyObject d) => (bool)d.GetValue(IsDragGripProperty);

        // ===== Private state per element =====
        private static readonly DependencyProperty IsDraggingProperty =
            DependencyProperty.RegisterAttached("IsDragging", typeof(bool), typeof(DragInCanvasBehavior), new PropertyMetadata(false));
        private static void SetIsDragging(DependencyObject d, bool v) => d.SetValue(IsDraggingProperty, v);
        private static bool GetIsDragging(DependencyObject d) => (bool)d.GetValue(IsDraggingProperty);

        private static readonly DependencyProperty StartPointProperty =
            DependencyProperty.RegisterAttached("StartPoint", typeof(Point), typeof(DragInCanvasBehavior), new PropertyMetadata(default(Point)));
        private static void SetStartPoint(DependencyObject d, Point v) => d.SetValue(StartPointProperty, v);
        private static Point GetStartPoint(DependencyObject d) => (Point)d.GetValue(StartPointProperty);

        private static readonly DependencyProperty StartLeftProperty =
            DependencyProperty.RegisterAttached("StartLeft", typeof(double), typeof(DragInCanvasBehavior), new PropertyMetadata(0.0));
        private static void SetStartLeft(DependencyObject d, double v) => d.SetValue(StartLeftProperty, v);
        private static double GetStartLeft(DependencyObject d) => (double)d.GetValue(StartLeftProperty);

        private static readonly DependencyProperty StartTopProperty =
            DependencyProperty.RegisterAttached("StartTop", typeof(double), typeof(DragInCanvasBehavior), new PropertyMetadata(0.0));
        private static void SetStartTop(DependencyObject d, double v) => d.SetValue(StartTopProperty, v);
        private static double GetStartTop(DependencyObject d) => (double)d.GetValue(StartTopProperty);

        // ===== Wiring =====
        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement fe)
            {
                if ((bool)e.NewValue)
                {
                    fe.Loaded += OnLoaded;
                    fe.PreviewMouseLeftButtonDown += OnMouseDown;
                    fe.PreviewMouseMove += OnMouseMove;
                    fe.PreviewMouseLeftButtonUp += OnMouseUp;
                }
                else
                {
                    fe.Loaded -= OnLoaded;
                    fe.PreviewMouseLeftButtonDown -= OnMouseDown;
                    fe.PreviewMouseMove -= OnMouseMove;
                    fe.PreviewMouseLeftButtonUp -= OnMouseUp;
                }
            }
        }

        private static void OnLoaded(object? sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe)
            {
                if (double.IsNaN(Canvas.GetLeft(fe))) Canvas.SetLeft(fe, 0);
                if (double.IsNaN(Canvas.GetTop(fe))) Canvas.SetTop(fe, 0);
            }
        }

        private const double DragThreshold = 4.0; // px

        private static void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement fe) return;
            var canvas = FindCanvas(fe);
            if (canvas is null) return;

            // 입력/버튼/Thumb 등은 드래그 시작 무시
            if (GetIgnoreInteractiveChildren(fe) && IsInteractiveChild(e.OriginalSource as DependencyObject))
                return;

            // ✅ Grip(헤더 등)에서만 드래그 시작 허용
            if (!IsWithinDragGrip(e.OriginalSource as DependencyObject))
                return;

            fe.CaptureMouse();
            SetIsDragging(fe, false); // 임계치 넘기 전까지는 드래그 아님
            SetStartPoint(fe, e.GetPosition(canvas));
            SetStartLeft(fe, Canvas.GetLeft(fe));
            SetStartTop(fe, Canvas.GetTop(fe));
            e.Handled = false; // 다른 컨트롤 동작 필요 시 false 유지
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is not FrameworkElement fe) return;
            var canvas = FindCanvas(fe);
            if (canvas is null) return;

            var pos = e.GetPosition(canvas);
            var delta = pos - GetStartPoint(fe);

            // 임계치 넘을 때만 드래그 시작
            if (!GetIsDragging(fe) && fe.IsMouseCaptured)
            {
                if (Math.Abs(delta.X) < DragThreshold && Math.Abs(delta.Y) < DragThreshold)
                    return;
                SetIsDragging(fe, true);
                if (GetBringToFront(fe))
                    Panel.SetZIndex(fe, GetNextZ(canvas));
            }

            if (!GetIsDragging(fe)) return;

            var newLeft = GetStartLeft(fe) + delta.X;
            var newTop = GetStartTop(fe) + delta.Y;

            if (GetClampToParent(fe))
            {
                newLeft = Math.Max(0, Math.Min(newLeft, Math.Max(0, canvas.ActualWidth - fe.ActualWidth)));
                newTop = Math.Max(0, Math.Min(newTop, Math.Max(0, canvas.ActualHeight - fe.ActualHeight)));
            }

            Canvas.SetLeft(fe, newLeft);
            Canvas.SetTop(fe, newTop);
        }

        private static void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is not FrameworkElement fe) return;
            SetIsDragging(fe, false);
            if (fe.IsMouseCaptured) fe.ReleaseMouseCapture();
        }

        // ===== helpers =====
        private static Canvas? FindCanvas(DependencyObject? d)
        {
            while (d != null && d is not Canvas)
                d = VisualTreeHelper.GetParent(d);
            return d as Canvas;
        }

        private static bool IsInteractiveChild(DependencyObject? d)
        {
            while (d != null)
            {
                if (d is TextBoxBase || d is PasswordBox || d is ButtonBase || d is Selector
                    || d is Thumb) // 리사이즈 핸들 무시
                    return true;
                d = VisualTreeHelper.GetParent(d);
            }
            return false;
        }

        /// <summary>
        /// 클릭된 요소에서 위로 올라가며 IsDragGrip=True가 달린 요소를 찾음.
        /// (붙인 곳에서만 드래그 시작 허용)
        /// </summary>
        private static bool IsWithinDragGrip(DependencyObject? d)
        {
            while (d != null)
            {
                if (GetIsDragGrip(d)) return true;
                d = VisualTreeHelper.GetParent(d);
            }
            return false;
        }

        private static int GetNextZ(Canvas canvas)
        {
            int max = 0;
            foreach (UIElement child in canvas.Children)
                max = Math.Max(max, Panel.GetZIndex(child));
            return max + 1;
        }
    }
}
