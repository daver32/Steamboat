using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        public AppEntity Process(AppEntity appEntity, IReadOnlyDictionary<int, AppPriceInfo> priceInfos, Guid loopId)
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
                OnFreePromotionDetected(appEntity);
            }

            return appEntity with
            {
                PriceFetchId = loopId,
                LastDiscountPercentage = priceInfo.Data.PriceOverview.DiscountPercent,
            };
        }
        
        private void OnFreePromotionDetected(AppEntity appEntity)
        {
            _notificationJobRepository.Enqueue(new NotificationJobEntity()
            {
                AppName = appEntity.Name,
                AppId = appEntity.Id,
            });
        }
    }
}