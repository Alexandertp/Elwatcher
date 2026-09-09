namespace ElWatcher.Models;

public class Co2Observationer
{
    public DateTime Tidspunkt { get; set; }
    public double EmissionValue { get; set; }
    public string Omraade { get; set; } = string.Empty;
}