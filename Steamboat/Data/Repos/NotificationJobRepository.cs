using System.Threading.Tasks;
using Dapper.Transaction;
using InterfaceGenerator;
using Steamboat.Data.Entities;

namespace Steamboat.Data.Repos
{
    [GenerateAutoInterface]
    internal class NotificationJobRepository : INotificationJobRepository
    {
        private readonly IDbContext _dbContext;

        public NotificationJobRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<NotificationJobEntity?> DequeueAsync()
        {
            var transaction = await _dbContext.GetTransactionAsync();
            
            var notification = await transaction.QuerySingleOrDefaultAsync<NotificationJobEntity?>(
                "SELECT * FROM NotificationJobs WHERE HasBeenProcessed = 0 ORDER BY Id ASC LIMIT 1");

            if (notification is not null)
            {
                // For debugging purposes only mark the notification as processed instead of deleting
                await transaction.ExecuteAsync(
                    "UPDATE NotificationJobs SET HasBeenProcessed = 1 WHERE Id = @Id", notification);
            }

            return notification;
        }

        public async Task EnqueueAsync(NotificationJobEntity notificationJob)
        {
            var transaction = await _dbContext.GetTransactionAsync();

            const string command = "INSERT INTO NotificationJobs(AppName, AppId, CreatedUtc, HasBeenProcessed) " +
                                   "VALUES (@AppName, @AppId, @CreatedUtc, @HasBeenProcessed)";
            
            await transaction.ExecuteAsync(command, notificationJob);
        }
    }
}