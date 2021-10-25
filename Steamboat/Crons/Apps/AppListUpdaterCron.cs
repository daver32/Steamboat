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
    internal class AppListUpdaterCron : ICron
    {
        private readonly IAppListApiService _appListApiService;
        private readonly IAppRepository _appRepository;
        private readonly IAppListLastUpdateTimeStore _lastUpdateTimeStore;

        public AppListUpdaterCron(
            IAppListApiService appListApiService,
            IAppRepository appRepository,
            IAppListLastUpdateTimeStore lastUpdateTimeStore)
        {
            _appListApiService = appListApiService;
            _appRepository = appRepository;
            _lastUpdateTimeStore = lastUpdateTimeStore;
        }

        public async Task Update(CancellationToken cancellationToken)
        {
            Console.WriteLine("Updating app list...");

            DateTimeOffset? lastUpdate = await _lastUpdateTimeStore.GetAsync();
            var apps = await _appListApiService.GetAsync(lastUpdate, cancellationToken);

            Console.WriteLine($"Fetched a total of {apps.Count} apps");

            await _appRepository.AddOrUpdateAppsAsync(apps.Select(MapApp), false, false);
            
            await _lastUpdateTimeStore.StoreAsync(DateTimeOffset.UtcNow);
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
        
        public class Config : CronConfig<AppListUpdaterCron>
        {
            private readonly IConfiguration _configuration;

            public Config(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public override int UpdateIntervalMs
            {
                get
                {
                    var intervalHours = _configuration.GetValue<int?>("AppListUpdateIntervalHours");
                    intervalHours ??= ConfigDefaults.AppListUpdateIntervalHours;
                    return (int)TimeSpan.FromHours(intervalHours.Value).TotalMilliseconds;
                }
            }

            public override int? UpdateTimeoutMs => (int)TimeSpan.FromMinutes(30).TotalMilliseconds;
        }
    }
}