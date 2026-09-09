using ElWatcher.Models;

namespace ElWatcher.Interfaces;

public interface IEnergiDataService
{
    Task<IEnumerable<Co2Observationer>> GetCurrentCo2EmissionAsync();
}