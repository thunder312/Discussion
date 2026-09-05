namespace Discussion.Services;

public class KiVerbindungsFehler : Exception
{
    public KiVerbindungsFehler(string message) : base(message)
    {
    }
}
