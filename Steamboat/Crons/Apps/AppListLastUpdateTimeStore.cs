using System;
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

        public DateTimeOffset? Get()
        {
            var lastUpdateTimestamp = _stateEntryRepository.GetValue<long>(LastUpdateTimeKey);
            if (lastUpdateTimestamp == 0)
            {
                return default;
            }

            return DateTimeOffset.FromUnixTimeSeconds(lastUpdateTimestamp);
        }

        public void Store(DateTimeOffset time)
        {
            var timestamp = time.ToUnixTimeSeconds();
            _stateEntryRepository.StoreValue(LastUpdateTimeKey, timestamp);
        }
    }
}