using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Discussion;

/// <summary>Schaltet die Windows-Titelleiste eines Fensters auf das dunkle Design (Windows 10 20H1+/11).</summary>
public static class DunkleTitelleiste
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int wert, int groesse);

    public static void Anwenden(Window fenster)
    {
        fenster.SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(fenster).Handle;
            int an = 1;
            if (DwmSetWindowAttribute(hwnd, 20, ref an, sizeof(int)) != 0)
                DwmSetWindowAttribute(hwnd, 19, ref an, sizeof(int));
        };
    }
}
