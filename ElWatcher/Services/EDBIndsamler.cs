using System.Diagnostics.Eventing.Reader;
using ElWatcher.Hubs;
using ElWatcher.Interfaces;
using ElWatcher.Models;
using Microsoft.AspNetCore.SignalR;

namespace ElWatcher.Services;

public class EDBIndsamler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EDBIndsamler> _logger;
    private readonly IHubContext<EDBHub> _hub;
    private const int StandardIntervalSekunder = 60;
    private readonly TimeSpan _interval;

    public EDBIndsamler(
        IServiceScopeFactory scopeFactory,
        ILogger<EDBIndsamler> logger,
        IConfiguration configuration,
        IHubContext<EDBHub> hub)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _hub = hub;

        var sekunder = configuration.GetValue<int>(
            "EDB:IndsamlingsIntervalSekunder", StandardIntervalSekunder);

        if (sekunder <= 0)
        {
            _logger.LogWarning(
                "Vejr:IndsamingsIntervalSekunder er sat til {Ugyldig} - bruger {Standard} sekunder i stedet",
                sekunder, StandardIntervalSekunder);
            
            sekunder = StandardIntervalSekunder;
        }
        _interval = TimeSpan.FromSeconds(sekunder);
    }

    /// <summary>
    /// Kører indsamlingsløkken, indtil appen lukkes ned.
    /// </summary>
    /// <param name="stoppingToken">Token der signalerer at appen er ved at lukke</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Baggrundsindsamling startet - henter EDB-data hvert {Sekunder}. sekund WOOP WOOP!",
            _interval.TotalSeconds);
        
        using var timer = new PeriodicTimer(_interval);
        try
        {
            do
            {
                await IndsamlEnGangAsync(stoppingToken);
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Baggrundsindsamling stoppet - appen lukker ned (lige som klubben efter Anne Sofie tager hjem");
        }
    }

    /// <summary>
    /// Udfører én indsamling: henter fra DMI og gemmer i databasen.
    /// </summary>
    /// <param name="stoppingToken">Token der signalerer at appen er ved at lukke</param>
    private async Task IndsamlEnGangAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var historik = scope.ServiceProvider.GetRequiredService<IDBLagring>();
            var edbService = scope.ServiceProvider.GetRequiredService<IEnergiDataService>();

            var observationer = (await historik.HentTop6Co2Emission()).ToList();

            if (observationer.Count() == 0)
            {
                _logger.LogWarning("Baggrundsindsamling: DMI svarede ikke - intet gemt (Danskegulv lukket!)");
                return;
            }

            await historik.GemCo2Emission(observationer);

            _logger.LogInformation(
                "Baggrundsindsamling gennemført: {Antal} observationer gemt", observationer.Count());

            await SendTilBrowserneAsync(observationer, stoppingToken);
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Baggrundsindsamling fejlede - prøver igen om {Sekunder} sekunder",
                _interval.TotalSeconds);
        }
    }

    /// <summary>
    /// Sender de netop indsamlede målinger til alle tilsluttede browsere.
    /// </summary>
    /// <param name="observationer">Observationerne fra denne runde</param>
    /// <param name="stoppeToken">Token der signalerer at appen er ved at lukke</param>
    private async Task SendTilBrowserneAsync(
        IReadOnlyList<Co2Observation> observationer,
        CancellationToken stoppeToken)
    {
        var dtoer = observationer.Select(o => new EDBPushDto(
            o.CO2Emission,
            o.PriceArea ?? string.Empty,
            o.Minutes5DK = DateTime.UtcNow))
            .ToList();
        
        await _hub.Clients.All.SendAsync("NyeMaalinger", dtoer, stoppeToken);
        
        _logger.LogInformation(
            "Sendte {Antal} målinger til alle tilsluttede browsere", dtoer.Count());
    }
}