using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace Steamboat.Crons
{
    internal abstract class CronBase : IStartable, IDisposable
    {
        private volatile bool _isDisposed;
        protected abstract int UpdateIntervalMs { get; }

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public void Dispose()
        {
            _isDisposed = true;
            _cancellationTokenSource.Cancel();
        }

        public void Start()
        {
            Task.Run(async () =>
            {
                while (!_isDisposed)
                {
                    await TickAsync(_cancellationTokenSource.Token);
                }
            });
        }

        private async Task TickAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Update(cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
                    
            await Task.Delay(UpdateIntervalMs, _cancellationTokenSource.Token);
        }

        public abstract Task Update(CancellationToken cancellationToken);
    }
}