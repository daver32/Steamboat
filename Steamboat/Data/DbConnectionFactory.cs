using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Steamboat.Data
{
    internal class DbConnectionFactory
    {
        private readonly IConfiguration _configuration;
        private readonly DbStructureIntializer _structureIntializer;

        public DbConnectionFactory(IConfiguration configuration, DbStructureIntializer structureIntializer)
        {
            _configuration = configuration;
            _structureIntializer = structureIntializer;
        }

        public async Task<DbConnection> CreateAsync(bool useInMemoryDb = false)
        {
            var dataSource = DetermineDataSource(useInMemoryDb);
            CreatePathIfNeeded(dataSource);
            
            var connection = new SQLiteConnection(BuildConnectionString(dataSource));
            await _structureIntializer.EnsureCreatedAndUpToDateAsync(connection);

            return connection;
        }

        private string DetermineDataSource(bool useInMemoryDb)
        {
            if (useInMemoryDb)
            {
                return ":memory:";
            }
            
            return _configuration.GetValue<string?>("DbPath", null) ?? ConfigDefaults.DatabasePath;
        }

        private string BuildConnectionString(string dataSource)
        {
            var builder = new SQLiteConnectionStringBuilder();
            builder.Version = 3;
            builder.DataSource = dataSource;
            builder.FailIfMissing = false; // create a new file if not existing
            
            return builder.ToString();
        }

        private static void CreatePathIfNeeded(string file)
        {
            var directory = Path.GetDirectoryName(file);
            if (directory is not null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

    }
}