using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using InterfaceGenerator;
using Steamboat.Data.Entities;
using Steamboat.Data.Repos;
using Steamboat.Steam.Dtos;

namespace Steamboat.Crons.Prices
{
    [GenerateAutoInterface]
    internal class AppPriceProcessor : IAppPriceProcessor
    {
        private readonly INotificationJobRepository _notificationJobRepository;

        public AppPriceProcessor(INotificationJobRepository notificationJobRepository)
        {
            _notificationJobRepository = notificationJobRepository;
        }
        
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator", Justification = "100 is a gud value")]
        public async Task<AppEntity> ProcessAsync(
            AppEntity appEntity, IReadOnlyDictionary<int, AppPriceInfo> priceInfos, Guid loopId)
        {
            if (!priceInfos.TryGetValue(appEntity.Id, out var priceInfo) || 
                !priceInfo.Success || 
                priceInfo.Data?.PriceOverview is null)
            {
                return appEntity with
                {
                    PriceFetchId = loopId,
                    LastDiscountPercentage = double.NaN,
                };
            }
            
            if (priceInfo.Data.PriceOverview.DiscountPercent == 100 &&
                appEntity.LastDiscountPercentage != 100)
            {
                await OnFreePromotionDetectedAsync(appEntity);
            }

            return appEntity with
            {
                PriceFetchId = loopId,
                LastDiscountPercentage = priceInfo.Data.PriceOverview.DiscountPercent,
            };
        }
        
        private async Task OnFreePromotionDetectedAsync(AppEntity appEntity)
        {
            await _notificationJobRepository.EnqueueAsync(new NotificationJobEntity()
            {
                AppName = appEntity.Name,
                AppId = appEntity.Id,
                CreatedUtc = DateTimeOffset.UtcNow,
            });
        }
    }
}