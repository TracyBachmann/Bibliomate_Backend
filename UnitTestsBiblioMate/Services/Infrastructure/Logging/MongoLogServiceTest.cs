using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Services.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTestsBiblioMate.Services.Infrastructure.Logging
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

        // ---------------- Constructor validation ----------------

        /// <summary>
        /// Verifies that providing a null collection dependency
        /// throws an ArgumentNullException.
        /// </summary>
        [Fact]
        public void Constructor_NullCollection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MongoLogService(null!, _loggerMock.Object));
        }

        /// <summary>
        /// Verifies that providing a null logger dependency
        /// throws an ArgumentNullException.
        /// </summary>
        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new MongoLogService(_collectionMock.Object, null!));
        }

        // ---------------- AddAsync ----------------

        /// <summary>
        /// AddAsync should throw an ArgumentNullException
        /// if the provided log document is null.
        /// </summary>
        [Fact]
        public async Task AddAsync_NullLog_ThrowsArgumentNullException()
        {
            var service = new MongoLogService(_collectionMock.Object, _loggerMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => service.AddAsync(null!, CancellationToken.None));
        }

        /// <summary>
        /// AddAsync should delegate to InsertOneAsync exactly once
        /// when given a valid log document.
        /// </summary>
        [Fact]
        public async Task AddAsync_ValidLog_CallsInsertOneAsyncOnce()
        {
            var service = new MongoLogService(_collectionMock.Object, _loggerMock.Object);
            var log = new NotificationLogDocument { Id = "1", SentAt = DateTime.UtcNow };

            await service.AddAsync(log, CancellationToken.None);

            _collectionMock.Verify(
                c => c.InsertOneAsync(log, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ---------------- GetAllAsync ----------------

        /// <summary>
        /// GetAllAsync should return exactly the list provided by the collection.
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

        // ---------------- GetByIdAsync ----------------

        /// <summary>
        /// GetByIdAsync should throw an ArgumentException
        /// when the provided ID is null, empty, or whitespace.
        /// </summary>
        [Fact]
        public async Task GetByIdAsync_EmptyId_ThrowsArgumentException()
        {
            var service = new MongoLogService(_collectionMock.Object, _loggerMock.Object);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetByIdAsync("  ", CancellationToken.None));
        }

        /// <summary>
        /// GetByIdAsync should return the matching document when found.
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
        /// GetByIdAsync should return null when the collection
        /// does not contain a document with the given ID.
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
