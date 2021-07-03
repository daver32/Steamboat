using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using NSubstitute;
using Steamboat.Crons.Prices;
using Steamboat.Data.Entities;
using Steamboat.Data.Repos;
using Steamboat.Steam.Dtos;
using Xunit;

namespace Tests
{
    public class AppPriceProcessorTests
    {
        private readonly Fixture _fixture = new();

        private readonly INotificationJobRepository _notificationJobRepository
            = Substitute.For<INotificationJobRepository>();

        private readonly AppPriceProcessor _sut;

        public AppPriceProcessorTests()
        {
            _sut = new AppPriceProcessor(_notificationJobRepository);
        }

        [Fact]
        public async Task Process_TriggersNotification_WhenFullDiscountDetected()
        {
            // Given
            var appId = _fixture.Create<int>();
            var loopId = _fixture.Create<Guid>();

            var app = _fixture.Create<AppEntity>() with
            {
                Id = appId,
                LastDiscountPercentage = 0,
            };

            var appPrices = new Dictionary<int, AppPriceInfo>
            {
                [appId] = CreateAppPriceInfoWithFullDiscount(),
            };

            NotificationJobEntity createdNotificationJob = null;
            _notificationJobRepository.When(x => x.EnqueueAsync(Arg.Any<NotificationJobEntity>()))
                                      .Do(x => createdNotificationJob = x.ArgAt<NotificationJobEntity>(0));

            // When
            await _sut.ProcessAsync(app, appPrices, loopId);

            // Then
            _notificationJobRepository.Received(1).EnqueueAsync(Arg.Any<NotificationJobEntity>()).Wait();
            createdNotificationJob.Should().NotBeNull();
            createdNotificationJob.AppId.Should().Be(appId);
            createdNotificationJob.AppName.Should().Be(app.Name);
            createdNotificationJob.Id.Should().Be(0);
        }
        
        private AppPriceInfo CreateAppPriceInfoWithFullDiscount()
        {
            return new()
            {
                Success = true,
                Data = new AppPriceInfo.ItemData
                {
                    PriceOverview = _fixture.Create<AppPriceInfo.ItemData.ItemPriceOverview>() with
                    {
                        Initial = _fixture.Create<int>(),
                        Final = 0,
                        DiscountPercent = 100,
                    },
                },
            };
        }

        [Fact]
        public async Task Process_ReturnsCorrectly_WhenAppPriceNotFound()
        {
            // Arrange
            var appId = _fixture.Create<int>();
            var loopId = _fixture.Create<Guid>();
            var appPrices = new Dictionary<int, AppPriceInfo>();

            var app = _fixture.Create<AppEntity>() with
            {
                Id = appId,
                LastDiscountPercentage = 0,
            };

            // Act
            var returnedApp = await _sut.ProcessAsync(app, appPrices, loopId);

            // Assert

            returnedApp.Should().BeEquivalentTo(app, cfg
                => cfg.Excluding(x => x.PriceFetchId)
                      .Excluding(x => x.LastDiscountPercentage)
                      .ComparingByMembers<AppEntity>());

            returnedApp.PriceFetchId.Should().Be(loopId);
            returnedApp.LastDiscountPercentage.Should().Be(double.NaN);
        }
        
        [Fact]
        public async Task Process_ReturnsCorrectly_WhenAppPriceFound()
        {
            // Arrange
            var appId = _fixture.Create<int>();
            var loopId = _fixture.Create<Guid>();
            var discountPercent = _fixture.Create<double>() % 100;
            
            var appPrices = new Dictionary<int, AppPriceInfo>()
            {
                [appId] = new AppPriceInfo()
                {
                    Success = true,
                    Data = new AppPriceInfo.ItemData()
                    {
                        PriceOverview = _fixture.Create<AppPriceInfo.ItemData.ItemPriceOverview>() with
                        {
                            DiscountPercent = discountPercent,
                        },
                    },
                },
            };

            var app = _fixture.Create<AppEntity>() with
            {
                Id = appId,
                LastDiscountPercentage = 0,
            };

            // Act
            var returnedApp = await _sut.ProcessAsync(app, appPrices, loopId);

            // Assert
            returnedApp.Should().BeEquivalentTo(app, cfg
                => cfg.Excluding(x => x.PriceFetchId)
                      .Excluding(x => x.LastDiscountPercentage)
                      .ComparingByMembers<AppEntity>());

            returnedApp.PriceFetchId.Should().Be(loopId);
            returnedApp.LastDiscountPercentage.Should().Be(discountPercent);
        }
    }
}