namespace Discussion.Models;

public record ChatEintrag(DateTime Zeitpunkt, Sprecher Sprecher, string AnzeigeName, string Text)
{
    public string ZeitAnzeige => Zeitpunkt.ToString("HH:mm");
}
