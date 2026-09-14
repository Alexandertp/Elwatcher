using System.Diagnostics;
using ElWatcher.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ElWatcher.Models;

namespace ElWatcher.Controllers;

public class HomeController : Controller
{
    private readonly IDBLagring _dbLagring;
    private readonly IEnergiDataService _energiDataService;

    public HomeController(IDBLagring dbLagring, IEnergiDataService energiDataService)
    {
        _dbLagring = dbLagring;
        _energiDataService = energiDataService;
    }
    
    public async Task<IActionResult> Index()
    {
        await _dbLagring.GemCo2Emission(await _energiDataService.GetCurrentCo2EmissionAsync());
        var data = await _dbLagring.HentTop6Co2Emission();
        return View(data);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}