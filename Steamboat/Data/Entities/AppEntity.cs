using System;

namespace Steamboat.Data.Entities
{
    internal record AppEntity
    {
        public int Id { get; init; }
        public string Name { get; init; } = null!;
        public double LastDiscountPercentage { get; init; } = double.NaN;
        public long LastModified { get; init; }
        public long PriceChangeNumber { get; init; }
        public Guid PriceFetchId { get; init; }
    }
}