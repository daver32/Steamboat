using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Common;
using NSubstitute;
using Steamboat.Crons.Prices;
using Steamboat.Data.Entities;
using Steamboat.Steam;
using Steamboat.Steam.Dtos;
using Xunit;

namespace Tests
{
    public class AppPriceUpdaterTests
    {
        private readonly IAppPriceProcessor _appPriceProcessor
            = Substitute.For<IAppPriceProcessor>();

        private readonly Fixture _fixture = new();

        private readonly IPriceInfoApiService _priceInfoApiService
            = Substitute.For<IPriceInfoApiService>();

        private readonly AppPricesUpdater _sut;

        public AppPriceUpdaterTests()
        {
            _sut = new AppPricesUpdater(_priceInfoApiService, _appPriceProcessor);
        }

        [Fact]
        public async Task ProcessAppsAsync_GetsPrices_And_CallsProcessOnApps()
        {
            // Arrange
            const int numApps = 100;
            var appIds = _fixture.CreateMany<int>(numApps).ToArray();
            var priceInfos = _fixture.CreateMany<AppPriceInfo>(numApps).ToArray();
            var priceInfoDictionary = CreateDictionary(appIds, priceInfos);

            var appEntities = _fixture.CreateMany<AppEntity>(numApps).ToArray();
            var appEntitiesUpdated = _fixture.CreateMany<AppEntity>(numApps).ToArray();
            for (var i = 0; i < numApps; i++)
            {
                appEntities[i] = appEntities[i] with
                {
                    Id = appIds[i],
                };

                appEntitiesUpdated[i] = appEntities[i] with
                {
                    Id = appIds[i],
                };
            }

            var loopId = _fixture.Create<Guid>();

            _priceInfoApiService
                .GetAsync(
                    Arg.Is<IEnumerable<int>>(x => x.IsSameOrEqualTo(appIds)),
                    Arg.Any<CancellationToken>())
                .Returns(Task.FromResult<IDictionary<int, AppPriceInfo>>(priceInfoDictionary));

            _appPriceProcessor
                .Process(
                    Arg.Any<AppEntity>(),
                    Arg.Is<IReadOnlyDictionary<int, AppPriceInfo>>(x => x.IsSameOrEqualTo(priceInfoDictionary)),
                    Arg.Is(loopId))
                .Returns(callInfo => appEntitiesUpdated.First(x => x.Id == callInfo.Arg<AppEntity>().Id));

            // Act
            await _sut.ProcessAppsAsync(appEntities, loopId);

            // Assert
            await _priceInfoApiService.ReceivedWithAnyArgs().GetAsync(default!);

            var processCalls = _appPriceProcessor.ReceivedWithAnyArgs().ReceivedCalls().ToArray();
            processCalls.Length.Should().Be(numApps);
        }

        private Dictionary<TKey, TValue> CreateDictionary<TKey, TValue>(
            IReadOnlyList<TKey> keys,
            IReadOnlyList<TValue> values)
        {
            keys.Count.Should().Be(values.Count);

            var pairs = new List<KeyValuePair<TKey, TValue>>();

            for (var i = 0; i < keys.Count; i++)
            {
                pairs.Add(new KeyValuePair<TKey, TValue>(keys[i], values[i]));
            }

            return new Dictionary<TKey, TValue>(pairs);
        }
    }
}