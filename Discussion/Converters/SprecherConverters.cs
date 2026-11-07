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
    private static readonly SolidColorBrush FarbeA = new(Color.FromRgb(0x1B, 0x2A, 0x4A));
    private static readonly SolidColorBrush FarbeB = new(Color.FromRgb(0x1D, 0x3B, 0x2C));
    private static readonly SolidColorBrush FarbeSchiedsrichter = new(Color.FromRgb(0x3D, 0x2F, 0x10));
    private static readonly SolidColorBrush FarbeSystem = new(Color.FromRgb(0x3D, 0x1F, 0x1F));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is Sprecher s
            ? s switch
            {
                Sprecher.PersonaA => FarbeA,
                Sprecher.PersonaB => FarbeB,
                Sprecher.Schiedsrichter => FarbeSchiedsrichter,
                _ => FarbeSystem
            }
            : new SolidColorBrush(Color.FromRgb(0x1F, 0x23, 0x28));

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class SprecherZuRandFarbeConverter : IValueConverter
{
    private static readonly SolidColorBrush RandA = new(Color.FromRgb(0x5B, 0x8D, 0xEF));
    private static readonly SolidColorBrush RandB = new(Color.FromRgb(0x3E, 0xCF, 0x8E));
    private static readonly SolidColorBrush RandSchiedsrichter = new(Color.FromRgb(0xF0, 0xB4, 0x29));
    private static readonly SolidColorBrush RandSystem = new(Color.FromRgb(0xF2, 0x6D, 0x6D));

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
