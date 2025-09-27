using BackendBiblioMate.Configuration;
using BackendBiblioMate.Models.Mongo;
using BackendBiblioMate.Services.Reports;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;

namespace UnitTestsBiblioMate.Services.Reports
{
    /// <summary>
    /// Unit tests for <see cref="SearchActivityLogService"/>.
    /// Covers constructor behavior, TTL index creation, and document insertion.
    /// </summary>
    public class SearchActivityLogServiceTests
    {
        /// <summary>
        /// Builds a valid <see cref="MongoSettings"/> instance for test purposes.
        /// </summary>
        private static IOptions<MongoSettings> MakeOptions() =>
            Options.Create(new MongoSettings
            {
                ConnectionString = "mongodb://localhost:27017",
                DatabaseName     = "TestDb"
            });

        /// <summary>
        /// The constructor should create a TTL index on the <c>Timestamp</c> field,
        /// with an expiration time of 90 days.
        /// </summary>
        [Fact]
        public void Ctor_CreatesTtlIndexOnTimestamp()
        {
            // Arrange mocks for client, database, collection, and indexes
            var opts           = MakeOptions();
            var mockClient     = new Mock<IMongoClient>(MockBehavior.Strict);
            var mockDatabase   = new Mock<IMongoDatabase>(MockBehavior.Strict);
            var mockCollection = new Mock<IMongoCollection<SearchActivityLogDocument>>(MockBehavior.Strict);
            var mockIndexes    = new Mock<IMongoIndexManager<SearchActivityLogDocument>>(MockBehavior.Strict);

            mockClient
                .Setup(c => c.GetDatabase(opts.Value.DatabaseName, null))
                .Returns(mockDatabase.Object);
            mockDatabase
                .Setup(d => d.GetCollection<SearchActivityLogDocument>("SearchActivityLogs", null))
                .Returns(mockCollection.Object);
            mockCollection
                .SetupGet(c => c.Indexes)
                .Returns(mockIndexes.Object);

            // We do not deeply test index keys rendering,
            // only that an index creation is triggered with the expected expiration.
            mockIndexes
                .Setup(i => i.CreateOne(
                    It.Is<CreateIndexModel<SearchActivityLogDocument>>(m =>
                        m.Options != null &&
                        m.Options.ExpireAfter.HasValue &&
                        m.Options.ExpireAfter.Value == TimeSpan.FromDays(90)),
                    null,
                    default))
                .Returns("idx");

            // Act: construct the service (should trigger index creation)
            _ = new SearchActivityLogService(opts, mockClient.Object);

            // Assert: index creation verified once
            mockIndexes.VerifyAll();
        }

        /// <summary>
        /// LogAsync should insert the provided document into the MongoDB collection.
        /// </summary>
        [Fact]
        public async Task LogAsync_InsertsDocument()
        {
            var opts           = MakeOptions();
            var mockClient     = new Mock<IMongoClient>();
            var mockDatabase   = new Mock<IMongoDatabase>();
            var mockCollection = new Mock<IMongoCollection<SearchActivityLogDocument>>();
            var mockIndexes    = new Mock<IMongoIndexManager<SearchActivityLogDocument>>();

            mockClient.Setup(c => c.GetDatabase(opts.Value.DatabaseName, null))
                      .Returns(mockDatabase.Object);
            mockDatabase.Setup(d => d.GetCollection<SearchActivityLogDocument>("SearchActivityLogs", null))
                        .Returns(mockCollection.Object);
            mockCollection.SetupGet(c => c.Indexes).Returns(mockIndexes.Object);
            mockIndexes.Setup(i => i.CreateOne(It.IsAny<CreateIndexModel<SearchActivityLogDocument>>(), null, default))
                       .Returns("idx");

            var service = new SearchActivityLogService(opts, mockClient.Object);

            var doc = new SearchActivityLogDocument
            {
                UserId    = 123,
                QueryText = "title: clean code",
                Timestamp = DateTime.UtcNow
            };

            // Act
            await service.LogAsync(doc, CancellationToken.None);

            // Assert: verify InsertOneAsync was called once with matching values
            mockCollection.Verify(c => c.InsertOneAsync(
                It.Is<SearchActivityLogDocument>(d =>
                    d.UserId    == 123 &&
                    d.QueryText == "title: clean code" &&
                    d.Timestamp == doc.Timestamp),
                null,
                CancellationToken.None),
                Times.Once);
        }

        /// <summary>
        /// LogAsync should throw an <see cref="ArgumentNullException"/>
        /// when a null document is passed.
        /// </summary>
        [Fact]
        public async Task LogAsync_NullDoc_Throws()
        {
            var service = BuildService(out _);
            await Assert.ThrowsAsync<ArgumentNullException>(() => service.LogAsync(null!));
        }

        /// <summary>
        /// Constructor should throw when options are null.
        /// </summary>
        [Fact]
        public void Ctor_NullOptions_Throws()
        {
            var mockClient = new Mock<IMongoClient>();
            Assert.Throws<ArgumentNullException>(() => new SearchActivityLogService(null!, mockClient.Object));
        }

        /// <summary>
        /// Constructor should throw when client is null.
        /// </summary>
        [Fact]
        public void Ctor_NullClient_Throws()
        {
            var opts = MakeOptions();
            Assert.Throws<ArgumentNullException>(() => new SearchActivityLogService(opts, null!));
        }

        // --------------- helpers ---------------

        /// <summary>
        /// Builds a <see cref="SearchActivityLogService"/> with mocks
        /// and returns both the service and the mock collection.
        /// </summary>
        private static SearchActivityLogService BuildService(
            out Mock<IMongoCollection<SearchActivityLogDocument>> mockCollection)
        {
            var opts           = MakeOptions();
            var mockClient     = new Mock<IMongoClient>();
            var mockDatabase   = new Mock<IMongoDatabase>();
            mockCollection     = new Mock<IMongoCollection<SearchActivityLogDocument>>();
            var mockIndexes    = new Mock<IMongoIndexManager<SearchActivityLogDocument>>();

            mockClient.Setup(c => c.GetDatabase(opts.Value.DatabaseName, null))
                      .Returns(mockDatabase.Object);
            mockDatabase.Setup(d => d.GetCollection<SearchActivityLogDocument>("SearchActivityLogs", null))
                        .Returns(mockCollection.Object);
            mockCollection.SetupGet(c => c.Indexes).Returns(mockIndexes.Object);
            mockIndexes.Setup(i => i.CreateOne(It.IsAny<CreateIndexModel<SearchActivityLogDocument>>(), null, default))
                       .Returns("idx");

            return new SearchActivityLogService(opts, mockClient.Object);
        }
    }
}
