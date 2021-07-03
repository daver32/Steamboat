using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using InterfaceGenerator;

namespace Steamboat.Data
{
    [GenerateAutoInterface]
    internal class DbContext : IDbContext, IDisposable, IAsyncDisposable
    {
        private readonly DbConnectionFactory _dbConnectionFactory;

        private readonly SemaphoreSlim _semaphore = new(1);

        private bool _isInitialized;

        private DbTransaction _transaction = null!;
        private DbConnection _connection = null!;

        public DbContext(DbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public bool IsUnitOfWorkCancelled { get; set; }

        [AutoInterfaceIgnore]
        public async ValueTask DisposeAsync()
        {
            await _semaphore.WaitAsync();

            try
            {
                await DisposeInternalAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        [AutoInterfaceIgnore]
        public void Dispose()
        {
            Task.Run(async () => await DisposeAsync());
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync();

            try
            {
                if (_isInitialized)
                {
                    return;
                }

                _isInitialized = true;

                _connection = await _dbConnectionFactory.CreateAsync();
                await _connection.OpenAsync(cancellationToken);
                _transaction = await _connection.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<DbTransaction> GetTransactionAsync(CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            return _transaction;
        }

        public async Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            await EnsureInitializedAsync(cancellationToken);
            return _connection;
        }

        private async Task DisposeInternalAsync()
        {
            if (!_isInitialized)
            {
                return;
            }

            if (IsUnitOfWorkCancelled)
            {
                await _transaction.RollbackAsync();
            }
            else
            {
                await _transaction.CommitAsync();
            }

            await _transaction.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}