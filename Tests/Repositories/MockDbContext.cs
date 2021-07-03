using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Steamboat.Data;

namespace Tests.Repositories
{
    internal class MockDbContext : IDbContext, IDisposable
    {
        private readonly SQLiteConnection _connection;
        private readonly SQLiteTransaction _transaction;

        bool IDbContext.IsUnitOfWorkCancelled
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public MockDbContext()
        {
            _connection = new SQLiteConnection(MakeConnectionString());
            _connection.Open();
            _connection.ExecuteAsync(DatabaseInitProvider.GetInitScript());
            _transaction = _connection.BeginTransaction(IsolationLevel.Serializable);
        }

        private string MakeConnectionString()
        {
            var builder = new SQLiteConnectionStringBuilder()
            {
                Version = 3,
                DataSource = ":memory:",
            };

            return builder.ToString();
        }

        public Task<DbTransaction> GetTransactionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<DbTransaction>(_transaction);
        }

        public Task<DbConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<DbConnection>(_connection);
        }

        public void CommitMockTransaction()
        {
            _transaction.Commit();
        }

        public void Dispose()
        {
        }
    }
}