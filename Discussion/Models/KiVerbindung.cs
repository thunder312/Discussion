namespace Discussion.Models;

public class KiVerbindung
{
    public ApiFormat Format { get; set; } = ApiFormat.Ollama;
    public string BasisUrl { get; set; } = "http://192.168.188.181:11434/api/chat";
    public string ApiKey { get; set; } = "";
    public string ModellA { get; set; } = "mistral-small3.2:latest";
    public string ModellB { get; set; } = "hf.co/mayflowergmbh/Llama-3-SauerkrautLM-8b-Instruct-GGUF:Q4_K_M";
    public double Temperature { get; set; } = 0.8;
    public int TimeoutSekunden { get; set; } = 120;
}
