using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper.Transaction;
using InterfaceGenerator;
using Steamboat.Data.Entities;

namespace Steamboat.Data.Repos
{
    [GenerateAutoInterface]
    internal class AppRepository : IAppRepository
    {
        private readonly IDbContext _dbContext;

        public AppRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddOrUpdateAppsAsync(
            IEnumerable<AppEntity> apps,
            bool updatePriceFetchId,
            bool updateLastDiscountPercentage)
        {
            var transaction = await _dbContext.GetTransactionAsync();
            
            var command = BuildInsertOrUpdateCommand(updatePriceFetchId, updateLastDiscountPercentage);

            foreach (var app in apps)
            {
                await transaction.ExecuteAsync(command, app);
            }
        }

        public async Task<IList<AppEntity>> ListAppsAsync(
            int page,
            int amount,
            Guid? excludedPriceFetchId = default)
        {
            var transaction = await _dbContext.GetTransactionAsync();

            var results = await transaction.QueryAsync<AppEntity>(
                "SELECT * FROM Apps WHERE PriceFetchId != @excludedPriceFetchId ORDER BY Id ASC LIMIT @amount OFFSET @offset",
                new { offset = page * amount, amount, excludedPriceFetchId = excludedPriceFetchId.ToString() });

            return results.ToList();
        }

        private static string BuildInsertOrUpdateCommand(
            bool updatePriceFetchId,
            bool updateLastDiscountPercentage)
        {
            var commandBuilder = new StringBuilder();

            commandBuilder.Append("INSERT INTO Apps");
            commandBuilder.Append("(Id, PriceFetchId, LastDiscountPercentage, Name, LastModified, PriceChangeNumber) ");
            commandBuilder.Append("VALUES");
            commandBuilder.Append("(@Id, @PriceFetchId, @LastDiscountPercentage, @Name, @LastModified, @PriceChangeNumber) ");

            commandBuilder.Append("ON CONFLICT(Id) DO UPDATE SET ");

            if (updatePriceFetchId)
            {
                commandBuilder.Append("PriceFetchId = @PriceFetchId, ");
            }

            if (updateLastDiscountPercentage)
            {
                commandBuilder.Append("LastDiscountPercentage = @LastDiscountPercentage, ");
            }

            commandBuilder.Append("Name = @Name, ");
            commandBuilder.Append("LastModified = @LastModified, ");
            commandBuilder.Append("PriceChangeNumber = @PriceChangeNumber");

            return commandBuilder.ToString();
        }
    }
}