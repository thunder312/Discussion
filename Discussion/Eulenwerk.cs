using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace EulenwerkCI;

/// <summary>
/// Gemeinsame Bausteine des Eulenwerk-Design-Systems für WPF-Apps (siehe D:\Dropbox\Design\CI\Design-System):
/// die Eule als Vektorgrafik, Entwickler-/Claude-Karten, Technik-Badges und die Eulenwerk-Fußzeile des Über-Dialogs.
/// Erwartet die Pinsel "Text", "TextMuted", "Surface", "SurfaceHover", "Border", "Accent", "AccentLight", "AccentSoft"
/// in den Application.Resources (Eulenwerk.Theme.xaml oder App.xaml).
/// </summary>
public static class Eulenwerk
{
    public const string Homepage = "https://daniel-ertl.de";
    public const string GitHub = "https://github.com/thunder312";
    public const string BuyMeABeer = "https://buymeacoffee.com/dertl";
    public const string ClaudeUrl = "https://claude.ai";

    private const string OwlStrokes =
        "M8 5 C11 12 16 13 24 13 C28 13 30 15 32 16 C34 15 36 13 40 13 C48 13 53 12 56 5 " +
        "M13 28 V48 C13 54 18 59 24 59 H40 C46 59 51 54 51 48 V28 M19 40 C18.5 48 19.5 54 23 59 M45 40 C45.5 48 44.5 54 41 59 " +
        "M29.5 39 L32 42.5 L34.5 39 M31 28 A9 9 0 1 1 13 28 A9 9 0 1 1 31 28 M51 28 A9 9 0 1 1 33 28 A9 9 0 1 1 51 28";
    private const string OwlFills = "M25.2 28 A3.2 3.2 0 1 1 18.8 28 A3.2 3.2 0 1 1 25.2 28 M45.2 28 A3.2 3.2 0 1 1 38.8 28 A3.2 3.2 0 1 1 45.2 28";

    private static Brush Pinsel(string key) =>
        Application.Current.TryFindResource(key) as Brush ?? Brushes.Gray;

    /// <summary>Die Eulenwerk-Eule (Linienstil, 64er-Raster) in gewünschter Größe und Farbe.</summary>
    public static FrameworkElement Eule(double groesse, Brush? farbe = null)
    {
        farbe ??= Pinsel("Accent");
        var canvas = new Canvas { Width = 64, Height = 64 };
        canvas.Children.Add(new System.Windows.Shapes.Path
        {
            Data = Geometry.Parse(OwlStrokes), Stroke = farbe, StrokeThickness = 4,
            StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round, StrokeLineJoin = PenLineJoin.Round,
        });
        canvas.Children.Add(new System.Windows.Shapes.Path { Data = Geometry.Parse(OwlFills), Fill = farbe });
        return new Viewbox { Width = groesse, Height = groesse, Child = canvas };
    }

