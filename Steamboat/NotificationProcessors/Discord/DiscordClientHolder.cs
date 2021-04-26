using System;
using System.Threading.Tasks;
using AsyncLazy;
using DSharpPlus;
using DSharpPlus.Entities;
using Steamboat.Util;

namespace Steamboat.NotificationProcessors.Discord
{
    internal class DiscordClientHolder : IDisposable
    {
        private readonly DiscordClientFactory _clientFactory;
        
        private readonly AsyncLazy<DiscordClient> _lazyClient;

        public DiscordClientHolder(DiscordClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _lazyClient = new AsyncLazy<DiscordClient>(CreateAndConnectAsync);
        }
        
        public void Dispose()
        {
            if (_lazyClient.IsValueCreated)
            {
                _lazyClient.Value.Dispose();
            }
        }

        public async Task<DiscordClient> GetClientAsync()
        {
            return await _lazyClient.GetValueAsync();
        }

        private async Task<DiscordClient> CreateAndConnectAsync()
        {
            var client = _clientFactory.Create();
            await client.ConnectAsync(status: UserStatus.Online);
            await client.WaitForReadyAsync();
            return client;
        }
    }
}