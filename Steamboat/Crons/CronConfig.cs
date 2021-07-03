using System;

namespace Steamboat.Crons
{
    internal abstract class CronConfig
    {
        public abstract Type CronType { get; }
        public abstract int UpdateIntervalMs { get; }
    }
    
    internal abstract class CronConfig<TCron> : CronConfig where TCron : ICron
    {
        public override Type CronType => typeof(TCron);
    }
}