using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InterfaceGenerator;
using Steamboat.Data.Entities;
using Steamboat.Steam;
using Steamboat.Util;

namespace Steamboat.Crons.Prices
{
    [GenerateAutoInterface]
    internal class AppPricesUpdater : IAppPricesUpdater
    {
        private readonly IAppPriceProcessor _appPriceProcessor;
        private readonly IPriceInfoApiService _priceInfoApiService;

        public AppPricesUpdater(IPriceInfoApiService priceInfoApiService, IAppPriceProcessor appPriceProcessor)
        {
            _priceInfoApiService = priceInfoApiService;
            _appPriceProcessor = appPriceProcessor;
        }

        public async Task<ICollection<AppEntity>> ProcessAppsAsync(
            IList<AppEntity> appEntities,
            Guid loopId,
            CancellationToken cancellationToken = default)
        {
            var priceInfos = await _priceInfoApiService.GetAsync(appEntities.Select(x => x.Id), cancellationToken);
            var appEntitiesUpdated = new List<AppEntity>(appEntities.Count);

            foreach (var appEntity in appEntities)
            {
                var appEntityUpdated = _appPriceProcessor.Process(
                    appEntity, ReadOnlyDictionaryProxy.From(priceInfos), loopId);
                
                appEntitiesUpdated.Add(appEntityUpdated);
            }

            return appEntitiesUpdated;
        }
    }
}