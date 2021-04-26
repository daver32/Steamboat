using System;
using System.Threading.Tasks;

namespace Steamboat.NotificationProcessors
{
    internal class LogNotificationProcessor : INotificationProcessor
    {
        public Task HandleFreePromotion(int appId, string appName)
        {
            Console.WriteLine($"Free Promotion Detected: {appName} (ID={appId})");
            return Task.CompletedTask;
        }
    }
}