using Discussion.Models;

namespace Discussion.Services;

public class DiskussionsEngine
{
    private readonly IKiClient _client;

    public event Action<ChatEintrag>? NeuerEintrag;

    public DiskussionsEngine(IKiClient client)
    {
        _client = client;
    }

    public async Task StartenAsync(AppSettings settings, DiskussionsLogger logger, CancellationToken ct)
    {
        var positionA = settings.PositionPersonaA;
        var positionB = positionA.Gegenteil();

        var verlaufA = new List<ChatMessage> { new("system", BaueSystemPrompt(settings.PersonaA, settings.PersonaB, settings.Thema, positionA)) };
        var verlaufB = new List<ChatMessage> { new("system", BaueSystemPrompt(settings.PersonaB, settings.PersonaA, settings.Thema, positionB)) };

        string? letzterTextB = null;

        for (int runde = 1; runde <= settings.MaxTexteJePersona; runde++)
        {
            ct.ThrowIfCancellationRequested();

            string eingabeA = letzterTextB is null
                ? $"Eröffne die Diskussion zum Thema \"{settings.Thema}\". Formuliere deine Eröffnungsthese entsprechend deiner festgelegten Position."
                : $"Dein Gesprächspartner antwortet:\n\"{letzterTextB}\"\n\nEntgegne darauf und verteidige oder verfeinere deine eigene Position.";
            verlaufA.Add(new ChatMessage("user", eingabeA));
            string textA = await _client.SendeAsync(settings.Verbindung, settings.Verbindung.ModellA, verlaufA, ct);
            verlaufA.Add(new ChatMessage("assistant", textA));
            var eintragA = new ChatEintrag(DateTime.Now, Sprecher.PersonaA, textA);
            logger.Schreiben(eintragA);
            NeuerEintrag?.Invoke(eintragA);

            ct.ThrowIfCancellationRequested();

            string eingabeB = runde == 1
                ? $"Dein Gesprächspartner eröffnet die Diskussion zum Thema \"{settings.Thema}\" mit folgender These:\n\"{textA}\"\n\nWiderlege diese These entsprechend deiner festgelegten Position."
                : $"Dein Gesprächspartner antwortet:\n\"{textA}\"\n\nEntgegne darauf und verteidige oder verfeinere deine eigene Position.";
            verlaufB.Add(new ChatMessage("user", eingabeB));
            string textB = await _client.SendeAsync(settings.Verbindung, settings.Verbindung.ModellB, verlaufB, ct);
            verlaufB.Add(new ChatMessage("assistant", textB));
            var eintragB = new ChatEintrag(DateTime.Now, Sprecher.PersonaB, textB);
            logger.Schreiben(eintragB);
            NeuerEintrag?.Invoke(eintragB);

            letzterTextB = textB;
        }
    }

    private static string BeschreibeProfil(PersonaProfil p)
    {
        string profil = $"- Alter: {p.Alter}\n- Geschlecht: {p.Geschlecht}\n- Bildungsstand: {p.Bildungsstand}\n- Politische Ausrichtung: {p.PolitischeAusrichtung}";
        if (!string.IsNullOrWhiteSpace(p.Zusatz))
            profil += $"\n- Weitere Merkmale: {p.Zusatz}";
        return profil;
    }

    private static string BaueSystemPrompt(PersonaProfil eigenes, PersonaProfil gegenueber, string thema, Position eigenePosition)
    {
        string positionText = eigenePosition == Position.Pro
            ? "PRO: Du stimmst der These/dem Thema zu, befürwortest sie und argumentierst dafür."
            : "CONTRA: Du lehnst die These/das Thema ab, widersprichst ihr und argumentierst dagegen.";

        return
$@"Du bist Teilnehmer einer Diskussion und hast folgendes Profil:
{BeschreibeProfil(eigenes)}

Über deinen Gesprächspartner weißt du ausschließlich das folgende Profil - mehr Informationen über ihn hast du nicht:
{BeschreibeProfil(gegenueber)}

Thema der Diskussion: ""{thema}""

Deine Position in dieser Diskussion: {positionText} Diese Position ist bindend und muss über die gesamte Diskussion hinweg konsequent beibehalten werden, unabhängig davon, wie überzeugend die Gegenargumente sind.

Du weißt nicht, wer oder was dein Gesprächspartner sonst noch ist (z.B. ob es sich um eine KI handelt) - urteile ausschließlich anhand des oben genannten Profils. Deine Wortwahl und Argumentationsweise sollen zu deinem eigenen Profil passen (Alter, Bildungsstand, politische Ausrichtung und Geschlecht dürfen erkennbar prägen, WIE du argumentierst, nicht OB - deine Position bleibt wie oben festgelegt). Passe deine Argumentationsstrategie zusätzlich an das Profil deines Gesprächspartners an, um ihn möglichst wirkungsvoll zu überzeugen bzw. seine Position zu widerlegen. Du kennst den gesamten bisherigen Gesprächsverlauf dieser Diskussion - wiederhole keine Argumente, die du oder dein Gesprächspartner bereits gebracht habt, und widersprich dir nicht selbst. Argumentiere sachlich, aber pointiert, gehe konkret auf die zuletzt genannten Aussagen deines Gesprächspartners ein und versuche, dessen Argumente zu entkräften. Antworte ausschließlich mit deinem eigenen Diskussionsbeitrag in 2 bis 5 Sätzen, ohne Meta-Kommentare, Regieanweisungen oder Sprecherkennzeichnung.";
    }
}
