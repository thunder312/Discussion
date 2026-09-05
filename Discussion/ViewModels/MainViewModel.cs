using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Discussion.Models;
using Discussion.Services;

namespace Discussion.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IKiClient _client = new KiClient();
    private CancellationTokenSource? _cts;

    public AppSettings Settings { get; }
    public ObservableCollection<ChatEintrag> Verlauf { get; } = new();
    public ObservableCollection<string> GefundeneModelle { get; } = new();

    public Array ApiFormatWerte { get; } = Enum.GetValues(typeof(ApiFormat));

    public string[] GeschlechtOptionen { get; } = { "weiblich", "männlich", "divers" };

    public string[] BildungsstandOptionen { get; } =
    {
        "Hauptschulabschluss", "Realschulabschluss", "Abitur", "Studium/Akademiker", "Promotion"
    };

    public string[] PolitischeAusrichtungOptionen { get; } =
    {
        "Links", "Mitte-links", "Mitte", "Mitte-rechts", "Rechts", "Libertär", "Unpolitisch"
    };

    public Array PositionWerte { get; } = Enum.GetValues(typeof(Position));

    private Position _positionPersonaA;
    public Position PositionPersonaA
    {
        get => _positionPersonaA;
        set
        {
            _positionPersonaA = value;
            Settings.PositionPersonaA = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(PositionPersonaBAnzeige));
        }
    }

    public string PositionPersonaBAnzeige => PositionPersonaA.Gegenteil().ToString();

    private int _aktuelleRunde;
    public int AktuelleRunde
    {
        get => _aktuelleRunde;
        private set { _aktuelleRunde = value; OnPropertyChanged(); OnPropertyChanged(nameof(RundenAnzeige)); }
    }

    private int _gesamtRunden;
    public int GesamtRunden
    {
        get => _gesamtRunden;
        private set { _gesamtRunden = value; OnPropertyChanged(); OnPropertyChanged(nameof(RundenAnzeige)); }
    }

    public string RundenAnzeige => GesamtRunden > 0 ? $"{AktuelleRunde} / {GesamtRunden}" : "–";

    private string _etaAnzeige = "–";
    public string EtaAnzeige
    {
        get => _etaAnzeige;
        private set { _etaAnzeige = value; OnPropertyChanged(); }
    }

    private DateTime _startZeit;

    private bool _laeuft;
    public bool Laeuft
    {
        get => _laeuft;
        private set
        {
            _laeuft = value;
            OnPropertyChanged();
            StartCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
        }
    }

    private string _status = "Bereit.";
    public string Status
    {
        get => _status;
        private set { _status = value; OnPropertyChanged(); }
    }

    public RelayCommand StartCommand { get; }
    public RelayCommand StopCommand { get; }
    public RelayCommand SpeichernCommand { get; }
    public RelayCommand TestVerbindungCommand { get; }
    public RelayCommand LlmSuchenCommand { get; }

    public MainViewModel()
    {
        Settings = ConfigService.Laden();
        _positionPersonaA = Settings.PositionPersonaA;
        StartCommand = new RelayCommand(StartenAsync, () => !Laeuft);
        StopCommand = new RelayCommand(Stoppen, () => Laeuft);
        SpeichernCommand = new RelayCommand(() =>
        {
            ConfigService.Speichern(Settings);
            Status = "Konfiguration gespeichert.";
        });
        TestVerbindungCommand = new RelayCommand(TestVerbindungAsync);
        LlmSuchenCommand = new RelayCommand(LlmSuchenAsync);
    }

    private async Task StartenAsync()
    {
        ConfigService.Speichern(Settings);
        Verlauf.Clear();
        Laeuft = true;
        AktuelleRunde = 0;
        GesamtRunden = Settings.MaxTexteJePersona;
        EtaAnzeige = "wird berechnet...";
        _startZeit = DateTime.Now;
        _cts = new CancellationTokenSource();
        DiskussionsLogger? logger = null;
        try
        {
            logger = new DiskussionsLogger(Settings.Thema);
            Status = $"Läuft... (Log: {logger.Dateipfad})";

            var engine = new DiskussionsEngine(_client);
            engine.NeuerEintrag += eintrag => Verlauf.Add(eintrag);
            engine.FortschrittGeaendert += AktualisiereFortschritt;
            await engine.StartenAsync(Settings, logger, _cts.Token);

            Status = "Diskussion beendet.";
            EtaAnzeige = "Fertig";
        }
        catch (OperationCanceledException)
        {
            Status = "Abgebrochen.";
            EtaAnzeige = "–";
        }
        catch (KiVerbindungsFehler ex)
        {
            var fehlerEintrag = new ChatEintrag(DateTime.Now, Sprecher.System, "System", ex.Message);
            Verlauf.Add(fehlerEintrag);
            logger?.Schreiben(fehlerEintrag);
            Status = "Fehler - siehe Verlauf.";
            EtaAnzeige = "–";
        }
        finally
        {
            logger?.Dispose();
            Laeuft = false;
            _cts = null;
        }
    }

    private void AktualisiereFortschritt(int aktuelleRunde, int gesamtRunden)
    {
        AktuelleRunde = aktuelleRunde;
        GesamtRunden = gesamtRunden;

        int abgeschlosseneRunden = aktuelleRunde - 1;
        if (abgeschlosseneRunden <= 0)
        {
            EtaAnzeige = "wird berechnet...";
            return;
        }

        double elapsedSekunden = (DateTime.Now - _startZeit).TotalSeconds;
        double sekundenProRunde = elapsedSekunden / abgeschlosseneRunden;
        int restRunden = gesamtRunden - abgeschlosseneRunden;
        EtaAnzeige = FormatiereDauer(sekundenProRunde * restRunden);
    }

    private static string FormatiereDauer(double sekunden)
    {
        if (sekunden < 1)
            return "< 1 s";
        var dauer = TimeSpan.FromSeconds(sekunden);
        return dauer.TotalMinutes >= 1
            ? $"~{(int)dauer.TotalMinutes} min {dauer.Seconds} s"
            : $"~{(int)dauer.TotalSeconds} s";
    }

    private void Stoppen()
    {
        _cts?.Cancel();
        Status = "Wird gestoppt...";
    }

    private async Task TestVerbindungAsync()
    {
        Status = "Teste Verbindung...";
        try
        {
            var nachrichten = new List<ChatMessage>
            {
                new("system", "Antworte ausschließlich mit dem einen Wort: OK"),
                new("user", "Test")
            };
            var antwort = await _client.SendeAsync(Settings.Verbindung, Settings.Verbindung.ModellA, nachrichten, CancellationToken.None);
            Status = "Verbindung erfolgreich.";
            MessageBox.Show($"Antwort der KI (Modell '{Settings.Verbindung.ModellA}'):\n{antwort}", "Verbindungstest",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (KiVerbindungsFehler ex)
        {
            Status = "Verbindungstest fehlgeschlagen.";
            MessageBox.Show(ex.Message, "Verbindungstest fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LlmSuchenAsync()
    {
        Status = "Suche verfügbare Modelle...";
        try
        {
            var modelle = await _client.ListeModelleAsync(Settings.Verbindung, CancellationToken.None);
            GefundeneModelle.Clear();
            foreach (var modell in modelle)
                GefundeneModelle.Add(modell);
            Status = modelle.Count > 0
                ? $"{modelle.Count} Modell(e) gefunden."
                : "Keine Modelle am Endpunkt gefunden.";
        }
        catch (KiVerbindungsFehler ex)
        {
            Status = "Modellsuche fehlgeschlagen.";
            MessageBox.Show(ex.Message, "Modellsuche fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
