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

        public void AddOrUpdateApps(params AppEntity[] apps)
        {
            AddOrUpdateApps((IEnumerable<AppEntity>)apps);
        }
        
        public void AddOrUpdateApps(IEnumerable<AppEntity> apps)
        {
            var collection = GetCollection();
            
            var newApps = new List<AppEntity>();
            foreach (var app in apps)
            {
                if (!collection.Update(app))
                {
                    newApps.Add(app);
                }
            }
            
            collection.InsertBulk(newApps);
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