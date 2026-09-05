using System.IO;
using Discussion.Models;

namespace Discussion.Services;

public class DiskussionsLogger : IDisposable
{
    private readonly StreamWriter _writer;
    public string Dateipfad { get; }

    public DiskussionsLogger(string thema)
    {
        var ordner = Path.Combine(AppContext.BaseDirectory, "Logs");
        Directory.CreateDirectory(ordner);
        Dateipfad = Path.Combine(ordner, $"Diskussion_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        _writer = new StreamWriter(Dateipfad, append: true) { AutoFlush = true };
        _writer.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm}h Diskussion gestartet. Thema: {thema}");
    }

    public void Schreiben(ChatEintrag eintrag)
    {
        _writer.WriteLine($"{eintrag.Zeitpunkt:dd.MM.yyyy HH:mm}h {eintrag.AnzeigeName}: {eintrag.Text}");
    }

    public void Dispose() => _writer.Dispose();
}
