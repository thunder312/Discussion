using System.IO;
using Discussion.Models;

namespace Discussion.Services;

public class DiskussionsLogger : IDisposable
{
    public static string StandardOrdner => Path.Combine(AppContext.BaseDirectory, "Logs");

    private readonly StreamWriter _writer;
    public string Dateipfad { get; }

    public DiskussionsLogger(AppSettings settings)
    {
        var ordner = string.IsNullOrWhiteSpace(settings.Pfade.LogOrdner) ? StandardOrdner : settings.Pfade.LogOrdner;
        Directory.CreateDirectory(ordner);
        Dateipfad = Path.Combine(ordner, $"Diskussion_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        _writer = new StreamWriter(Dateipfad, append: true) { AutoFlush = true };
        SchreibeKopf(settings);
    }

    private void SchreibeKopf(AppSettings settings)
    {
        var positionA = settings.PositionPersonaA;
        var positionB = positionA.Gegenteil();

        _writer.WriteLine("==== Diskussions-Einstellungen ====");
        _writer.WriteLine($"Gestartet: {DateTime.Now:dd.MM.yyyy HH:mm}h");
        _writer.WriteLine($"Thema: {settings.Thema}");
        _writer.WriteLine($"Max. Texte je Persona: {settings.MaxTexteJePersona}");
        _writer.WriteLine($"KI-Endpunkt: {settings.Verbindung.BasisUrl} ({settings.Verbindung.Format})");
        _writer.WriteLine();
        SchreibePersona("Persona A", settings.PersonaA, positionA, settings.Verbindung.ModellA);
        _writer.WriteLine();
        SchreibePersona("Persona B", settings.PersonaB, positionB, settings.Verbindung.ModellB);
        _writer.WriteLine();
        _writer.WriteLine($"Schiedsrichter: {(settings.SchiedsrichterAktiv ? "aktiv" : "inaktiv")}");
        if (settings.SchiedsrichterAktiv)
            _writer.WriteLine($"  Modell: {settings.Verbindung.ModellSchiedsrichter}");
        _writer.WriteLine("====================================");
        _writer.WriteLine();
    }

    private void SchreibePersona(string label, PersonaProfil p, Position position, string modell)
    {
        string anzeigeName = string.IsNullOrWhiteSpace(p.Name) ? label : p.Name;
        _writer.WriteLine($"{label} ({anzeigeName}):");
        _writer.WriteLine($"  Position: {position}");
        _writer.WriteLine($"  Alter: {p.Alter}");
        _writer.WriteLine($"  Geschlecht: {p.Geschlecht}");
        _writer.WriteLine($"  Bildungsstand: {p.Bildungsstand}");
        _writer.WriteLine($"  Politische Ausrichtung: {p.PolitischeAusrichtung}");
        if (!string.IsNullOrWhiteSpace(p.Zusatz))
            _writer.WriteLine($"  Weitere Merkmale: {p.Zusatz}");
        _writer.WriteLine($"  Modell: {modell}");
    }

    public void Schreiben(ChatEintrag eintrag)
    {
        if (eintrag.Sprecher == Sprecher.Trenner)
        {
            _writer.WriteLine(eintrag.Text);
            return;
        }
        _writer.WriteLine($"{eintrag.Zeitpunkt:dd.MM.yyyy HH:mm}h {eintrag.AnzeigeName}: {eintrag.Text}");
    }

    public void Dispose() => _writer.Dispose();
}
