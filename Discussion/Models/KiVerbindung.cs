namespace Discussion.Models;

public class KiVerbindung
{
    public ApiFormat Format { get; set; } = ApiFormat.Ollama;
    public string BasisUrl { get; set; } = "";
    public string ApiKey { get; set; } = "";
    public string ModellA { get; set; } = "";
    public string ModellB { get; set; } = "";
    public string ModellSchiedsrichter { get; set; } = "";
    public double Temperature { get; set; } = 0.8;
    public int TimeoutSekunden { get; set; } = 300;
}
