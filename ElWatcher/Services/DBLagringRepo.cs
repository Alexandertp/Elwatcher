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
        dbCatalog = configuration["DBCredentials:DBCatalog"] ?? "elwatcher";
        
    }
    private async Task<SqlConnection> ForbindTilDatabase()
    {
        
        var strBuilder = new SqlConnectionStringBuilder
        {
            DataSource = dataSovs,
            UserID = userID,
            Password = password,
            InitialCatalog = dbCatalog
        };

        var connectionString = strBuilder.ConnectionString;

        try
        {
            var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return connection;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        return null;
    }

    public async Task GemCo2Emission(IEnumerable<Co2Observationer> data)
    {
        SqlConnection connection = await ForbindTilDatabase();

        string del1 = "INSERT INTO Co2Emission (EmissionValue, Omraade, Tidspunkt) VALUES";
        string del2 = "";
        foreach (var Co2Observationer in data)
        {
            del2 += $"('{Co2Observationer.EmissionValue}','{Co2Observationer.Omraade}','{Co2Observationer.Tidspunkt})',";
        }
        del2.TrimEnd(',');
        var stringSql = del1 + del2 + ";";
        await using var command = new SqlCommand(stringSql, connection);
        await using var reader = await command.ExecuteReaderAsync();
    }
}