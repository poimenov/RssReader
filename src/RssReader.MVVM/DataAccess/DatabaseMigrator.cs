using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RssReader.MVVM.DataAccess.Interfaces;

namespace RssReader.MVVM.DataAccess;

/// <summary>
/// dotnet ef migrations add InitialCreate -o DataAccess/Migrations
/// </summary>
public class DatabaseMigrator : IDatabaseMigrator
{
    private readonly ILogger<DatabaseMigrator> _logger;
    private Database _database;
    public DatabaseMigrator(ILogger<DatabaseMigrator> logger)
    {
        _logger = logger;
    }
    public void MigrateDatabase()
    {
        var path = Path.Combine(AppSettings.AppDataPath, Database.DB_FILE_NAME);
        if (!File.Exists(path))
        {
            using (_database = new Database())
            {
                _database.Database.Migrate();
            }

            if (_logger != null)
            {
                _logger.LogInformation("Database created");
            }
        }
    }
}
