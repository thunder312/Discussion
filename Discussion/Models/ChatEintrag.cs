namespace Discussion.Models;

public record ChatEintrag(DateTime Zeitpunkt, Sprecher Sprecher, string Text)
{
    public string SprecherName => Sprecher switch
    {
        Sprecher.PersonaA => "Persona A",
        Sprecher.PersonaB => "Persona B",
        _ => "System"
    };

    public string ZeitAnzeige => Zeitpunkt.ToString("HH:mm");
}
