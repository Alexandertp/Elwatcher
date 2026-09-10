using ElWatcher.Models;

namespace ElWatcher.Interfaces;

public interface IDBLagring
{
    Task GemCo2Emission(IEnumerable<Co2Observationer> data);
}