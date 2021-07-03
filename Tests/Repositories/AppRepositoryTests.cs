using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Steamboat.Data;
using Steamboat.Data.Entities;
using Steamboat.Data.Repos;
using Xunit;

namespace Tests.Repositories
{
    public class AppRepositoryTests : IDisposable
    {
        private readonly MockDbContext _dbContext;
        private readonly AppRepository _sut;
        
        private readonly Fixture _fixture = new();

        public AppRepositoryTests()
        {
            DapperTypeHandlerInitializer.Init();
            _dbContext = new MockDbContext();
            _sut = new AppRepository(_dbContext);
        }

        [Fact]
        public async Task AddOrUpdateAppsAsync_AddsApps() // other tests in this class depend on this one to succeed
        {
            // Arrange
            const int numApps = 100;
            var apps = GenerateAppsWithAscendingIds(numApps);
            
            // Act
            await _sut.AddOrUpdateAppsAsync(apps, false, false);
            _dbContext.CommitMockTransaction();

            // Assert
            var connection = await _dbContext.GetConnectionAsync();
            var actualApps = connection.Query<AppEntity>("SELECT * FROM Apps").ToList();

            actualApps.Count.Should().Be(numApps);
            actualApps.Should().ContainInOrder(apps);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task AddOrUpdateAppsAsync_UpdatesAppsAndRespectsParams(
            bool updatePriceFetchId,
            bool updateLastDiscountPercentage)
        {
            // Arrange
            const int numApps = 100;
            var oldApps = GenerateAppsWithAscendingIds(numApps);
            var newApps = GenerateAppsWithAscendingIds(numApps);
            
            await _sut.AddOrUpdateAppsAsync(oldApps, false, false); 

            // Act
            await _sut.AddOrUpdateAppsAsync(newApps, updatePriceFetchId, updateLastDiscountPercentage);
            _dbContext.CommitMockTransaction();
            
            // Assert
            var connection = await _dbContext.GetConnectionAsync();
            var actualApps = connection.Query<AppEntity>("SELECT * FROM Apps").ToList();

            actualApps.Count.Should().Be(numApps);

            for (var i = 0; i < numApps; i++)
            {
                var actualApp = actualApps[i];
                var expectedApp = newApps[i];
                var oldApp = oldApps[i];

                if (!updatePriceFetchId)
                {
                    expectedApp = expectedApp with { PriceFetchId = oldApp.PriceFetchId };
                }
                
                if (!updateLastDiscountPercentage)
                {
                    expectedApp = expectedApp with { LastDiscountPercentage = oldApp.LastDiscountPercentage };
                }

                actualApp.Should().Be(expectedApp);
            }
        }

        [Theory]
        [InlineData(10)]
        [InlineData(15)]
        [InlineData(69)]
        public async Task ListAppsAsync_ListsPages(int pageSize)
        {
            // Arrange
            const int numApps = 100;
            var apps = GenerateAppsWithAscendingIds(numApps);
            
            await _sut.AddOrUpdateAppsAsync(apps, false, false); 
            
            // Act
            var actualPages = new List<IList<AppEntity>>();
            
            for (int i = 0; i < numApps / pageSize; i++)
            {
                actualPages.Add(await _sut.ListAppsAsync(i, pageSize));
            }
            
            _dbContext.CommitMockTransaction();
            
            // Assert
            int pageNumber = 0;
            foreach (var actualPage in actualPages)
            {
                var expectedPage = apps.Skip(pageNumber * pageSize).Take(pageSize).ToList();
                actualPage.Should().ContainInOrder(expectedPage);
                actualPage.Count.Should().Be(expectedPage.Count);

                ++pageNumber;
            }
        }

        [Fact]
        public async Task ListAppsAsync_RespectIgnoredPriceFetchId()
        {
            // Arrange
            var idToIgnore = _fixture.Create<Guid>();
            const int numApps = 100;
            var apps = GenerateAppsWithAscendingIds(numApps);

            for (int i = 0; i < numApps; i += 2)
            {
                apps[i] = apps[i] with
                {
                    PriceFetchId = idToIgnore,
                };
            }
            
            await _sut.AddOrUpdateAppsAsync(apps, false, false); 
            
            // Act
            var actualApps = await _sut.ListAppsAsync(0, numApps, idToIgnore);
            _dbContext.CommitMockTransaction();
            
            // Assert
            var expectedApps = apps.Where(x => x.PriceFetchId != idToIgnore);
            actualApps.Should().ContainInOrder(expectedApps);
        }

        private List<AppEntity> GenerateAppsWithAscendingIds(int numApps)
        {
            var appId = 1;
            return _fixture.CreateMany<AppEntity>(numApps)
                           .Select(app => app with { Id = appId++ }) // make sure the IDs are unique
                           .ToList();
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}