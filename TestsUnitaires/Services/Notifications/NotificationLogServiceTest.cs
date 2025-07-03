using BackendBiblioMate.Models.Enums;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Services.Notifications;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Moq;

namespace TestsUnitaires.Services.Notifications
{
    /// <summary>
    /// Unit tests for <see cref="NotificationLogService"/>,
    /// verifying that log entries are inserted correctly
    /// and retrieved in descending order by timestamp.
    /// </summary>
    public class NotificationLogServiceTest
    {
        private readonly NotificationLogService _service;
        private readonly Mock<IMongoCollection<NotificationLogDocument>> _mockCollection;

        public NotificationLogServiceTest()
        {
            // Arrange a real service wired with test configuration
            var inMem = new Dictionary<string, string?>
            {
                ["MongoDb:ConnectionString"] = "mongodb://localhost:27017",
                ["MongoDb:DatabaseName"]     = "TestDb"
            };
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMem)
                .Build();

            _service = new NotificationLogService(config);

            // Replace the private _collection field with our mock
            _mockCollection = new Mock<IMongoCollection<NotificationLogDocument>>();
            var field = typeof(NotificationLogService)
                .GetField("_collection",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)!;
            field.SetValue(_service, _mockCollection.Object);
        }

        /// <summary>
        /// Calling LogAsync should insert a new document
        /// with the correct properties and a SentAt timestamp ≤ now.
        /// </summary>
        [Fact]
        public async Task LogAsync_ShouldInsertDocument()
        {
            // Arrange
            int userId = 5;
            var type = NotificationType.Info;
            string message = "Test message";

            // Act
            await _service.LogAsync(userId, type, message);

            // Assert: verify InsertOneAsync called once with a document matching our inputs
            _mockCollection.Verify(c => c.InsertOneAsync(
                It.Is<NotificationLogDocument>(d =>
                    d.UserId  == userId &&
                    d.Type    == type &&
                    d.Message == message &&
                    d.SentAt  <= DateTime.UtcNow),
                null,
                default
            ), Times.Once);
        }

        /// <summary>
        /// Calling GetByUserAsync should return all matching documents
        /// sorted in descending order by SentAt.
        /// </summary>
        [Fact]
        public async Task GetByUserAsync_ShouldReturnLogsInDescendingOrder()
        {
            // Arrange
            int userId = 7;
            var docs = new List<NotificationLogDocument>
            {
                new() { UserId = userId, Type = NotificationType.Warning, Message = "Warn", SentAt = DateTime.UtcNow.AddMinutes(-1) },
                new() { UserId = userId, Type = NotificationType.Error,   Message = "Err",  SentAt = DateTime.UtcNow             }
            };

            // Mock IAsyncCursor to return our list once
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

            // When FindAsync is called, return our cursor
            _mockCollection
                .Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<NotificationLogDocument>>(),
                    It.IsAny<FindOptions<NotificationLogDocument, NotificationLogDocument>>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _service.GetByUserAsync(userId);

            // Assert: must have two items, newest first
            Assert.Equal(2, result.Count);
            Assert.Equal("Err", result.First().Message);
        }
    }
}