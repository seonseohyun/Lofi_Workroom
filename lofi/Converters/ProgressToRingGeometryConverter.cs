// Converters/ProgressToRingGeometryConverter.cs
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace lofi.Converters
{
    /// <summary>
    /// MultiBinding 입력:
    /// [0] progress: 0~1(double)
    /// [1] diameter: 링 직경(double) - 보통 Ellipse.Width 바인딩
    /// [2] thickness: 링 두께(double) - VM에서 제공
    /// 출력: 진행 아크 PathGeometry
    /// </summary>
    public sealed class ProgressToRingGeometryConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double progress = values.Length > 0 && values[0] is double p ? Math.Clamp(p, 0.0, 1.0) : 0.0;
            double diameter = values.Length > 1 && values[1] is double d ? d : 180.0;
            double thickness = values.Length > 2 && values[2] is double t ? Math.Max(1.0, t) : 10.0;

            // Stroke 중앙선 기준이므로 반경 보정
            double r = Math.Max(1.0, diameter / 2.0 - thickness / 2.0);

            // ArcSegment가 0/360에서 특이점 생겨서 살짝 보정
            double sweep = progress * 360.0;
            if (sweep >= 360.0) sweep = 359.999;
            if (sweep <= 0.0) sweep = 0.0001;

            // 12시(-90도)에서 시작
            const double startDeg = -90.0;
            double endDeg = startDeg + sweep;

            Point center = new(diameter / 2.0, diameter / 2.0);
            Point start = PointOnCircle(center, r, startDeg);
            Point end = PointOnCircle(center, r, endDeg);
            bool isLargeArc = sweep > 180.0;

            var fig = new PathFigure
            {
                StartPoint = start,
                IsClosed = false,
                IsFilled = false
            };
            fig.Segments.Add(new ArcSegment(
                point: end,
                size: new Size(r, r),
                rotationAngle: 0,
                isLargeArc: isLargeArc,
                sweepDirection: SweepDirection.Clockwise,
                isStroked: true));

            var geo = new PathGeometry();
            geo.Figures.Add(fig);
            geo.Freeze();
            return geo;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();

        private static Point PointOnCircle(Point c, double radius, double degrees)
        {
            double rad = degrees * Math.PI / 180.0;
            return new Point(c.X + radius * Math.Cos(rad),
                             c.Y + radius * Math.Sin(rad));
        }
    }
}
