using System.Windows.Media.Imaging;

namespace Discussion;

/// <summary>Lädt aus einer .ico den größten Frame (WPF nimmt sonst den ersten, meist 16x16, und skaliert ihn unscharf hoch).</summary>
public static class IconQuelle
{
    public static BitmapSource GroessterFrame(string packUri)
    {
        var decoder = new IconBitmapDecoder(new Uri(packUri), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        return decoder.Frames.OrderByDescending(f => f.PixelWidth).First();
    }
}
