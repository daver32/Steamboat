using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using Steamboat.Data.Entities;

namespace Steamboat.NotificationProcessors.Discord
{
    internal class DiscordNotificationProcessor : INotificationProcessor
    {
        private readonly DiscordClientHolder _clientHolder;
        private readonly IConfiguration _configuration;

        public DiscordNotificationProcessor(DiscordClientHolder clientHolder, IConfiguration configuration)
        {
            _clientHolder = clientHolder;
            _configuration = configuration;
        }

        public async Task HandleFreePromotion(int appId, string appName)
        {
            var messageContent = $"@everyone " +
                                 $"Free game detected: {appName} " +
                                 $"https://store.steampowered.com/app/{appId}";
            
            foreach (var channel in await GetNotificationChannelsAsync())
            {
                await channel.SendMessageAsync(messageContent);
            }
        }

        public Task HandleAppPricesScanned(IReadOnlyList<AppEntity> apps)
        {
            // using Task.Run because retrieving the discord client may take time and this is expected
            // to be used more as a fire and forget event
            
            Task.Run(async () => await HandleAppPricesScannedInternalAsync(apps));
            return Task.CompletedTask;
        }

        private async Task HandleAppPricesScannedInternalAsync(IReadOnlyList<AppEntity> apps)
        {
            var client = await _clientHolder.GetClientAsync();

            if (apps.Count == 0)
            {
                return;
            }

            var activity = new DiscordActivity($"apps {apps[0].Id} - {apps[^1].Id}", ActivityType.Watching);
            await client.UpdateStatusAsync(activity, UserStatus.Online);
        }

        private async Task<IEnumerable<DiscordChannel>> GetNotificationChannelsAsync()
        {
            var client = await _clientHolder.GetClientAsync();

            var channels = new List<DiscordChannel>();

            foreach (var channelId in GetNotificationChannelIds())
            {
                var channel = await client.GetChannelAsync(channelId);
                
                if (channel is not null)
                {
                    channels.Add(channel);
                }
            }

            return channels;
        }

        private IEnumerable<ulong> GetNotificationChannelIds()
        {
            return _configuration.GetSection("NotificationChannelIds")
                                 .GetChildren()
                                 .Select(x => x.Value)
                                 .Select(x => ulong.Parse(x));
        }
    }
}