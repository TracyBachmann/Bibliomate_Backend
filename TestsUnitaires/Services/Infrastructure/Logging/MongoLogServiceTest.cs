using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Services.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Moq;

namespace TestsUnitaires.Services.Infrastructure.Logging
{
    /// <summary>
    /// Unit tests for <see cref="MongoLogService"/>.
    /// Verifies constructor validation, AddAsync, GetAllAsync and GetByIdAsync behaviors.
    /// </summary>
    public class MongoLogServiceTest
    {
        private readonly Mock<INotificationLogCollection> _collectionMock;
        private readonly Mock<ILogger<MongoLogService>> _loggerMock;

        /// <summary>
        /// Initializes mocks for the test suite.
        /// </summary>
        public MongoLogServiceTest()
        {
            _collectionMock = new Mock<INotificationLogCollection>();
            _loggerMock = new Mock<ILogger<MongoLogService>>();
        }

        /// <summary>
        /// Verifies that providing a null collection dependency throws an ArgumentNullException.
        /// </summary>
        [Fact]
        public void Constructor_NullCollection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MongoLogService(null!, _loggerMock.Object));
        }

        /// <summary>
        /// Verifies that providing a null logger dependency throws an ArgumentNullException.
        /// </summary>
        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MongoLogService(_collectionMock.Object, null!));
        }

        /// <summary>
        /// Verifies that AddAsync throws an ArgumentNullException when given a null document.
        /// </summary>
        [Fact]
        public async Task AddAsync_NullLog_ThrowsArgumentNullException()
        {
            var service = new MongoLogService(_collectionMock.Object, _loggerMock.Object);
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.AddAsync(null!, CancellationToken.None));
        }

        /// <summary>
        /// Verifies that AddAsync calls InsertOneAsync exactly once with the correct document.
        /// </summary>
        [Fact]
        public async Task AddAsync_ValidLog_CallsInsertOneAsyncOnce()
        {
            var service = new MongoLogService(_collectionMock.Object, _loggerMock.Object);
            var log = new NotificationLogDocument { Id = "1", SentAt = DateTime.UtcNow };

            await service.AddAsync(log, CancellationToken.None);

            _collectionMock.Verify(c => c.InsertOneAsync(log, It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that GetAllAsync returns the expected list of documents from the collection.
        /// </summary>
        [Fact]
        public async Task GetAllAsync_ReturnsListFromCollection()
        {
            var docs = new List<NotificationLogDocument>
            {
                new() { Id = "1", SentAt = DateTime.UtcNow.AddHours(-1) },
                new() { Id = "2", SentAt = DateTime.UtcNow }
            };

            _collectionMock
                .Setup(c => c.GetAllSortedAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(docs);

            var service = new MongoLogService(_collectionMock.Object, _loggerMock.Object);

            var result = await service.GetAllAsync(CancellationToken.None);

            Assert.Equal(docs, result);
        }

        /// <summary>
        /// Verifies that GetByIdAsync throws an ArgumentException when given an empty ID.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_EmptyId_ThrowsArgumentException()
        {
            var service = new MongoLogService(_collectionMock.Object, _loggerMock.Object);
            await Assert.ThrowsAsync<ArgumentException>(() => service.GetByIdAsync("  ", CancellationToken.None));
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns the expected document when found.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_ValidId_ReturnsDocument()
        {
            var expected = new NotificationLogDocument { Id = "123", SentAt = DateTime.UtcNow };

            _collectionMock
                .Setup(c => c.GetByIdAsync("123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var service = new MongoLogService(_collectionMock.Object, _loggerMock.Object);

            var actual = await service.GetByIdAsync("123", CancellationToken.None);

            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Verifies that GetByIdAsync returns null when no matching document is found.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_DocumentNotFound_ReturnsNull()
        {
            _collectionMock
                .Setup(c => c.GetByIdAsync("nonexistent", It.IsAny<CancellationToken>()))
                .ReturnsAsync((NotificationLogDocument?)null);

            var service = new MongoLogService(_collectionMock.Object, _loggerMock.Object);

            var actual = await service.GetByIdAsync("nonexistent", CancellationToken.None);

            Assert.Null(actual);
        }
    }
}