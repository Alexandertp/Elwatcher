using System.Diagnostics;
using ElWatcher.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ElWatcher.Models;

namespace ElWatcher.Controllers;

public class HomeController : Controller
{
    private readonly IDBLagring _dbLagring;

    public HomeController(IDBLagring dbLagring)
    {
        _dbLagring = dbLagring;
    }
    
    public async Task<IActionResult> Index()
    {
        var data = await _dbLagring.HentAlleCo2Emission();
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