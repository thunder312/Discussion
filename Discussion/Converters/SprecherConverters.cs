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
    private static readonly SolidColorBrush FarbeSchiedsrichter = new(Color.FromRgb(0xFF, 0xF1, 0xC2));
    private static readonly SolidColorBrush FarbeSystem = new(Color.FromRgb(0xFF, 0xDD, 0xDD));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Sprecher s
            ? s switch
            {
                Sprecher.PersonaA => FarbeA,
                Sprecher.PersonaB => FarbeB,
                Sprecher.Schiedsrichter => FarbeSchiedsrichter,
                _ => FarbeSystem
            }
            : Brushes.White;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class SprecherZuRandFarbeConverter : IValueConverter
{
    private static readonly SolidColorBrush RandA = new(Color.FromRgb(0x21, 0x4A, 0x8F));
    private static readonly SolidColorBrush RandB = new(Color.FromRgb(0x1E, 0x6E, 0x3E));
    private static readonly SolidColorBrush RandSchiedsrichter = new(Color.FromRgb(0xB5, 0x8A, 0x00));
    private static readonly SolidColorBrush RandSystem = new(Color.FromRgb(0xA5, 0x19, 0x2F));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Sprecher s
            ? s switch
            {
                Sprecher.PersonaA => RandA,
                Sprecher.PersonaB => RandB,
                Sprecher.Schiedsrichter => RandSchiedsrichter,
                _ => RandSystem
            }
            : Brushes.Gray;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