    /// <summary>Klickbarer Link in Akzentfarbe (ohne Unterstreichung, Hover unterstrichen).</summary>
    public static Hyperlink Link(string text, string ziel)
    {
        var link = new Hyperlink(new Run(text))
        {
            NavigateUri = new Uri(ziel), Foreground = Pinsel("AccentLight"), TextDecorations = null,
            Cursor = System.Windows.Input.Cursors.Hand,
        };
        link.MouseEnter += (_, _) => link.TextDecorations = TextDecorations.Underline;
        link.MouseLeave += (_, _) => link.TextDecorations = null;
        link.RequestNavigate += (_, e) =>
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true }); }
            catch { /* kein Standardprogramm */ }
            e.Handled = true;
        };
        return link;
    }

    /// <summary>Kopfzeile des Über-Dialogs: App-Logo, Name, "Version x · Ein Eulenwerk-Projekt".</summary>
    public static FrameworkElement Kopf(FrameworkElement logo, string appName, string version, FontFamily? ueberschriftSchrift = null)
    {
        logo.Margin = new Thickness(0, 0, 18, 0);
        logo.VerticalAlignment = VerticalAlignment.Center;
        var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        text.Children.Add(new TextBlock
        {
            Text = appName, FontSize = 24, FontWeight = FontWeights.SemiBold, Foreground = Pinsel("Text"),
            FontFamily = ueberschriftSchrift ?? new FontFamily("Inter, Segoe UI"),
        });
        text.Children.Add(new TextBlock
        {
            Text = $"Version {version} · Ein Eulenwerk-Projekt", Foreground = Pinsel("TextMuted"), FontSize = 12.5,
            Margin = new Thickness(0, 2, 0, 0),
        });
        var zeile = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 16) };
        zeile.Children.Add(logo);
        zeile.Children.Add(text);
        return zeile;
    }

    /// <summary>Die beiden Standardkarten "Daniel Ertl" (GitHub, Buy me a beer, Quellcode) und "Claude".</summary>
    public static FrameworkElement EntwicklerKarten(string? quellcodeUrl)
    {
        var daniel = new List<(string, string?)>
        {
            ("github.com/thunder312", GitHub),
            ("Buy me a beer", BuyMeABeer),
        };
        if (!string.IsNullOrEmpty(quellcodeUrl)) daniel.Add(("Quellcode", quellcodeUrl));

        var zeile = new StackPanel { Orientation = Orientation.Horizontal };
        zeile.Children.Add(Karte("DE", Pinsel("Accent"), Pinsel("Bg"), "Daniel Ertl", "Idee & Entwicklung", daniel, 12));
        zeile.Children.Add(Karte("AI", Pinsel("SurfaceHover"), Pinsel("AccentLight"), "Claude", "KI-Assistent beim Programmieren",
            [("claude.ai", ClaudeUrl)], 0));
        return zeile;
    }

    /// <summary>Überschrift in Kapitälchen-Optik ("ENTWICKELT VON", "TECHNIK").</summary>
    public static TextBlock Abschnitt(string titel) => new()
    {
        Text = titel.ToUpperInvariant(), FontSize = 11, FontWeight = FontWeights.SemiBold,
        Foreground = Pinsel("TextMuted"), Margin = new Thickness(0, 14, 0, 8),
    };

    /// <summary>Technik-Badges (Pillen) in einer umbrechenden Zeile.</summary>
    public static FrameworkElement Technik(params string[] eintraege)
    {
        var wrap = new WrapPanel();
        foreach (var t in eintraege)
        {
            wrap.Children.Add(new Border
            {
                Background = Pinsel("SurfaceHover"), BorderBrush = Pinsel("Border"), BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(999), Padding = new Thickness(9, 2, 9, 3), Margin = new Thickness(0, 0, 8, 8),
                Child = new TextBlock { Text = t, FontSize = 11.5, FontWeight = FontWeights.SemiBold, Foreground = Pinsel("TextMuted") },
            });
        }
        return wrap;
    }

    /// <summary>Fußzeile: Eule + "Eulenwerk – Software aus der Freizeit" (Link) links, Copyright rechts, optional Schließen-Knopf.</summary>
    public static FrameworkElement Fuss(Button? schliessen = null)
    {
        var marke = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        marke.Children.Add(Eule(18));
        var tb = new TextBlock { FontSize = 12.5, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        tb.Inlines.Add(Link("Eulenwerk – Software aus der Freizeit", Homepage));
        marke.Children.Add(tb);

        var dock = new DockPanel { Margin = new Thickness(0, 16, 0, 0) };
        if (schliessen != null)
        {
            DockPanel.SetDock(schliessen, Dock.Right);
            dock.Children.Add(schliessen);
        }
        var copyright = new TextBlock
        {
            Text = $"© {DateTime.Now.Year} Daniel Ertl", Foreground = Pinsel("TextMuted"), FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 16, 0),
        };
        DockPanel.SetDock(copyright, Dock.Right);
        dock.Children.Add(copyright);
        dock.Children.Add(marke);
        return dock;
    }

    private static Border Karte(string initialen, Brush avatarBg, Brush avatarFg, string name, string rolle,
        IEnumerable<(string Text, string? Ziel)> links, double rechterRand)
    {
        var avatar = new Border
        {
            Width = 36, Height = 36, CornerRadius = new CornerRadius(18), Background = avatarBg,
            BorderBrush = Pinsel("Border"), BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 12, 0),
            VerticalAlignment = VerticalAlignment.Top,
            Child = new TextBlock
            {
                Text = initialen, FontSize = 12, FontWeight = FontWeights.Bold, Foreground = avatarFg,
                HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center,
            },
        };
        var sp = new StackPanel();
        sp.Children.Add(new TextBlock { Text = name, FontSize = 14, FontWeight = FontWeights.SemiBold, Foreground = Pinsel("Text") });
        sp.Children.Add(new TextBlock { Text = rolle, FontSize = 12, Foreground = Pinsel("TextMuted"), Margin = new Thickness(0, 0, 0, 6) });
        foreach (var (text, ziel) in links)
        {
            var tb = new TextBlock { FontSize = 12.5, Margin = new Thickness(0, 1, 0, 1) };
            if (ziel != null) tb.Inlines.Add(Link(text, ziel)); else tb.Inlines.Add(new Run(text) { Foreground = Pinsel("Text") });
            sp.Children.Add(tb);
        }
        var inhalt = new StackPanel { Orientation = Orientation.Horizontal };
        inhalt.Children.Add(avatar);
        inhalt.Children.Add(sp);
        return new Border
        {
            Background = Pinsel("SurfaceHover"), BorderBrush = Pinsel("Border"), BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10), Padding = new Thickness(14, 12, 18, 12), Margin = new Thickness(0, 0, rechterRand, 0),
            MinWidth = 240, Child = inhalt,
        };
    }
}
