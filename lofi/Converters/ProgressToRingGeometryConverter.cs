using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace lofi.Converters
{
    // values: [0]=progress(0..1), [1]=width, [2]=height, [3]=strokeThickness
    public class ProgressToDashArrayConverter : IMultiValueConverter
    {
        public object Convert(object[] v, Type targetType, object parameter, CultureInfo culture)
        {
            if (v == null || v.Length < 4) return new DoubleCollection { 0, 1 };
            if (Unset(v[0]) || Unset(v[1]) || Unset(v[2]) || Unset(v[3])) return new DoubleCollection { 0, 1 };

            double p = Clamp01(System.Convert.ToDouble(v[0]));
            double w = System.Convert.ToDouble(v[1]);
            double h = System.Convert.ToDouble(v[2]);
            double t = Math.Max(0.0001, System.Convert.ToDouble(v[3])); // 0 보호

            // 원 둘레(픽셀) = 2πr, r은 정원의 중심선 반지름(min(w,h)/2)
            double r = Math.Min(w, h) / 2.0;
            double circumferencePx = 2.0 * Math.PI * r;

            // ★ DashArray는 '두께 배수' 단위 → 픽셀 길이를 두께로 나눠 환산
            double unitsTotal = circumferencePx / t;
            double dash = unitsTotal * p;
            double gap = Math.Max(0.0, unitsTotal - dash);

            if (p <= 0.0) return new DoubleCollection { 0, unitsTotal };
            if (p >= 1.0) return new DoubleCollection { unitsTotal, 0 };
            return new DoubleCollection { dash, gap };
        }

        static bool Unset(object o) => o == null || o == DependencyProperty.UnsetValue;
        static double Clamp01(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
