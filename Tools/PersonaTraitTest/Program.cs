using System.IO;
using System.Text.Json;
using Discussion.Models;
using Discussion.Services;

namespace PersonaTraitTest;

internal record TestErgebnis(
    string Parameter,
    string Variante,
    string Wert,
    string Alter,
    string Geschlecht,
    string Bildungsstand,
    string PolitischeAusrichtung,
    string WeitereMerkmale,
    string Antwort,
    double DauerSekunden);

internal static class Program
{
    private const string Zuckerfrage = "Sollen zuckerhaltige Snacks verboten werden?";

    private static async Task Main()
    {
        var settings = ConfigService.Laden();

        if (string.IsNullOrWhiteSpace(settings.Verbindung.BasisUrl))
        {
            Console.WriteLine("Kein KI-Endpunkt konfiguriert. Bitte zuerst die Discussion-App starten und im");
            Console.WriteLine("Konfiguration-Tab eine Endpunkt-URL und ein Modell für Persona A hinterlegen.");
            return;
        }

        string modell = settings.Verbindung.ModellA;
        if (string.IsNullOrWhiteSpace(modell))
        {
            Console.WriteLine("Kein Modell für Persona A konfiguriert. Abbruch.");
            return;
        }

        Console.WriteLine($"Endpunkt: {settings.Verbindung.BasisUrl} ({settings.Verbindung.Format})");
        Console.WriteLine($"Modell:   {modell}");
        Console.WriteLine($"Frage:    \"{Zuckerfrage}\"");
        Console.WriteLine();

        // Normal-Basiswerte, siehe Test-Parameter-Vorgabe (min / normal / max je Eigenschaft)
        var normal = new PersonaProfil
        {
            Alter = "50",
            Geschlecht = "divers",
            Bildungsstand = "Abitur",
            PolitischeAusrichtung = "unpolitisch",
            Zusatz = ""
        };

        var parameter = new List<(string Name, string Min, string Max, Action<PersonaProfil, string> Setzen)>
        {
            ("Alter", "6", "100", (p, v) => p.Alter = v),
            ("Geschlecht", "männlich", "weiblich", (p, v) => p.Geschlecht = v),
            ("Bildungsstand", "Hauptschule", "Promotion", (p, v) => p.Bildungsstand = v),
            ("Politische Ausrichtung", "links", "rechts", (p, v) => p.PolitischeAusrichtung = v),
            ("Weitere Merkmale", "ruhig bedächtig", "cholerisch, fluchend", (p, v) => p.Zusatz = v),
        };

        var client = new KiClient();
        var ergebnisse = new List<TestErgebnis>();

        foreach (var (name, min, max, setzen) in parameter)
        {
            Console.WriteLine($"=== Parameter: {name} ===");

            foreach (var (variante, wert) in new (string, string?)[] { ("normal", null), ("min", min), ("max", max) })
            {
                var profil = KopiereNormal(normal);
                if (wert != null)
                    setzen(profil, wert);

                var uhr = System.Diagnostics.Stopwatch.StartNew();
                string antwort;
                try
                {
                    antwort = await FrageAsync(client, settings.Verbindung, modell, profil);
                }
                catch (KiVerbindungsFehler ex)
                {
                    antwort = $"[FEHLER] {ex.Message}";
                }
                uhr.Stop();

                var ergebnis = new TestErgebnis(
                    name, variante, wert ?? "(normal)",
                    profil.Alter, profil.Geschlecht, profil.Bildungsstand, profil.PolitischeAusrichtung, profil.Zusatz,
                    antwort, uhr.Elapsed.TotalSeconds);
                ergebnisse.Add(ergebnis);

                Console.WriteLine($"  [{variante,-6}] ({(wert ?? "normal")}) -> {Kuerzen(antwort, 100)}  ({uhr.Elapsed.TotalSeconds:F1}s)");
            }

            Console.WriteLine();
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
            Frage = Zuckerfrage,
            NormalProfil = normal,
            Ergebnisse = ergebnisse
        };

        File.WriteAllText(jsonPfad, JsonSerializer.Serialize(gesamtErgebnis, new JsonSerializerOptions { WriteIndented = true }));

        Console.WriteLine($"Fertig. {ergebnisse.Count} Antworten gesammelt.");
        Console.WriteLine($"Ergebnisse gespeichert unter: {jsonPfad}");
    }

    private static async Task<string> FrageAsync(IKiClient client, KiVerbindung verbindung, string modell, PersonaProfil profil)
    {
        string profilBeschreibung = $"- Alter: {profil.Alter}\n- Geschlecht: {profil.Geschlecht}\n- Bildungsstand: {profil.Bildungsstand}\n- Politische Ausrichtung: {profil.PolitischeAusrichtung}";
        if (!string.IsNullOrWhiteSpace(profil.Zusatz))
            profilBeschreibung += $"\n- Weitere Merkmale: {profil.Zusatz}";

        string system =
$@"Du bist folgende Person:
{profilBeschreibung}

Beantworte die folgende Frage aus der Sicht dieser Person. Deine Wortwahl, dein Argumentationsstil und deine Meinung sollen erkennbar zu diesem Profil passen. Antworte ausschließlich mit deiner Antwort in 2 bis 5 Sätzen, ohne Meta-Kommentare und ohne die Frage zu wiederholen.";

        var nachrichten = new List<ChatMessage>
        {
            new("system", system),
            new("user", Zuckerfrage)
        };

        return await client.SendeAsync(verbindung, modell, nachrichten, CancellationToken.None);
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
