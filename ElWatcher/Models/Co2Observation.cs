namespace ElWatcher.Models;

public class Co2Observation
{
    public DateTime Minutes5DK { get; set; }
    public double CO2Emission { get; set; }
    public string PriceArea { get; set; } = string.Empty;
}