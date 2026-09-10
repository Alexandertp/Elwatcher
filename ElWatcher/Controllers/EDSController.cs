using ElWatcher.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ElWatcher.Controllers;

[ApiController]
public class EDSController : ControllerBase
{
    private readonly IEnergiDataService _energiDataService;
    private readonly IDBLagring _dbLagring;
    public EDSController(IEnergiDataService energiDataService, IDBLagring dbLagring)
    {
        _energiDataService =  energiDataService;
        _dbLagring = dbLagring;
    }

    [HttpGet]
    [Route("Co2Emission")]
    public async Task<IActionResult> GetCo2Emission()
    {
        var data = await _energiDataService.GetCurrentCo2EmissionAsync();
        return Ok(data);
    }

    [HttpGet]
    [Route("Co2Emission/HentOgGem")]
    public async Task<IActionResult> HentOgGemCo2Emission()
    {
        var data = await _energiDataService.GetCurrentCo2EmissionAsync();
        await _dbLagring.GemCo2Emission(data);
        return Ok();
    }
}