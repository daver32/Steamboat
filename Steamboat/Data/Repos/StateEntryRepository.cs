using System;
using InterfaceGenerator;
using LiteDB;
using Steamboat.Data.Entities;

namespace Steamboat.Data.Repos
{
    [GenerateAutoInterface]
    internal class StateEntryRepository : IStateEntryRepository
    {
        private readonly DatabaseHolder _databaseHolder;

        public StateEntryRepository(DatabaseHolder databaseHolder)
        {
            _databaseHolder = databaseHolder;
        }

        public string? GetValue(string key)
        {
            var collection = GetCollection();
            var entry = collection.FindById(key);

            if (entry is null)
            {
                return default;
            }

            return entry.Value;
        }
        
        public TValue? GetValue<TValue>(string key)
        {
            var strValue = GetValue(key);
            
            if (strValue is null)
            {
                return default;
            }

            return (TValue?)Convert.ChangeType(strValue, typeof(TValue));
        }

        public void StoreValue(string key, string? value)
        {
            var collection = GetCollection();

            var entry = new StateEntryEntity
            {
                Key = key,
                Value = value,
            };

            if (!collection.Update(entry))
            {
                collection.Insert(entry);
            }
        }

        public void StoreValue<TValue>(string key, TValue? value)
        {   
            StoreValue(key, Convert.ToString(value));
        }
        
        private ILiteCollection<StateEntryEntity> GetCollection()
        {
            return _databaseHolder.Database.GetCollection<StateEntryEntity>();
        }
    }
}