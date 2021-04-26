using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using InterfaceGenerator;
using Newtonsoft.Json.Linq;
using Steamboat.Steam.Dtos;

namespace Steamboat.Steam
{
    // https://wiki.teamfortress.com/wiki/User:RJackson/StorefrontAPI
    [GenerateAutoInterface]
    internal class PriceInfoApiService : IDisposable, IPriceInfoApiService
    {
        private readonly HttpClient _client = new();

        [AutoInterfaceIgnore]
        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task<IDictionary<int, AppPriceInfo>> GetAsync(
            IEnumerable<int> appIds,
            CancellationToken cancellationToken = default)
        {
            var url = BuildRequestUrl(appIds);

            var json = await _client.GetStringAsync(url, cancellationToken);

            return ParseJsonResponse(json);
        }

        private static Dictionary<int, AppPriceInfo> ParseJsonResponse(string json)
        {
            var rootObj = JObject.Parse(json);
            var result = new Dictionary<int, AppPriceInfo>();

            foreach (var property in rootObj.Properties())
            {
                var appId = int.Parse(property.Name);
                
                var appObject = (JObject)property.Value;
                PatchAppJsonObject(appObject);

                var appPriceInfo = appObject.ToObject<AppPriceInfo>() ??
                                   throw new Exception("Something went wrong while deserializing the JSON response. ");
                
                result.Add(appId, appPriceInfo);
            }

            return result;
        }

        private static void PatchAppJsonObject(JObject appObject)
        {
            var appDataValue = appObject.GetValue("data");
            if (appDataValue is not JObject)
            {
                // for some reason, when the appDataValue is empty, we get an empty JArray instead of 
                // a JObject ([] instead of {}), so the good old methods of convenient deserialization don't work
                appObject["data"] = null;
            }
        }

        private static string BuildRequestUrl(IEnumerable<int> appIds)
        {
            return "https://store.steampowered.com"
                   .AppendPathSegment("api")
                   .AppendPathSegment("appdetails")
                   .SetQueryParam("appids", string.Join(',', appIds))
                   .SetQueryParam("filters", "price_overview");
        }
    }
}