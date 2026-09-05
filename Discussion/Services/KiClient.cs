using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Discussion.Models;

namespace Discussion.Services;

public class KiClient : IKiClient
{
    private static readonly HttpClient Http = new();

    public async Task<string> SendeAsync(KiVerbindung v, string modell, IReadOnlyList<ChatMessage> nachrichten, CancellationToken ct)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linked.CancelAfter(TimeSpan.FromSeconds(Math.Max(5, v.TimeoutSekunden)));

        object payload = v.Format == ApiFormat.Ollama
            ? new
            {
                model = modell,
                messages = nachrichten.Select(m => new { role = m.Role, content = m.Content }),
                stream = false,
                options = new { temperature = v.Temperature }
            }
            : new
            {
                model = modell,
                messages = nachrichten.Select(m => new { role = m.Role, content = m.Content }),
                temperature = v.Temperature
            };

        using var request = new HttpRequestMessage(HttpMethod.Post, v.BasisUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        if (!string.IsNullOrWhiteSpace(v.ApiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", v.ApiKey);

        HttpResponseMessage response;
        string body;
        try
        {
            response = await Http.SendAsync(request, linked.Token);
            body = await response.Content.ReadAsStringAsync(ct);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new KiVerbindungsFehler($"Zeitüberschreitung nach {v.TimeoutSekunden}s beim Aufruf von {v.BasisUrl}.");
        }
        catch (HttpRequestException ex)
        {
            throw new KiVerbindungsFehler($"KI unter {v.BasisUrl} nicht erreichbar: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
            throw new KiVerbindungsFehler($"HTTP {(int)response.StatusCode} von {v.BasisUrl}: {Kuerzen(body)}");

        try
        {
            using var doc = JsonDocument.Parse(body);
            string? inhalt = v.Format == ApiFormat.Ollama
                ? doc.RootElement.GetProperty("message").GetProperty("content").GetString()
                : doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            if (string.IsNullOrWhiteSpace(inhalt))
                throw new KiVerbindungsFehler("Die KI hat eine leere Antwort geliefert.");
            return inhalt.Trim();
        }
        catch (Exception ex) when (ex is not KiVerbindungsFehler)
        {
            throw new KiVerbindungsFehler($"Antwort von {v.BasisUrl} konnte nicht gelesen werden: {ex.Message} (Rohdaten: {Kuerzen(body)})");
        }
    }

    public async Task<List<string>> ListeModelleAsync(KiVerbindung v, CancellationToken ct)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linked.CancelAfter(TimeSpan.FromSeconds(Math.Max(5, v.TimeoutSekunden)));

        string origin;
        try
        {
            var uri = new Uri(v.BasisUrl);
            origin = $"{uri.Scheme}://{uri.Authority}";
        }
        catch (Exception ex)
        {
            throw new KiVerbindungsFehler($"Endpunkt-URL '{v.BasisUrl}' ist ungültig: {ex.Message}");
        }

        string url = v.Format == ApiFormat.Ollama ? $"{origin}/api/tags" : $"{origin}/v1/models";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrWhiteSpace(v.ApiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", v.ApiKey);

        HttpResponseMessage response;
        string body;
        try
        {
            response = await Http.SendAsync(request, linked.Token);
            body = await response.Content.ReadAsStringAsync(ct);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new KiVerbindungsFehler($"Zeitüberschreitung beim Abfragen der Modell-Liste unter {url}.");
        }
        catch (HttpRequestException ex)
        {
            throw new KiVerbindungsFehler($"Modell-Liste unter {url} nicht erreichbar: {ex.Message}");
        }

        if (!response.IsSuccessStatusCode)
            throw new KiVerbindungsFehler($"HTTP {(int)response.StatusCode} von {url}: {Kuerzen(body)}");

        try
        {
            using var doc = JsonDocument.Parse(body);
            var namen = new List<string>();
            if (v.Format == ApiFormat.Ollama)
            {
                foreach (var m in doc.RootElement.GetProperty("models").EnumerateArray())
                {
                    var name = m.TryGetProperty("model", out var mp) ? mp.GetString() : m.GetProperty("name").GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                        namen.Add(name!);
                }
            }
            else
            {
                foreach (var m in doc.RootElement.GetProperty("data").EnumerateArray())
                {
                    var name = m.GetProperty("id").GetString();
                    if (!string.IsNullOrWhiteSpace(name))
                        namen.Add(name!);
                }
            }
            namen.Sort(StringComparer.OrdinalIgnoreCase);
            return namen;
        }
        catch (Exception ex) when (ex is not KiVerbindungsFehler)
        {
            throw new KiVerbindungsFehler($"Modell-Liste von {url} konnte nicht gelesen werden: {ex.Message} (Rohdaten: {Kuerzen(body)})");
        }
    }

    private static string Kuerzen(string s) => s.Length > 300 ? s[..300] + "..." : s;
}
