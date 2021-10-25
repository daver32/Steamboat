using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl;
using InterfaceGenerator;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Steamboat.Steam.Dtos;
using Steamboat.Util;

namespace Steamboat.Steam
{
    // https://steamapi.xpaw.me/#IStoreService/GetAppList
    [GenerateAutoInterface]
    internal class AppListApiService : IDisposable, IAppListApiService
    {
        private readonly HttpClient _client = new();

        private readonly IConfiguration _configuration;

        public const int MaxAppsPerRequest = 50000;
        private const int RequestTimeoutMs = 60000;

        public AppListApiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [AutoInterfaceIgnore]
        public void Dispose()
        {
            _client.Dispose();
        }
        
        private string GetApiKey()
        {
            return _configuration.GetValue<string?>("ApiKey")
                   ?? throw new Exception("API key not configured. ");
        }

        public async Task<IReadOnlyList<SteamAppDto>> GetAsync(
            DateTimeOffset? ifModifiedSince = null,
            CancellationToken cancellationToken = default)
        {
            var apiKey = GetApiKey();

            var appList = new List<SteamAppDto>();
            long? lastAppId = null;

            while (true)
            {
                var response = await GetResponseAsync(apiKey, ifModifiedSince, lastAppId, cancellationToken);
                var apps = response.Response.Apps;
                
                if (apps is null || apps.Length == 0)
                {
                    break;
                }

                appList.AddRange(apps);
                lastAppId = apps[^1].AppId;
            }

            return appList;
        }

        private async Task<AppListResponse> GetResponseAsync(
            string apiKey,
            DateTimeOffset? ifModifiedSince,
            long? lastAppId,
            CancellationToken cancellationToken = default)
        {
            var url = BuildUrl(apiKey, ifModifiedSince, lastAppId);

            var json = await _client.GetStringAsync(url, cancellationToken).WithTimeout(RequestTimeoutMs);
            return JsonConvert.DeserializeObject<AppListResponse>(json) ??
                   throw new Exception("Something went wrong while deserializing the JSON response. ");
        }

        private static Url BuildUrl(string apiKey, DateTimeOffset? ifModifiedSince, long? lastAppId)
        {
            var url = "https://api.steampowered.com"
                   .AppendPathSegment("IStoreService")
                   .AppendPathSegment("GetAppList")
                   .AppendPathSegment("v1")
                   .SetQueryParam("key", apiKey)
                   .SetQueryParam("max_results", MaxAppsPerRequest)
                   .SetQueryParam("include_games", 1)
                   .SetQueryParam("include_dlc", 1)
                   .SetQueryParam("include_software", 1)
                   .SetQueryParam("include_videos", 0)
                   .SetQueryParam("include_hardware", 0);
            
            if (ifModifiedSince is not null)
            {
                var timestamp = ifModifiedSince.Value.ToUnixTimeSeconds();
                url = url.SetQueryParam("if_modified_since", timestamp);
            }
            
            if (lastAppId is not null)
            {
                url = url.SetQueryParam("last_appid", lastAppId);
            }

            return url;
        }
 
        // all those damn APIs with 5231235343 levels of nested JSON objects
        private class AppListResponse
        {
            [JsonProperty("response")]
            public ResponseData Response { get; set; } = null!;
            
            public class ResponseData
            {
                [JsonProperty("apps")]
                public SteamAppDto[]? Apps { get; set; } 
            }
        }
    }
}