using System.Collections.Generic;
using System.Threading.Tasks;
using InterfaceGenerator;
using Steamboat.Data.Entities;

namespace Steamboat.NotificationProcessors
{
    [GenerateAutoInterface]
    internal class NotificationDispatcher : INotificationDispatcher
    {
        private readonly IEnumerable<INotificationProcessor> _notificationProcessors;

        public NotificationDispatcher(IEnumerable<INotificationProcessor> notificationProcessors)
        {
            _notificationProcessors = notificationProcessors;
        }

        public async Task NotifyFreeAppDetected(int appId, string appName)
        {
            foreach (var notificationProcessor in _notificationProcessors)
            {
                await notificationProcessor.HandleFreePromotion(appId, appName);
            }
        }

        public async Task NotifyAppPricesScanned(IReadOnlyList<AppEntity> apps)
        {
            foreach (var notificationProcessor in _notificationProcessors)
            {
                await notificationProcessor.HandleAppPricesScanned(apps);
            }
        }
    }
}