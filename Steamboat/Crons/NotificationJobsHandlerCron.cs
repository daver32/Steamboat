using System;
using System.Threading;
using System.Threading.Tasks;
using Steamboat.Data.Repos;
using Steamboat.NotificationProcessors;

namespace Steamboat.Crons
{
    internal class NotificationJobsHandlerCron : CronBase
    {
        private readonly INotificationJobRepository _notificationJobRepository;
        private readonly INotificationDispatcher _notificationDispatcher;

        public NotificationJobsHandlerCron(
            INotificationJobRepository notificationJobRepository,
            INotificationDispatcher notificationDispatcher)
        {
            _notificationJobRepository = notificationJobRepository;
            _notificationDispatcher = notificationDispatcher;
        }
        
        protected override int UpdateIntervalMs { get; } = (int)TimeSpan.FromMinutes(10).TotalMilliseconds;
        
        public override async Task Update(CancellationToken cancellationToken)
        {
            while (true)
            {
                var notification = _notificationJobRepository.Dequeue();
                if (notification is null)
                {
                    break;
                }
                
                await _notificationDispatcher.NotifyFreeAppDetected(notification.AppId, notification.AppName);
            }
        }
    }
}