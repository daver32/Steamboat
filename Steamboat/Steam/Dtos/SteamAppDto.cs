using Newtonsoft.Json;

namespace Steamboat.Steam.Dtos
{
    internal class SteamAppDto
    {
        [JsonProperty("appid")]
        public int AppId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = null!;
        
        [JsonProperty("last_modified")]
        public long LastModified { get; set; }
        
        [JsonProperty("price_change_number")]
        public long PriceChangeNumber { get; set; }
    }
}