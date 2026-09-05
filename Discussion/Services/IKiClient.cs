using Discussion.Models;

namespace Discussion.Services;

public interface IKiClient
{
    Task<string> SendeAsync(KiVerbindung verbindung, string modell, IReadOnlyList<ChatMessage> nachrichten, CancellationToken ct);
    Task<List<string>> ListeModelleAsync(KiVerbindung verbindung, CancellationToken ct);
}
