using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Steamboat.Data.Entities;
using Steamboat.Data.Repos;
using Steamboat.Steam;
using Steamboat.Steam.Dtos;

namespace Steamboat.Crons.Apps
{
    internal class AppListUpdaterCron : CronBase
    {
        private readonly IAppListApiService _appListApiService;
        private readonly IAppRepository _appRepository;
        private readonly IAppListLastUpdateTimeStore _lastUpdateTimeStore;
        private readonly IConfiguration _configuration;

        public AppListUpdaterCron(
            IConfiguration configuration,
            IAppListApiService appListApiService,
            IAppRepository appRepository,
            IAppListLastUpdateTimeStore lastUpdateTimeStore)
        {
            _configuration = configuration;
            _appListApiService = appListApiService;
            _appRepository = appRepository;
            _lastUpdateTimeStore = lastUpdateTimeStore;
        }

        protected override int UpdateIntervalMs
        {
            get
            {
                var intervalHours = _configuration.GetValue<int?>("AppListUpdateIntervalHours");
                intervalHours ??= ConfigDefaults.AppListUpdateIntervalHours;
                return (int)TimeSpan.FromHours(intervalHours.Value).TotalMilliseconds;
            }
        }

        public override async Task Update(CancellationToken cancellationToken)
        {
            Console.WriteLine("Updating app list...");

            DateTimeOffset? lastUpdate = _lastUpdateTimeStore.Get();
            var apps = await _appListApiService.GetAsync(lastUpdate, cancellationToken);

            Console.WriteLine($"Fetched a total of {apps.Count} apps");

            _appRepository.AddOrUpdateApps(apps.Select(MapApp), false, false);
            
            _lastUpdateTimeStore.Store(DateTimeOffset.UtcNow);
        }

        private static AppEntity MapApp(SteamAppDto appDto)
        {
            return new()
            {
                Id = appDto.AppId,
                Name = appDto.Name,
                LastModified = appDto.LastModified,
                PriceChangeNumber = appDto.PriceChangeNumber,
            };
        }
    }
}