namespace Discussion.Models;

public class AppSettings
{
    public KiVerbindung Verbindung { get; set; } = new();
    public PersonaProfil PersonaA { get; set; } = new();
    public PersonaProfil PersonaB { get; set; } = new();
    public string Thema { get; set; } = "";
    public int MaxTexteJePersona { get; set; } = 5;
    public Position PositionPersonaA { get; set; } = Position.Pro;
    public bool SchiedsrichterAktiv { get; set; } = true;
    public Pfade Pfade { get; set; } = new();

    public static AppSettings Standard() => new()
    {
        Verbindung = new KiVerbindung(),
        PositionPersonaA = Position.Pro,
        SchiedsrichterAktiv = true,
        Pfade = new Pfade(),
        PersonaA = new PersonaProfil
        {
            Name = "",
            Alter = "34",
            Geschlecht = "weiblich",
            Bildungsstand = "Studium/Akademiker",
            PolitischeAusrichtung = "Mitte-links",
            Zusatz = ""
        },
        PersonaB = new PersonaProfil
        {
            Name = "",
            Alter = "52",
            Geschlecht = "männlich",
            Bildungsstand = "Realschulabschluss",
            PolitischeAusrichtung = "Mitte-rechts",
            Zusatz = ""
        },
        Thema = "Soll Zucker verboten werden?",
        MaxTexteJePersona = 5
    };
}
