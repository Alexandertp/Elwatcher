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
    public DBLagringRepo(IConfiguration configuration)
    {
        dataSovs = configuration["DBCredentials:DataSource"] ?? "localhost";
        userID = configuration["DBCredentials:UserID"] ?? "root";
        password = configuration["DBCredentials:Password"] ?? "root";
        dbCatalog = configuration["DBCredentials:InitialCatalog"] ?? "elwatcher";
        
    }
    private async Task<SqlConnection> ForbindTilDatabase()
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

        try
        {
            Console.WriteLine("Attempting connection to " + connectionString);
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            Console.WriteLine("Connection attempted");
            return connection;
            
        }
        catch (Exception e)
        {
            Console.WriteLine("connection failed HER ER FEJLEN NEDENUNDER HELST");
            Console.WriteLine(e.ToString());
        }

        return null;
    }

    public async Task GemCo2Emission(IEnumerable<Co2Observation> data)
    {
        SqlConnection connection = await ForbindTilDatabase();

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
        
        command.CommandText = "INSERT INTO Co2Emission (EmissionValue, Omraade, Tidspunkt) VALUES "
            + string.Join(", ", vaerdier);
        
        await command.ExecuteNonQueryAsync();
    }
}