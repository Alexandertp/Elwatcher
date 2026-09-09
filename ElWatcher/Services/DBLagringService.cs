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
    private async Task ForbindTilDatabase()
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
            Console.WriteLine("\nQuery data example:");
            Console.WriteLine("=========================================\n");

            await connection.OpenAsync();

            var sql = "SELECT smiley,test FROM test";
            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Console.WriteLine("{0} {1}", reader.GetString(0), reader.GetString(1));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public void GemCo2Emission(IEnumerable<Co2Observationer> data)
    {
        
    }
}