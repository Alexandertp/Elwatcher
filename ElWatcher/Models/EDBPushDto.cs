namespace ElWatcher.Models;

/// <summary>
/// Den ene måling, som en browser modtager gennem SignalR-beskeden "NyeMaalinger".
/// Tre felter - ikke ét mere.
/// </summary>
/// <param name="PriceArea">Viser område fra EDB</param>
/// <param name="CO2Emission">Henter Co2 Emission value fra EDB</param>
/// <param name="Minutes5DK">Viser tidspunkt hentet EDB</param>
public record EDBPushDto(
    double? CO2Emission,
    string? PriceArea,
    DateTime? Minutes5DK);
