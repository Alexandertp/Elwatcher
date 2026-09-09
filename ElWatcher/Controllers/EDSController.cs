using ElWatcher.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ElWatcher.Controllers;

[ApiController]
public class EDSController : ControllerBase
{
    private readonly IEnergiDataService _energiDataService;

    public EDSController(IEnergiDataService energiDataService)
    {
        _energiDataService =  energiDataService;
    }

    [HttpGet]
    [Route("Co2Emission")]
    public async Task<IActionResult> GetCo2Emission()
    {
        var data = await _energiDataService.GetCurrentCo2EmissionAsync();
        return Ok(data);
    }
}