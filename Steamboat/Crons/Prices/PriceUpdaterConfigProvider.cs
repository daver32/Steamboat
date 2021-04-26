using InterfaceGenerator;
using Microsoft.Extensions.Configuration;

namespace Steamboat.Crons.Prices
{
    [GenerateAutoInterface]
    internal class PriceUpdaterConfigProvider : IPriceUpdaterConfigProvider
    {
        private readonly IConfiguration _configuration;

        public PriceUpdaterConfigProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public int UpdateIntervalMs
        {
            get
            {
                var interval = _configuration.GetValue<int?>("AppsPriceUpdateTickIntervalSeconds")
                               ?? ConfigDefaults.AppsPriceUpdateTickIntervalSeconds;

                return interval * 1000;
            }
        }

        public int NumAppsPerPriceUpdateTick
        {
            get
            {
                var result = _configuration.GetValue<int?>("NumAppsPerPriceUpdateTick");
                return result ?? ConfigDefaults.NumAppsPerPriceUpdateTick;
            }
        }
    }
}