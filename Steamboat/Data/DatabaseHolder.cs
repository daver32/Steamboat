using System;
using LiteDB;

namespace Steamboat.Data
{
    internal class DatabaseHolder
    {
        private readonly Lazy<LiteDatabase> _lazyDatabase;

        public DatabaseHolder(DbConnectionFactory connectionFactory)
        {
            _lazyDatabase = new Lazy<LiteDatabase>(connectionFactory.Create);
        }

        public LiteDatabase Database => _lazyDatabase.Value;
    }
}