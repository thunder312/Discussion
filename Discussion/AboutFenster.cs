using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using EulenwerkCI;

namespace Discussion;

/// <summary>„Über KI-Diskussion“ im einheitlichen Eulenwerk-Layout (siehe D:\Dropbox\Design\CI\Design-System).</summary>
public sealed class AboutFenster : Window
{
    public AboutFenster(Window besitzer, string version)
    {
        Title = "Über KI-Diskussion";
        Owner = besitzer;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SizeToContent = SizeToContent.WidthAndHeight;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Background = (Brush)FindResource("SurfaceBrush");

        var logo = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/Assets/AppIcon.ico")),
            Width = 56, Height = 56,
        };
        RenderOptions.SetBitmapScalingMode(logo, BitmapScalingMode.HighQuality);

        var schliessen = new Button { Content = "Schließen", IsDefault = true, IsCancel = true, Margin = new Thickness(8, 0, 0, 0) };
        schliessen.Click += (_, _) => Close();

        var gesamt = new StackPanel { Margin = new Thickness(28, 24, 28, 20), MaxWidth = 560 };
        gesamt.Children.Add(Eulenwerk.Kopf(logo, "KI-Diskussion", version));
        gesamt.Children.Add(new TextBlock
        {
            Text = "Zwei unabhängige KI-Personas diskutieren ein frei gewähltes Thema, ein dritter Schiedsrichter wertet die " +
                   "Debatte aus – wahlweise über lokales Ollama oder jeden OpenAI-kompatiblen Endpunkt.",
            Foreground = (Brush)FindResource("MutedTextBrush"), FontSize = 13, TextWrapping = TextWrapping.Wrap,
        });
        gesamt.Children.Add(Eulenwerk.Abschnitt("Entwickelt von"));
        gesamt.Children.Add(Eulenwerk.EntwicklerKarten("https://github.com/thunder312/Discussion"));
        gesamt.Children.Add(Eulenwerk.Abschnitt("Technik"));
        gesamt.Children.Add(Eulenwerk.Technik("C# / WPF", ".NET 8", "MVVM", "Ollama", "OpenAI-kompatible API", "WiX"));
        gesamt.Children.Add(Eulenwerk.Fuss(schliessen));
        Content = gesamt;

        KeyDown += (_, e) => { if (e.Key == Key.Escape) Close(); };
    }
}
