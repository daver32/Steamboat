using System.IO;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Steamboat.Data.Entities;

namespace Steamboat.Data
{
    internal class DbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LiteDatabase Create()
        {
            var dbPath = _configuration.GetValue<string?>("DbPath", null) ?? ConfigDefaults.DatabasePath;
            CreatePathIfNeeded(dbPath);
            var database = new LiteDatabase(dbPath);
            CreateIndices(database);
            return database;
        }

        private static void CreatePathIfNeeded(string file)
        {
            var directory = Path.GetDirectoryName(file);
            if (directory is not null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static void CreateIndices(LiteDatabase database)
        {
            database.GetCollection<AppEntity>().EnsureIndex(x => x.Id);
            database.GetCollection<NotificationJobEntity>().EnsureIndex(x => x.Id);
            database.GetCollection<StateEntryEntity>().EnsureIndex(x => x.Key);
        }
    }
}