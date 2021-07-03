using System;
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
    public class NotificationJobRepositoryTests : IDisposable
    {
        private readonly MockDbContext _dbContext;
        private readonly NotificationJobRepository _sut;
        
        private readonly Fixture _fixture = new();

        public NotificationJobRepositoryTests()
        {
            DapperTypeHandlerInitializer.Init();
            _dbContext = new MockDbContext();
            _sut = new NotificationJobRepository(_dbContext);
        }

        [Fact]
        public async Task Enqueue_CreatesProperly()
        {
            // Arrange
            var expectedJob = _fixture.Create<NotificationJobEntity>() with
            {
                CreatedUtc = DateTimeOffset.FromUnixTimeMilliseconds(_fixture.Create<long>()),
            };

            // Act
            await _sut.EnqueueAsync(expectedJob);
            _dbContext.CommitMockTransaction();
            
            // Assert
            var connection = await _dbContext.GetConnectionAsync();
            var actualJobs = connection.Query<NotificationJobEntity>("SELECT * FROM NotificationJobs").ToList();

            actualJobs.Count.Should().Be(1);
            var actualJob = actualJobs[0];
            
            actualJob.AppId.Should().Be(expectedJob.AppId);
            actualJob.AppName.Should().Be(expectedJob.AppName);
            actualJob.CreatedUtc.Should().Be(expectedJob.CreatedUtc);
            actualJob.HasBeenProcessed.Should().Be(expectedJob.HasBeenProcessed);
        }

        [Fact]
        public async Task Dequeue_ReturnsAndUpdatesProperly()
        {
            // Arrange
            var expectedJob = _fixture.Create<NotificationJobEntity>() with
            {
                HasBeenProcessed = false,
                CreatedUtc = DateTimeOffset.FromUnixTimeMilliseconds(_fixture.Create<long>()),
            };

            var connection = await _dbContext.GetConnectionAsync();
            const string insert = "INSERT INTO NotificationJobs(AppId, AppName, CreatedUtc, HasBeenProcessed) " +
                                  "VALUES (@AppId, @AppName, @CreatedUtc, @HasBeenProcessed)";
            await connection.ExecuteAsync(insert, expectedJob);
            
            // Act
            var actualJob = await _sut.DequeueAsync();
            _dbContext.CommitMockTransaction();
            
            // Assert
            actualJob.Should().NotBeNull();
            actualJob.AppId.Should().Be(expectedJob.AppId);
            actualJob.AppName.Should().Be(expectedJob.AppName);
            actualJob.CreatedUtc.Should().Be(expectedJob.CreatedUtc);
            actualJob.HasBeenProcessed.Should().Be(expectedJob.HasBeenProcessed);

            // check if the HasBeenProcessed flag was set after the job had been dequeued
            var hasBeenProcessedNow = connection.QueryFirst<bool>("SELECT HasBeenProcessed FROM NotificationJobs");
            hasBeenProcessedNow.Should().BeTrue();
        }

        [Fact]
        public async Task Dequeue_ReturnsNullIfNoRecords()
        {
            // Act
            var actualJob = await _sut.DequeueAsync();
            
            // Assert
            actualJob.Should().BeNull();
        }
        
        [Fact]
        public async Task Dequeue_ReturnsNullIfAllProcessed()
        {
            // test the functionality of the HasBeenProcessed flag
            
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                var job = _fixture.Create<NotificationJobEntity>() with
                {
                    HasBeenProcessed = true,
                };

                var connection = await _dbContext.GetConnectionAsync();
                const string insert = "INSERT INTO NotificationJobs(AppId, AppName, CreatedUtc, HasBeenProcessed) " +
                                      "VALUES (@AppId, @AppName, @CreatedUtc, @HasBeenProcessed)";
                await connection.ExecuteAsync(insert, job);
            }
            
            // Act
            var actualJob = await _sut.DequeueAsync();

            // Assert
            actualJob.Should().BeNull();
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}