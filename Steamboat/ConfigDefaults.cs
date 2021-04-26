namespace Steamboat
{
    internal static class ConfigDefaults
    {
        public const string DatabasePath = "data/steamboat.db";
        
        public const int AppListUpdateIntervalHours = 48;
        
        public const int AppsPriceUpdateTickIntervalSeconds = 60;
        public const int NumAppsPerPriceUpdateTick = 100;
    }
}