using System;
using DSharpPlus;
using Microsoft.Extensions.Configuration;

namespace Steamboat.NotificationProcessors.Discord
{
    internal class DiscordClientFactory
    {
        private readonly IConfiguration _configuration;

        public DiscordClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public DiscordClient Create()
        {
            var configuration = new DiscordConfiguration
            {
                Token = GetToken(),
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                ReconnectIndefinitely = true, 
            };

            return new DiscordClient(configuration);
        }

        private string GetToken()
        {
            return _configuration.GetValue<string?>("DiscordToken")
                   ?? throw new Exception("Discord token is missing");
        }
    }
}