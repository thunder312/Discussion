using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Discussion.Models;
using Discussion.Services;

namespace Discussion.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IKiClient _client = new KiClient();
    private CancellationTokenSource? _cts;
    private readonly DispatcherTimer _liveTimer;

    public AppSettings Settings { get; }
    public ObservableCollection<ChatEintrag> Verlauf { get; } = new();
    public ObservableCollection<string> GefundeneModelle { get; } = new();
    public ObservableCollection<string> PersonaVorlagen { get; } = new();

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

    private string _verstricheneZeitAnzeige = "–";
    public string VerstricheneZeitAnzeige
    {
        get => _verstricheneZeitAnzeige;
        private set { _verstricheneZeitAnzeige = value; OnPropertyChanged(); }
    }

    private DateTime _startZeit;
    private DateTime _rundenStartZeit;
    private double? _durchschnittSekundenProRunde;

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
    public RelayCommand VorlageSpeichernCommand { get; }
    public RelayCommand VorlageLadenCommand { get; }
    public RelayCommand PersonaOrdnerWaehlenCommand { get; }
    public RelayCommand LogOrdnerWaehlenCommand { get; }

    private string EffektiverPersonaOrdner =>
        string.IsNullOrWhiteSpace(Settings.Pfade.PersonaVorlagenOrdner)
            ? PersonaVorlagenService.StandardOrdner
            : Settings.Pfade.PersonaVorlagenOrdner;

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
        LlmSuchenCommand = new RelayCommand(LlmSuchenButtonAsync);
        VorlageSpeichernCommand = new RelayCommand(param => VorlageSpeichern(param as PersonaProfil));
        VorlageLadenCommand = new RelayCommand(param => VorlageLaden(param as object[]));
        PersonaOrdnerWaehlenCommand = new RelayCommand(() => OrdnerWaehlen(
            Settings.Pfade.PersonaVorlagenOrdner, PersonaVorlagenService.StandardOrdner,
            pfad => { Settings.Pfade.PersonaVorlagenOrdner = pfad; AktualisierePersonaVorlagen(); }));
        LogOrdnerWaehlenCommand = new RelayCommand(() => OrdnerWaehlen(
            Settings.Pfade.LogOrdner, DiskussionsLogger.StandardOrdner,
            pfad => Settings.Pfade.LogOrdner = pfad));

        _liveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _liveTimer.Tick += (_, _) => AktualisiereLiveAnzeige();

        AktualisierePersonaVorlagen();

        if (!string.IsNullOrWhiteSpace(Settings.Verbindung.BasisUrl))
            _ = LlmSuchenAsync(zeigeFehlerDialog: false);
    }

    private async Task StartenAsync()
    {
        ConfigService.Speichern(Settings);
        Verlauf.Clear();
        Laeuft = true;
        AktuelleRunde = 0;
        GesamtRunden = Settings.MaxTexteJePersona;
        EtaAnzeige = "wird berechnet...";
        VerstricheneZeitAnzeige = "0 s";
        _durchschnittSekundenProRunde = null;
        _startZeit = DateTime.Now;
        _rundenStartZeit = _startZeit;
        _cts = new CancellationTokenSource();
        _liveTimer.Start();
        DiskussionsLogger? logger = null;
        try
        {
            logger = new DiskussionsLogger(Settings);
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
            _liveTimer.Stop();
            logger?.Dispose();
            Laeuft = false;
            _cts = null;
        }
    }

    private void AktualisiereFortschritt(int aktuelleRunde, int gesamtRunden)
    {
        var jetzt = DateTime.Now;
        int abgeschlosseneRunden = aktuelleRunde - 1;
        _durchschnittSekundenProRunde = abgeschlosseneRunden > 0
            ? (jetzt - _startZeit).TotalSeconds / abgeschlosseneRunden
            : null;
        _rundenStartZeit = jetzt;

        AktuelleRunde = aktuelleRunde;
        GesamtRunden = gesamtRunden;
        AktualisiereLiveAnzeige();
    }

    private void AktualisiereLiveAnzeige()
    {
        var jetzt = DateTime.Now;
        VerstricheneZeitAnzeige = FormatiereDauer((jetzt - _startZeit).TotalSeconds);

        if (_durchschnittSekundenProRunde is not double durchschnitt)
        {
            EtaAnzeige = "wird berechnet...";
            return;
        }

        double zeitInAktuellerRunde = (jetzt - _rundenStartZeit).TotalSeconds;
        double restAktuelleRunde = Math.Max(0, durchschnitt - zeitInAktuellerRunde);
        int restRundenDanach = Math.Max(0, GesamtRunden - AktuelleRunde);
        EtaAnzeige = FormatiereDauer(restAktuelleRunde + restRundenDanach * durchschnitt);
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

    private Task LlmSuchenButtonAsync() => LlmSuchenAsync(zeigeFehlerDialog: true);

    private async Task LlmSuchenAsync(bool zeigeFehlerDialog)
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
            if (zeigeFehlerDialog)
                MessageBox.Show(ex.Message, "Modellsuche fehlgeschlagen", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void VorlageSpeichern(PersonaProfil? profil)
    {
        if (profil is null)
            return;
        if (string.IsNullOrWhiteSpace(profil.Name))
        {
            MessageBox.Show("Bitte zuerst im Feld 'Name' einen Namen vergeben, unter dem die Vorlage gespeichert werden soll.",
                "Name fehlt", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        PersonaVorlagenService.Speichern(profil, EffektiverPersonaOrdner);
        AktualisierePersonaVorlagen();
        Status = $"Vorlage '{profil.Name}' gespeichert.";
    }

    private void VorlageLaden(object[]? werte)
    {
        if (werte is null || werte.Length < 2 || werte[0] is not PersonaProfil ziel)
            return;

        if (werte[1] is not string vorlageName || string.IsNullOrWhiteSpace(vorlageName))
        {
            Status = "Bitte zuerst eine Vorlage auswählen.";
            return;
        }

        try
        {
            var geladen = PersonaVorlagenService.Laden(vorlageName, EffektiverPersonaOrdner);
            ziel.Name = geladen.Name;
            ziel.Alter = geladen.Alter;
            ziel.Geschlecht = geladen.Geschlecht;
            ziel.Bildungsstand = geladen.Bildungsstand;
            ziel.PolitischeAusrichtung = geladen.PolitischeAusrichtung;
            ziel.Zusatz = geladen.Zusatz;
            Status = $"Vorlage '{vorlageName}' geladen.";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Vorlage konnte nicht geladen werden: {ex.Message}", "Fehler",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void AktualisierePersonaVorlagen()
    {
        PersonaVorlagen.Clear();
        foreach (var name in PersonaVorlagenService.Auflisten(EffektiverPersonaOrdner))
            PersonaVorlagen.Add(name);
    }

    private static void OrdnerWaehlen(string aktuell, string standard, Action<string> setzen)
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            InitialDirectory = string.IsNullOrWhiteSpace(aktuell) ? standard : aktuell,
            Title = "Ordner auswählen"
        };
        if (dialog.ShowDialog() == true)
            setzen(dialog.FolderName);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
