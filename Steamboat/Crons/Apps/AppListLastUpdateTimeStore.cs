using System;
using System.Threading.Tasks;
using InterfaceGenerator;
using Steamboat.Data.Repos;

namespace Steamboat.Crons.Apps
{
    [GenerateAutoInterface]
    internal class AppListLastUpdateTimeStore : IAppListLastUpdateTimeStore
    {
        public const string LastUpdateTimeKey = "last_applist_update";
        private readonly IStateEntryRepository _stateEntryRepository;

        public AppListLastUpdateTimeStore(IStateEntryRepository stateEntryRepository)
        {
            _stateEntryRepository = stateEntryRepository;
        }

        public async Task<DateTimeOffset?> GetAsync()
        {
            var lastUpdateTimestampStr = await _stateEntryRepository.GetValueAsync(LastUpdateTimeKey);
            if (!long.TryParse(lastUpdateTimestampStr, out var lastUpdateTimestamp) || lastUpdateTimestamp <= 0)
            {
                return default;
            }

            return DateTimeOffset.FromUnixTimeSeconds(lastUpdateTimestamp);
        }

        public async Task StoreAsync(DateTimeOffset time)
        {
            var timestamp = time.ToUnixTimeSeconds();
            await _stateEntryRepository.StoreValueAsync(LastUpdateTimeKey, timestamp.ToString());
        }
    }
}