using LiteDB;

namespace Steamboat.Data.Entities
{
    internal record StateEntryEntity
    {
        [BsonId]
        public string Key { get; init; } = null!;
        public string? Value { get; init; } 
    }
}