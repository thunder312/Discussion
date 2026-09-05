using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Discussion.Models;

namespace Discussion.Converters;

public class SprecherZuAusrichtungConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Sprecher s
            ? s switch
            {
                Sprecher.PersonaA => HorizontalAlignment.Left,
                Sprecher.PersonaB => HorizontalAlignment.Right,
                _ => HorizontalAlignment.Center
            }
            : HorizontalAlignment.Left;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class SprecherZuFarbeConverter : IValueConverter
{
    private static readonly SolidColorBrush FarbeA = new(Color.FromRgb(0xDD, 0xEB, 0xFF));
    private static readonly SolidColorBrush FarbeB = new(Color.FromRgb(0xDD, 0xFF, 0xE1));
    private static readonly SolidColorBrush FarbeSystem = new(Color.FromRgb(0xFF, 0xDD, 0xDD));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Sprecher s
            ? s switch
            {
                Sprecher.PersonaA => FarbeA,
                Sprecher.PersonaB => FarbeB,
                _ => FarbeSystem
            }
            : Brushes.White;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
