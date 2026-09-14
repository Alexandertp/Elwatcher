using ElWatcher.Models;

namespace ElWatcher.Interfaces;

public interface IDBLagring
{
    Task GemCo2Emission(IEnumerable<Co2Observation> data);
    Task<IEnumerable<Co2Observation>> HentAlleCo2Emission();
    Task<IEnumerable<Co2Observation>> HentTop6Co2Emission();
}