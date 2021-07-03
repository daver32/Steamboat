using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Steamboat;
using Steamboat.Data;

DapperTypeHandlerInitializer.Init();

var configPath = Environment.GetEnvironmentVariable("CONFIG_PATH")
                 ?? "config.json";

var config = new ConfigurationBuilder()
             .AddJsonFile(configPath)
             .Build();

Bootsrapper.Init(config);

await Task.Delay(-1);