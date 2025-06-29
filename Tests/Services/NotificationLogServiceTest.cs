using backend.Models.Enums;
using backend.Models.Mongo;
using backend.Services;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Moq;

namespace Tests.Services
{
    public class NotificationLogServiceTest
    {
        private readonly NotificationLogService _service;
        private readonly Mock<IMongoCollection<NotificationLogDocument>> _mockCollection;

        public NotificationLogServiceTest()
        {
            var inMem = new Dictionary<string, string?>
            {
                ["MongoDb:ConnectionString"] = "mongodb://localhost:27017",
                ["MongoDb:DatabaseName"]     = "TestDb"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMem)
                .Build();

            _service = new NotificationLogService(config);

            _mockCollection = new Mock<IMongoCollection<NotificationLogDocument>>();
            var field = typeof(NotificationLogService)
                .GetField("_collection",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)!;
            field.SetValue(_service, _mockCollection.Object);
        }

        [Fact]
        public async Task LogAsync_ShouldInsertDocument()
        {
            int userId = 5;
            var type = NotificationType.Info;
            string message = "Test message";

            await _service.LogAsync(userId, type, message);

            _mockCollection.Verify(c => c.InsertOneAsync(
                It.Is<NotificationLogDocument>(d =>
                    d.UserId == userId &&
                    d.Type   == type    &&
                    d.Message== message &&
                    d.SentAt <= DateTime.UtcNow),
                null,
                default
            ), Times.Once);
        }

        [Fact]
        public async Task GetByUserAsync_ShouldReturnLogsInDescendingOrder()
        {
            int userId = 7;
            var docs = new List<NotificationLogDocument>
            {
                new() { UserId = userId, Type = NotificationType.Warning, Message = "Warn", SentAt = DateTime.UtcNow.AddMinutes(-1) },
                new() { UserId = userId, Type = NotificationType.Error,   Message = "Err",  SentAt = DateTime.UtcNow }
            };

            var mockCursor = new Mock<IAsyncCursor<NotificationLogDocument>>();
            mockCursor.Setup(_ => _.Current).Returns(docs);
            mockCursor
                .SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor
                .SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<NotificationLogDocument>>(),
                    It.IsAny<FindOptions<NotificationLogDocument, NotificationLogDocument>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            var result = await _service.GetByUserAsync(userId);

            Assert.Equal(2, result.Count);
            Assert.Equal("Err", result.First().Message);
        }
    }
}
