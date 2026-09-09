using ElWatcher.Models;
using Microsoft.Data.SqlClient;

namespace ElWatcher.Services;

public class DBLagringService
{
    private string dataSovs;
    private string userID;
    private string password;
    private string dbCatalog;
    public DBLagringService(IConfiguration configuration)
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
            await using var connection = new SqlConnection(connectionString);
            
            await connection.OpenAsync();
            return connection;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        return null;
    }

    public async Task GemCo2Emission(IEnumerable<Co2Observationer> data, SqlConnection connection)
    {
        var sql = "SELECT * FROM Co2Emission";
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
    }
}