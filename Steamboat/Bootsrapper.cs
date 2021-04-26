﻿using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Steamboat.Crons;
using Steamboat.Crons.Apps;
using Steamboat.Crons.Prices;
using Steamboat.Data;
using Steamboat.Data.Repos;
using Steamboat.NotificationProcessors;
using Steamboat.NotificationProcessors.Discord;
using Steamboat.Steam;
using Steamboat.Util.Serivices;

namespace Steamboat
{
    internal class Bootsrapper
    {
        private static Bootsrapper? instance;

        private static readonly object SyncRoot = new();
        
        public IContainer Container { get; }

        private Bootsrapper(IConfiguration config)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(config).As<IConfiguration>();
            builder.RegisterType<DbConnectionFactory>();
            builder.RegisterType<DatabaseHolder>().SingleInstance();
            builder.RegisterType<AppRepository>().As<IAppRepository>();
            builder.RegisterType<StateEntryRepository>().As<IStateEntryRepository>();
            builder.RegisterType<NotificationJobRepository>().As<INotificationJobRepository>();

            builder.RegisterType<AppListApiService>().As<IAppListApiService>();
            builder.RegisterType<PriceInfoApiService>().As<IPriceInfoApiService>();

            builder.RegisterType<AppListUpdaterCron>().As<IStartable>().SingleInstance();
            builder.RegisterType<AppListLastUpdateTimeStore>().As<IAppListLastUpdateTimeStore>();
            
            builder.RegisterType<PriceUpdaterCron>().As<IStartable>().SingleInstance();
            builder.RegisterType<PriceUpdaterConfigProvider>().As<IPriceUpdaterConfigProvider>();
            builder.RegisterType<LoopIdStore>().As<ILoopIdStore>();
            builder.RegisterType<AppPricesUpdater>().As<IAppPricesUpdater>();
            builder.RegisterType<AppPriceProcessor>().As<IAppPriceProcessor>();
            
            builder.RegisterType<NotificationJobsHandlerCron>().As<IStartable>().SingleInstance();

            builder.RegisterType<NotificationDispatcher>().As<INotificationDispatcher>().SingleInstance();
            builder.RegisterType<LogNotificationProcessor>().As<INotificationProcessor>().SingleInstance();
            builder.RegisterType<DiscordClientFactory>();
            builder.RegisterType<DiscordClientHolder>().SingleInstance();
            builder.RegisterType<DiscordNotificationProcessor>().As<INotificationProcessor>().SingleInstance();

            builder.RegisterType<GuidProvider>().As<IGuidProvider>();

            Container = builder.Build();
        }

        public static Bootsrapper Instance
        {
            get
            {
                lock (SyncRoot)
                {
                    if (instance is null)
                    {
                        throw new InvalidOperationException("Not initialized");
                    }

                    return instance;
                }
            }
        }

        public static void Init(IConfiguration config)
        {
            lock (SyncRoot)
            {
                if (instance is not null)
                {
                    throw new InvalidOperationException("Already initialized");
                }

                instance = new Bootsrapper(config);
            }
        }
    }
}