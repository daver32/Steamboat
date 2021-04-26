using System;
using InterfaceGenerator;
using Steamboat.Data.Repos;
using Steamboat.Util.Serivices;

namespace Steamboat.Crons.Prices
{
    [GenerateAutoInterface]
    internal class LoopIdStore : ILoopIdStore
    {
        private const string LoopIdKey = "price_update_id";
        
        private readonly IStateEntryRepository _stateEntryRepository;
        private readonly IGuidProvider _guidProvider;

        public LoopIdStore(IStateEntryRepository stateEntryRepository, IGuidProvider guidProvider)
        {
            _stateEntryRepository = stateEntryRepository;
            _guidProvider = guidProvider;
        }

        public Guid GetOrCreate()
        {
            var stringId = _stateEntryRepository.GetValue(LoopIdKey);

            if (stringId is null)
            {
                var id = _guidProvider.Create();
                _stateEntryRepository.StoreValue(LoopIdKey, id);
                return id;
            }

            return Guid.Parse(stringId);
        }

        public void Store(Guid id)
        {
            _stateEntryRepository.StoreValue(LoopIdKey, id);
        }
    }
}