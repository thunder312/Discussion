using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Discussion.ViewModels;

namespace Discussion;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
        _viewModel.Verlauf.CollectionChanged += Verlauf_CollectionChanged;
    }

    private void Verlauf_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            Dispatcher.BeginInvoke(() => VerlaufScroll.ScrollToEnd());
    }

    private void TestToolOeffnen_Click(object sender, RoutedEventArgs e)
    {
        var pfad = FindePersonaTraitTestExe();
        if (pfad == null)
        {
            MessageBox.Show(
                "PersonaTraitTest.exe wurde nicht gefunden. Bitte zuerst das Projekt Tools/PersonaTraitTest bauen.",
                "Nicht gefunden", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(pfad)
            {
                UseShellExecute = true,
                WorkingDirectory = Path.GetDirectoryName(pfad)!
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Konnte Persona-Merkmal-Test nicht starten: {ex.Message}",
                "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static string? FindePersonaTraitTestExe()
    {
        string[] kandidaten =
        {
            Path.Combine(AppContext.BaseDirectory, "PersonaTraitTest.exe"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Tools", "PersonaTraitTest", "bin", "Debug", "net8.0-windows", "PersonaTraitTest.exe"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Tools", "PersonaTraitTest", "bin", "Release", "net8.0-windows", "PersonaTraitTest.exe"),
        };

        foreach (var kandidat in kandidaten)
        {
            var voll = Path.GetFullPath(kandidat);
            if (File.Exists(voll))
                return voll;
        }
        return null;
    }
}
