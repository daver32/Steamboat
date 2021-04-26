using Newtonsoft.Json;

namespace Steamboat.Steam.Dtos
{
    [JsonObject]
    internal record AppPriceInfo
    {
        [JsonRequired, JsonProperty("success")]
        public bool Success { get; init; }

        [JsonProperty("data")]
        public ItemData? Data { get; init; }
        
        [JsonObject]
        public record ItemData
        {
            [JsonProperty("price_overview")]
            public ItemPriceOverview? PriceOverview { get; init; }
            
            public record ItemPriceOverview
            {
                [JsonRequired, JsonProperty("currency")]
                public string Currency { get; init; } = null!;

                [JsonRequired, JsonProperty("initial")]
                public int Initial { get; init; }
                
                [JsonRequired, JsonProperty("final")]
                public int Final { get; init; }
                
                [JsonRequired, JsonProperty("discount_percent")]
                public double DiscountPercent { get; init; }
            }
        }
    }
}