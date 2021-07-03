using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Dapper;
using FluentAssertions;
using Steamboat.Data;
using Steamboat.Data.Repos;
using Xunit;

namespace Tests.Repositories
{
    public class StateEntryRepositoryTests : IDisposable
    {
        private readonly MockDbContext _dbContext;

        private readonly Fixture _fixture = new();
        private readonly StateEntryRepository _sut;

        public StateEntryRepositoryTests()
        {
            DapperTypeHandlerInitializer.Init();
            _dbContext = new MockDbContext();
            _sut = new StateEntryRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }

        [Fact]
        public async Task StoreValueAsync_CreatesValue()
        {
            // Arrange
            var expectedKey = _fixture.Create<string>();
            var expectedValue = _fixture.Create<string>();

            // Act
            await _sut.StoreValueAsync(expectedKey, expectedValue);
            _dbContext.CommitMockTransaction();

            // Assert
            var connection = await _dbContext.GetConnectionAsync();
            var actualPair = connection.Query<(string Key, string Value)>(
                "SELECT * FROM StateEntries WHERE Key = @Key",
                new { Key = expectedKey }).First();

            actualPair.Key.Should().Be(expectedKey);
            actualPair.Value.Should().Be(expectedValue);
        }

        [Fact]
        public async Task StoreValueAsync_OverridesExistingValue()
        {
            // Arrange
            var expectedKey = _fixture.Create<string>();
            var oldValue = _fixture.Create<string>();
            var expectedValue = _fixture.Create<string>();
            
            var connection = await _dbContext.GetConnectionAsync();

            connection.Execute(
                "INSERT INTO StateEntries(Key, Value) VALUES (@Key, @Value)",
                new { Key = expectedKey, Value = oldValue });
            
            // Act
            await _sut.StoreValueAsync(expectedKey, expectedValue);
            _dbContext.CommitMockTransaction();
            
            // Assert
            var queryResults = connection.Query<(string Key, string Value)>(
                "SELECT * FROM StateEntries WHERE Key = @Key",
                new { Key = expectedKey }).ToList();

            queryResults.Count.Should().Be(1);
            var actualPair = queryResults[0];
            
            actualPair.Key.Should().Be(expectedKey);
            actualPair.Value.Should().Be(expectedValue);
        }

        [Fact]
        public async Task GetValueAsync_RetrievesExistingValue()
        {
            // Arrange
            var expectedKey = _fixture.Create<string>();
            var expectedValue = _fixture.Create<string>();
            
            var connection = await _dbContext.GetConnectionAsync();
            
            connection.Execute(
                "INSERT INTO StateEntries(Key, Value) VALUES (@Key, @Value)",
                new { Key = expectedKey, Value = expectedValue });
            
            // Act
            var actualValue = await _sut.GetValueAsync(expectedKey);
            _dbContext.CommitMockTransaction();
            
            // Assert
            actualValue.Should().Be(expectedValue);
        }
        
        [Fact]
        public async Task GetValueAsync_ReturnsNullWhenPairNotPresent()
        {
            // Arrange
            var key = _fixture.Create<string>();
            
            // Act
            var actualValue = await _sut.GetValueAsync(key);
            _dbContext.CommitMockTransaction();
            
            // Assert
            actualValue.Should().Be(null);
        }
    }
}