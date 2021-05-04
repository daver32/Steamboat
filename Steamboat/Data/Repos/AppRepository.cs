using System;
using System.Collections.Generic;
using InterfaceGenerator;
using LiteDB;
using Steamboat.Data.Entities;

namespace Steamboat.Data.Repos
{
    [GenerateAutoInterface]
    internal class AppRepository : IAppRepository
    {
        private readonly DatabaseHolder _databaseHolder;

        public AppRepository(DatabaseHolder databaseHolder)
        {
            _databaseHolder = databaseHolder;
        }
        
        public void AddOrUpdateApps(
            IEnumerable<AppEntity> apps,
            bool updatePriceFetchId,
            bool updateLastDiscountPercentage)
        {
            var collection = GetCollection();
            
            var newApps = new List<AppEntity>();
            foreach (var app in apps)
            {
                if (!UpdateAppIfExists(collection, app, updatePriceFetchId, updateLastDiscountPercentage))
                {
                    newApps.Add(app);
                }
            }
            
            collection.InsertBulk(newApps);
        }

        private static bool UpdateAppIfExists(
            ILiteCollection<AppEntity> collection,
            AppEntity newApp, 
            bool updatePriceFetchId, 
            bool updateLastDiscountPercentage)
        {
            var oldApp = collection.FindById(newApp.Id);
            if (oldApp is null)
            {
                return false;
            }

            var newPriceFetchId = updatePriceFetchId 
                ? newApp.PriceFetchId 
                : oldApp.PriceFetchId;
            
            var newLastDiscountPercentage = updateLastDiscountPercentage
                ? newApp.LastDiscountPercentage
                : oldApp.LastDiscountPercentage;

            newApp = newApp with
            {
                PriceFetchId = newPriceFetchId,
                LastDiscountPercentage = newLastDiscountPercentage,
            };

            return collection.Update(newApp);
        }
        
        public IList<AppEntity> ListApps(
            int page, 
            int amount, 
            Guid? excludedPriceFetchId = null)
        {
            var collection = GetCollection();

            var query = collection.Query();

            if (excludedPriceFetchId is not null)
            {
                query = query.Where(x => x.PriceFetchId != excludedPriceFetchId);
            }

            return query.Skip(page * amount)
                        .Limit(amount)
                        .ToList();
        }

        private ILiteCollection<AppEntity> GetCollection()
        {
            return _databaseHolder.Database.GetCollection<AppEntity>();
        }
    }
}