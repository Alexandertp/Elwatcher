using Microsoft.AspNetCore.SignalR;

namespace ElWatcher.Hubs;

/// <summary>
/// Hubben er endepunktet, browserne forbinder til på /vejrhub. Den svarer med en
/// velkomstbesked til den nye klient og fortæller alle klienter, hvor mange der
/// er tilsluttede lige nu.
/// </summary>
public class EDBHub : Hub
{
    private readonly ILogger<EDBHub> _logger;
    private static int _tilsluttede;

    /// <summary>
    /// Constructor med Dependency Injection.
    /// </summary>
    /// <param name="logger">Logger til diagnostik</param>
    public EDBHub(ILogger<EDBHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Kaldes af SignalR, hver gang en browser har forbundet færdigt.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Kliient forbundet: {ForbindelsesId}", Context.ConnectionId);
        await Clients.Caller.SendAsync("Velkommen", "Forbundet til vejr-hubben. Du får nu opdateringer automatisk.");
        
        var antal = Interlocked.Increment(ref _tilsluttede);

        await Clients.All.SendAsync("TilsluttedeOpdateret", antal);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Kaldes af SignalR, når en browser har afbrudt forbindelsen.
    /// </summary>
    /// <param name="exception">Fejlen der afbrød forbindelsen, eller null ved en pæn afsked</param>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception == null)
        {
            _logger.LogInformation("Klient afbrudt: {ForbindelseId}", Context.ConnectionId);
        }
        else
        {
            _logger.LogWarning(exception, "Klient afbrudt uventet: {ForbindelsesId}", Context.ConnectionId);
        }
        var antal = Interlocked.Decrement(ref _tilsluttede);

        await Clients.All.SendAsync("TilsluttedeOpdateret", antal);

        await base.OnDisconnectedAsync(exception);
    }
}