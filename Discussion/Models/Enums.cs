namespace Discussion.Models;

public enum ApiFormat
{
    Ollama,
    OpenAiKompatibel
}

public enum Sprecher
{
    PersonaA,
    PersonaB,
    System
}

public enum Position
{
    Pro,
    Contra
}

public static class PositionErweiterungen
{
    public static Position Gegenteil(this Position p) => p == Position.Pro ? Position.Contra : Position.Pro;
}
