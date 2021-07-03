using System.Data.SQLite;
using System.Threading.Tasks;
using Dapper;

namespace Steamboat.Data
{
    internal class DbStructureIntializer
    {
        public async Task<bool> EnsureCreatedAndUpToDateAsync(SQLiteConnection connection)
        {
            if (await QueryUserVersionAsync(connection) == DatabaseInitProvider.InitScriptVersion)
            {
                return false;
            }

            await CreateStructureAsync(connection);
            await StoreUserVersionAsync(connection);
            return true;
        }

        private async Task CreateStructureAsync(SQLiteConnection connection)
        {
            await connection.ExecuteAsync(DatabaseInitProvider.GetInitScript());
        }

        private async Task<int> QueryUserVersionAsync(SQLiteConnection connection)
        {
            return await connection.QuerySingleAsync<int>("PRAGMA user_version");
        }

        private async Task StoreUserVersionAsync(SQLiteConnection connection)
        {
            await connection.ExecuteAsync($"PRAGMA user_version = {DatabaseInitProvider.InitScriptVersion}");
        }
    }
}