using System.Threading.Tasks;
using Dapper.Transaction;
using InterfaceGenerator;

namespace Steamboat.Data.Repos
{
    [GenerateAutoInterface]
    internal class StateEntryRepository : IStateEntryRepository
    {
        private readonly IDbContext _dbContext;

        public StateEntryRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string?> GetValueAsync(string key)
        {
            var transaction = await _dbContext.GetTransactionAsync();
            return await transaction.QuerySingleOrDefaultAsync<string?>(
                "SELECT Value FROM StateEntries WHERE Key=@key", new { key });
        }

        public async Task StoreValueAsync(string key, string? value)
        {
            var transaction = await _dbContext.GetTransactionAsync();

            const string insert = "INSERT INTO StateEntries(Key, Value) VALUES (@key, @value) " +
                                  "ON CONFLICT(Key) DO UPDATE SET Value = @value";

            await transaction.ExecuteAsync(insert, new { key, value });
        }
    }
}