using Discussion.Models;

namespace Discussion.Services;

public interface IKiClient
{
    Task<string> SendeAsync(KiVerbindung verbindung, string modell, IReadOnlyList<ChatMessage> nachrichten, CancellationToken ct);

    /// <summary>
    /// Wie <see cref="SendeAsync"/>, aber ohne Gesamtzeit-Limit (nutzt Streaming und einen
    /// Leerlauf-Wächter: bricht nur ab, wenn eine Zeit lang gar keine neuen Daten mehr ankommen,
    /// nicht nach einer festen Gesamtdauer). Gedacht für Aufrufe, die absehbar lange dauern
    /// (z.B. den Schiedsrichter mit einem langen Transkript), unabhängig vom konfigurierten
    /// Timeout für einzelne Diskussions-Runden.
    /// </summary>
    Task<string> SendeLangeAsync(KiVerbindung verbindung, string modell, IReadOnlyList<ChatMessage> nachrichten, CancellationToken ct);

    Task<List<string>> ListeModelleAsync(KiVerbindung verbindung, CancellationToken ct);
}
