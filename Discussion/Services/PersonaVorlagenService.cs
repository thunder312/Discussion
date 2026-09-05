using System.IO;
using System.Text.Json;
using Discussion.Models;

namespace Discussion.Services;

public static class PersonaVorlagenService
{
    public static string StandardOrdner { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Discussion", "Personas");

    public static List<string> Auflisten(string ordner)
    {
        if (!Directory.Exists(ordner))
            return new List<string>();

        return Directory.GetFiles(ordner, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name!)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static void Speichern(PersonaProfil profil, string ordner)
    {
        Directory.CreateDirectory(ordner);
        var json = JsonSerializer.Serialize(profil, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(DateiPfad(ordner, profil.Name), json);
    }

    public static PersonaProfil Laden(string name, string ordner)
    {
        var json = File.ReadAllText(DateiPfad(ordner, name));
        return JsonSerializer.Deserialize<PersonaProfil>(json) ?? new PersonaProfil();
    }

    private static string DateiPfad(string ordner, string name) =>
        Path.Combine(ordner, $"{SanitisiereDateiname(name)}.json");

    private static string SanitisiereDateiname(string name)
    {
        var ungueltig = Path.GetInvalidFileNameChars();
        var bereinigt = new string(name.Select(c => ungueltig.Contains(c) ? '_' : c).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(bereinigt) ? "Unbenannt" : bereinigt;
    }
}
