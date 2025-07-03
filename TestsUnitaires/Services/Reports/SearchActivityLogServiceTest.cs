using BackendBiblioMate.Interfaces;
using BackendBiblioMate.Models.Mongo;

namespace TestsUnitaires.Services.Reports
{
    /// <summary>
    /// Fake implementation of <see cref="ISearchActivityLogService"/> for unit tests.
    /// </summary>
    public class SearchActivityLogServiceFake : ISearchActivityLogService
    {
        /// <summary>
        /// Logs a search activity (no-op).
        /// </summary>
        public Task LogAsync(
            SearchActivityLogDocument doc,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        /// <summary>
        /// Retrieves search activities for a user (always returns an empty list).
        /// </summary>
        public Task<List<SearchActivityLogDocument>> GetByUserAsync(
            int userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new List<SearchActivityLogDocument>());
    }

    /// <summary>
    /// Unit tests for the fake <see cref="ISearchActivityLogService"/>.
    /// Verifies that methods do not throw and return expected results.
    /// </summary>
    public class SearchActivityLogServiceTest
    {
        private readonly ISearchActivityLogService _fake = new SearchActivityLogServiceFake();

        /// <summary>
        /// Ensures that LogAsync does not throw an exception.
        /// </summary>
        [Fact]
        public async Task LogAsync_DoesNotThrow()
        {
            // Arrange
            var doc = new SearchActivityLogDocument
            {
                UserId    = 42,
                QueryText = "test query",
                Timestamp = DateTime.UtcNow
            };

            // Act & Assert
            await _fake.LogAsync(doc, CancellationToken.None);
        }

        /// <summary>
        /// Ensures that GetByUserAsync returns a non-null, empty list.
        /// </summary>
        [Fact]
        public async Task GetByUserAsync_ReturnsEmptyList()
        {
            // Act
            var list = await _fake.GetByUserAsync(123, CancellationToken.None);

            // Assert
            Assert.NotNull(list);
            Assert.Empty(list);
        }
    }
}