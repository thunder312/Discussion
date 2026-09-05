using System.IO;
using System.Text.Json;
using Discussion.Models;

namespace Discussion.Services;

public static class ConfigService
{
    private static readonly string Pfad = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Discussion", "config.json");

    public static AppSettings Laden()
    {
        try
        {
            if (File.Exists(Pfad))
            {
                var json = File.ReadAllText(Pfad);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                    return settings;
            }
        }
        catch
        {
            // Konfigurationsdatei fehlerhaft -> Standardwerte verwenden
        }
        return AppSettings.Standard();
    }

    public static void Speichern(AppSettings settings)
    {
        var ordner = Path.GetDirectoryName(Pfad)!;
        Directory.CreateDirectory(ordner);
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Pfad, json);
    }
}
