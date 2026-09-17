using System.Diagnostics;
using ElWatcher.Interfaces;
using ElWatcher.Models;
using Microsoft.Data.SqlClient;

namespace ElWatcher.Services;

public class DBLagringRepo : IDBLagring
{
    private string dataSovs;
    private string userID;
    private string password;
    private string dbCatalog;
    private ILogger<DBLagringRepo> _logger;
    public DBLagringRepo(IConfiguration configuration, ILogger<DBLagringRepo> logger)
    {
        dataSovs = configuration["DBCredentials:DataSource"] ?? "localhost";
        userID = configuration["DBCredentials:UserID"] ?? "root";
        password = configuration["DBCredentials:Password"] ?? "root";
        dbCatalog = configuration["DBCredentials:InitialCatalog"] ?? "elwatcher";
        _logger = logger;
    }
    private async Task<SqlConnection?> ForbindTilDatabase()
    {
        
        var strBuilder = new SqlConnectionStringBuilder
        {
            DataSource = dataSovs,
            UserID = userID,
            Password = password,
            InitialCatalog = dbCatalog,
            TrustServerCertificate = true

        };

        var connectionString = strBuilder.ConnectionString;
        var connection = new SqlConnection(connectionString);

        try
        {
            Console.WriteLine("Attempting connection to " + connectionString);
            await connection.OpenAsync();
            Console.WriteLine("Connection attempted");
            return connection;
            
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Databaseforbindelse til {DataSource} fejlede", dataSovs);
            await connection.DisposeAsync();
            return null;
        }

       
    }

    public async Task GemCo2Emission(IEnumerable<Co2Observation> data)
    {
        SqlConnection connection = await ForbindTilDatabase();

        if (connection == null)
        {
            throw new InvalidOperationException("Kunne ikke oprette databaseforbindelse.");
        }

        var vaerdier = new List<string>();
        await using var command = new SqlCommand{ Connection = connection };

        var index = 0;
        foreach (var obs in data)
        {
            vaerdier.Add($"(@EmissionValue{index}, @Omraade{index}, @Tidspunkt{index})");
            command.Parameters.AddWithValue($"@EmissionValue{index}", obs.CO2Emission);
            command.Parameters.AddWithValue($"@Omraade{index}", obs.PriceArea);
            command.Parameters.AddWithValue($"@Tidspunkt{index}", obs.Minutes5DK);
            index++;
        }

        foreach (var parameter in command.Parameters)
        {
            Debug.WriteLine(parameter.ToString());
        }
        command.CommandText = "INSERT INTO Co2Emission (EmissionValue, Omraade, Tidspunkt) VALUES "
            + string.Join(", ", vaerdier);

        

        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<Co2Observation>> HentAlleCo2Emission()
    {
        await using var connection = await ForbindTilDatabase();
        
        var resultater = new List<Co2Observation>();

        var command = new SqlCommand(
            "SELECT EmissionValue, Omraade, Tidspunkt FROM Co2Emission ORDER BY Tidspunkt DESC",
            connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            resultater.Add(new Co2Observation
            {
                CO2Emission = reader.GetDouble(0),
                PriceArea = reader.GetString(1),
                Minutes5DK = reader.GetDateTime(2),
            });
        }
        return resultater;
    }
    public async Task<IEnumerable<Co2Observation>?> HentTop6Co2Emission()
    {
        await using var connection = await ForbindTilDatabase();
        if (connection == null)
        {
            throw new InvalidOperationException("Kunne ikke oprette databaseforbindelse.");
        }

        var resultater = new List<Co2Observation>();

        var command = new SqlCommand(
            "SELECT TOP 6 EmissionValue, Omraade, Tidspunkt FROM Co2Emission ORDER BY Tidspunkt DESC",
            connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            resultater.Add(new Co2Observation
            {
                CO2Emission = reader.GetDouble(0),
                PriceArea = reader.GetString(1),
                Minutes5DK = reader.GetDateTime(2),
            });
        }

        return resultater;
    }
}