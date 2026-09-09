using System.Text.Json;
using ElWatcher.Interfaces;
using ElWatcher.Models;

namespace ElWatcher.Services;

public class EnergiDataServiceClient : IEnergiDataService
{
    private readonly HttpClient _httpClient;

    public EnergiDataServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://api.energidataservice.dk/dataset/");
    }

    public async Task<IEnumerable<Co2Observationer>> GetCurrentCo2EmissionAsync()
    {
        var response = await _httpClient.GetAsync("CO2Emis?start=now-PT15M");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<Co2DataSvar>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (result?.Records is null)
        {
            return Enumerable.Empty<Co2Observationer>();
        }
        // Looper gennem Records listen og ligger den i en IEnumerable 
        return result.Records.Select(r => new Co2Observationer
        {
            Tidspunkt = r.Tidspunkt,
            EmissionValue = r.EmissionValue,
            Omraade = r.Omraade
        });
    }
/// <summary>
/// Indeholder en lista af Co2DataObservationer som vi henter fra Energi Data Service hjemmesiden
/// </summary>
    private class Co2DataSvar
    {
        public List<Models.Co2Observationer> Records { get; set; } = new();
    }
}