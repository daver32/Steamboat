using System.Collections.Generic;
using System.Threading.Tasks;
using Steamboat.Data.Entities;

namespace Steamboat.NotificationProcessors
{
    internal interface INotificationProcessor
    {
        public Task HandleFreePromotion(int appId, string appName)
        {
            return Task.CompletedTask;
        }

        public Task HandleAppPricesScanned(IReadOnlyList<AppEntity> apps)
        {
            return Task.CompletedTask;
        }
    }
}