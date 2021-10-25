using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Steamboat.Data;
using Steamboat.Util;

namespace Steamboat.Crons
{
    internal class CronRunner : IStartable, IDisposable
    {
        private volatile bool _isDisposed;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly ILifetimeScope _scope;
        
        public CronRunner(ILifetimeScope scope)
        {
            _scope = scope;
        }

        public void Start()
        {
            var cronConfigs = _scope.Resolve<IEnumerable<CronConfig>>();
            foreach (var cronConfig in cronConfigs)
            {
                Task.Run(async () => await RunCronAsync(cronConfig));
            }
        }

        private async Task RunCronAsync(CronConfig cronConfig)
        {
            while (!_isDisposed)
            {
                await UpdateCronAsync(cronConfig);
                await Task.Delay(cronConfig.UpdateIntervalMs, _cancellationTokenSource.Token);
            }
        }

        private async Task UpdateCronAsync(CronConfig cronConfig)
        {
            using var scope = _scope.BeginLifetimeScope(cronConfig);
            var cron = (ICron)scope.Resolve(cronConfig.CronType);

            try
            {
                if (cronConfig.UpdateTimeoutMs is int timeoutMs)
                {
                    await cron.Update(_cancellationTokenSource.Token).WithTimeout(timeoutMs);
                }
                else
                {
                    await cron.Update(_cancellationTokenSource.Token);
                }
            }
            catch (Exception e)
            {
                scope.Resolve<DbContext>().IsUnitOfWorkCancelled = true;
                Console.WriteLine(e);
            }
        }

        public void Dispose()
        {
            _isDisposed = true;
            _cancellationTokenSource.Cancel();
        }
    }
}