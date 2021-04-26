# Steamboat

A Discord notification bot that detects free-to-grab Steam games (temporary 100% sales)

## Config

Required values:
- `ApiKey` - your steam API key, can be acquired [here](https://steamcommunity.com/dev/apikey).
- `DiscordToken` - your Discord bot's token.
- `NotificationChannelIds` - an array of channel IDs for the bot to post notifications in. 

Optional values:
- `DbPath` - a path to the bot's LiteDB database file. Defaults to `data/steamboat.db`. The file is created if it doesn't already exist. 
- `AppListUpdateIntervalHours` - how often should the list of apps be updated. Defaults to 48. 
- `AppsPriceUpdateTickIntervalSeconds` - how often should new app prices be fetched. Defaults to 60.
- `NumAppsPerPriceUpdateTick` - how many app prices should be fetched per tick. Defaults to 100.


Example `config.json`:
```json
{
    "ApiKey": "<censored>",
    "DiscordToken": "<censored>",
    "NotificationChannelIds": [
        123456789123456789
    ]
}
```

## Docker

Build:
```
git clone https://github.com/daver32/Steamboat
docker build -t steamboat ./Steamboat
```

Run:
```
docker run -d --name=steamboat --restart=always -v /path_to_config.json:/app/config.json -v steamboat_data:/app/data steamboat
```
