using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steamboat.Data.Entities;
using Steamboat.Data.Repos;
using Steamboat.NotificationProcessors;
using Steamboat.Util;
using Steamboat.Util.Serivices;

namespace Steamboat.Crons.Prices
{
    internal class PriceUpdaterCron : CronBase
    {
        private readonly IAppRepository _appRepository;
        private readonly INotificationDispatcher _notificationDispatcher;
        private readonly IPriceUpdaterConfigProvider _configProvider;
        private readonly ILoopIdStore _loopIdStore;
        private readonly IGuidProvider _guidProvider;
        private readonly IAppPricesUpdater _appPricesUpdater;

        public PriceUpdaterCron(
            IAppRepository appRepository,
            INotificationDispatcher notificationDispatcher,
            IPriceUpdaterConfigProvider configProvider,
            ILoopIdStore loopIdStore,
            IGuidProvider guidProvider,
            IAppPricesUpdater appPricesUpdater)
        {
            _appRepository = appRepository;
            _notificationDispatcher = notificationDispatcher;
            _configProvider = configProvider;
            _loopIdStore = loopIdStore;
            _guidProvider = guidProvider;
            _appPricesUpdater = appPricesUpdater;
        }

        protected override int UpdateIntervalMs => _configProvider.UpdateIntervalMs;

        public override async Task Update(CancellationToken cancellationToken)
        {
            var loopId = _loopIdStore.GetOrCreate();
            Console.WriteLine($"Price update ID: {loopId}");

            // Over time fetch all the apps that don't have the current "loop uuid" yet
            var apps = GetNextApps(loopId);
            
            if (apps.Count == 0)
            {
                // Found no apps with a new ID -> refresh the ID and start anew in the next tick
                Console.WriteLine("Reached the end of price update");
                _loopIdStore.Store(_guidProvider.Create());
                return;
            }

            var appsUpdated = await _appPricesUpdater.ProcessAppsAsync(apps, loopId, cancellationToken);
            _appRepository.AddOrUpdateApps(appsUpdated);
            
            Console.WriteLine(
                $"Fetched the prices of the next {apps.Count} apps (app IDs {apps[0].Id} - {apps[^1].Id})");

            await _notificationDispatcher.NotifyAppPricesScanned(ReadOnlyListProxy.From(apps));
        }

        private IList<AppEntity> GetNextApps(Guid loopId)
        {
            return _appRepository.ListApps(0, _configProvider.NumAppsPerPriceUpdateTick, loopId);
        }
    }
}