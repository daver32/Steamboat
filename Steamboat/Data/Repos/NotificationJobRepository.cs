using InterfaceGenerator;
using Steamboat.Data.Entities;

namespace Steamboat.Data.Repos
{
    [GenerateAutoInterface]
    internal class NotificationJobRepository : INotificationJobRepository
    {
        private readonly DatabaseHolder _databaseHolder;

        public NotificationJobRepository(DatabaseHolder databaseHolder)
        {
            _databaseHolder = databaseHolder;
        }
        
        public NotificationJobEntity? Dequeue()
        {
            var collection = _databaseHolder.Database.GetCollection<NotificationJobEntity>();
            var notification = collection.Query().FirstOrDefault();

            if (notification is not null)
            {
                collection.Delete(notification.Id);
            }

            return notification;
        }

        public void Enqueue(NotificationJobEntity notificationJob)
        {
            var collection = _databaseHolder.Database.GetCollection<NotificationJobEntity>();
            collection.Insert(notificationJob);
        }
    }
}