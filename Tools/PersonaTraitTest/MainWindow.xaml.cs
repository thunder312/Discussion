using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using Discussion.Models;
using Discussion.Services;

namespace PersonaTraitTest;

public partial class MainWindow : Window
{
    private static readonly Dictionary<string, Action<PersonaProfil, string>> Setter = new()
    {
        ["Alter"] = (p, v) => p.Alter = v,
        ["Geschlecht"] = (p, v) => p.Geschlecht = v,
        ["Bildungsstand"] = (p, v) => p.Bildungsstand = v,
        ["Politische Ausrichtung"] = (p, v) => p.PolitischeAusrichtung = v,
        ["Weitere Merkmale"] = (p, v) => p.Zusatz = v,
    };

    private readonly MainViewModel _viewModel = new();
    private readonly IKiClient _client = new KiClient();
    private CancellationTokenSource? _cts;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        _viewModel.LogZeilen.CollectionChanged += LogZeilen_CollectionChanged;

        var settings = ConfigService.Laden();
        _viewModel.VerbindungsInfo = string.IsNullOrWhiteSpace(settings.Verbindung.BasisUrl)
            ? "Kein KI-Endpunkt konfiguriert - bitte zuerst in der Discussion-App einrichten."
            : $"Endpunkt: {settings.Verbindung.BasisUrl} ({settings.Verbindung.Format})   Modell (Persona A): {settings.Verbindung.ModellA}";
    }

    private void LogZeilen_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
            Dispatcher.BeginInvoke(() => LogScroll.ScrollToEnd());
    }

    private async void Start_Click(object sender, RoutedEventArgs e)
    {
        var settings = ConfigService.Laden();
        if (string.IsNullOrWhiteSpace(settings.Verbindung.BasisUrl) || string.IsNullOrWhiteSpace(settings.Verbindung.ModellA))
        {
            _viewModel.Status = "Kein KI-Endpunkt/Modell konfiguriert.";
            return;
        }

        _viewModel.LogZeilen.Clear();
        _viewModel.Laeuft = true;
        StartButton.IsEnabled = false;
        StoppButton.IsEnabled = true;
        _viewModel.Status = "läuft...";
        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        var normal = new PersonaProfil
        {
            Alter = ZeileWert("Alter").Normal,
            Geschlecht = ZeileWert("Geschlecht").Normal,
            Bildungsstand = ZeileWert("Bildungsstand").Normal,
            PolitischeAusrichtung = ZeileWert("Politische Ausrichtung").Normal,
            Zusatz = ZeileWert("Weitere Merkmale").Normal
        };

        var ergebnisse = new List<TestErgebnis>();
        string modell = settings.Verbindung.ModellA;
        string frage = _viewModel.Frage;

        try
        {
            foreach (var zeile in _viewModel.Zeilen)
            {
                ct.ThrowIfCancellationRequested();
                Log($"=== Merkmal: {zeile.Merkmal} ===");

                foreach (var (variante, wert) in new[] { ("normal", zeile.Normal), ("min", zeile.Min), ("max", zeile.Max) })
                {
                    ct.ThrowIfCancellationRequested();

                    var profil = KopiereNormal(normal);
                    if (variante != "normal")
                        Setter[zeile.Merkmal](profil, wert);

                    var uhr = Stopwatch.StartNew();
                    string antwort;
                    try
                    {
                        antwort = await FrageAsync(_client, settings.Verbindung, modell, profil, frage, ct);
                    }
                    catch (KiVerbindungsFehler ex)
                    {
                        antwort = $"[FEHLER] {ex.Message}";
                    }
                    uhr.Stop();

                    ergebnisse.Add(new TestErgebnis(
                        zeile.Merkmal, variante, variante == "normal" ? "(normal)" : wert,
                        profil.Alter, profil.Geschlecht, profil.Bildungsstand, profil.PolitischeAusrichtung, profil.Zusatz,
                        antwort, uhr.Elapsed.TotalSeconds));

                    Log($"  [{variante,-6}] ({wert}) -> {Kuerzen(antwort, 100)}  ({uhr.Elapsed.TotalSeconds:F1}s)");
                }
            }

            var ausgabeOrdner = Path.Combine(AppContext.BaseDirectory, "Ergebnisse");
            Directory.CreateDirectory(ausgabeOrdner);
            var zeitstempel = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var jsonPfad = Path.Combine(ausgabeOrdner, $"Ergebnisse_{zeitstempel}.json");

            var gesamtErgebnis = new
            {
                ErstelltAm = DateTime.Now,
                Endpunkt = settings.Verbindung.BasisUrl,
                Format = settings.Verbindung.Format.ToString(),
                Modell = modell,
                Temperature = settings.Verbindung.Temperature,
                Frage = frage,
                NormalProfil = normal,
                Ergebnisse = ergebnisse
            };

            File.WriteAllText(jsonPfad, JsonSerializer.Serialize(gesamtErgebnis, new JsonSerializerOptions { WriteIndented = true }));

            Log("");
            Log($"Fertig. {ergebnisse.Count} Antworten gesammelt.");
            Log($"Ergebnisse gespeichert unter: {jsonPfad}");
            _viewModel.Status = $"Fertig ({ergebnisse.Count} Antworten).";
        }
        catch (OperationCanceledException)
        {
            _viewModel.Status = "Abgebrochen.";
            Log("--- Abgebrochen ---");
        }
        finally
        {
            _viewModel.Laeuft = false;
            StartButton.IsEnabled = true;
            StoppButton.IsEnabled = false;
            _cts = null;
        }
    }

    private void Stopp_Click(object sender, RoutedEventArgs e)
    {
        _cts?.Cancel();
        _viewModel.Status = "Wird gestoppt...";
    }

    private MerkmalZeile ZeileWert(string merkmal) =>
        _viewModel.Zeilen.First(z => z.Merkmal == merkmal);

    private void Log(string text) => _viewModel.LogZeilen.Add(text);

    private static async Task<string> FrageAsync(IKiClient client, KiVerbindung verbindung, string modell, PersonaProfil profil, string frage, CancellationToken ct)
    {
        string profilBeschreibung = $"- Alter: {profil.Alter}\n- Geschlecht: {profil.Geschlecht}\n- Bildungsstand: {profil.Bildungsstand}\n- Politische Ausrichtung: {profil.PolitischeAusrichtung}";
        if (!string.IsNullOrWhiteSpace(profil.Zusatz))
            profilBeschreibung += $"\n- Weitere Merkmale: {profil.Zusatz}";

        string system =
$@"Du bist folgende Person:
{profilBeschreibung}

Beantworte die folgende Frage aus der Sicht dieser Person. Deine Wortwahl, dein Argumentationsstil und deine Meinung sollen erkennbar zu diesem Profil passen. Antworte ausschließlich mit deiner Antwort in 2 bis 5 Sätzen, ohne Meta-Kommentare und ohne die Frage zu wiederholen.";

        var nachrichten = new List<ChatMessage> { new("system", system), new("user", frage) };
        return await client.SendeAsync(verbindung, modell, nachrichten, ct);
    }

    private static PersonaProfil KopiereNormal(PersonaProfil normal) => new()
    {
        Name = "",
        Alter = normal.Alter,
        Geschlecht = normal.Geschlecht,
        Bildungsstand = normal.Bildungsstand,
        PolitischeAusrichtung = normal.PolitischeAusrichtung,
        Zusatz = normal.Zusatz
    };

    private static string Kuerzen(string s, int laenge) => s.Length > laenge ? s[..laenge] + "..." : s;
}
