using System;
using System.Threading.Tasks;
using InterfaceGenerator;
using Steamboat.Data.Repos;
using Steamboat.Util.Services;

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

        public async Task<Guid> GetOrCreateAsync()
        {
            var stringId = await _stateEntryRepository.GetValueAsync(LoopIdKey);
            if (stringId is not null)
            {
                return Guid.Parse(stringId);
            }

            var id = _guidProvider.Create();
            await StoreInternalAsync(id);
            return id;
        }

        public async Task StoreAsync(Guid id)
        {
            await StoreInternalAsync(id);
        }

        private async Task StoreInternalAsync(Guid id)
        {
            await _stateEntryRepository.StoreValueAsync(LoopIdKey, id.ToString());
        }
    }
}