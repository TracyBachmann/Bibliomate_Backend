using BackendBiblioMate.Helpers;

namespace UnitTestsBiblioMate.Helpers
{
    /// <summary>
    /// Tests for <see cref="PagedResult{T}"/> and its extension methods.
    /// </summary>
    public class PagedResultTests
    {
        /// <summary>
        /// Create should throw if pageNumber is less than 1.
        /// </summary>
        [Fact]
        public void Create_InvalidPageNumber_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => PagedResult<int>.Create(items: null, pageNumber: 0, pageSize: 10, totalCount: 0));
        }

        /// <summary>
        /// Create should throw if pageSize is less than 1.
        /// </summary>
        [Fact]
        public void Create_InvalidPageSize_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => PagedResult<int>.Create(items: null, pageNumber: 1, pageSize: 0, totalCount: 0));
        }

        /// <summary>
        /// Create should throw if totalCount is negative.
        /// </summary>
        [Fact]
        public void Create_NegativeTotalCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => PagedResult<int>.Create(items: null, pageNumber: 1, pageSize: 1, totalCount: -1));
        }

        /// <summary>
        /// Create should treat a null items collection as empty.
        /// </summary>
        [Fact]
        public void Create_NullItems_TreatsAsEmpty()
        {
            var result = PagedResult<string>.Create(items: null, pageNumber: 1, pageSize: 5, totalCount: 0);
            Assert.Empty(result.Items);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(5, result.PageSize);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.TotalPages);
        }

        /// <summary>
        /// TotalPages should compute ceiling(totalCount/pageSize).
        /// </summary>
        [Fact]
        public void TotalPages_ComputedCorrectly()
        {
            var result = PagedResult<int>.Create(Enumerable.Range(1,10), pageNumber: 1, pageSize: 10, totalCount: 23);
            Assert.Equal(3, result.TotalPages);
        }

        /// <summary>
        /// ToPagedResultAsync should return the correct slice and metadata.
        /// </summary>
        [Fact]
        public async Task ToPagedResultAsync_ReturnsCorrectSlice()
        {
            // Prepare an in-memory EF-like IQueryable
            var source = Enumerable.Range(1, 25).AsQueryable();

            var paged = await source.ToPagedResultAsync(pageNumber: 2, pageSize: 10, CancellationToken.None);

            Assert.Equal(2, paged.PageNumber);
            Assert.Equal(10, paged.PageSize);
            Assert.Equal(25, paged.TotalCount);
            Assert.Equal(3, paged.TotalPages);

            var items = paged.Items.ToList();
            Assert.Equal(10, items.Count);
            Assert.Equal(11, items.First());
            Assert.Equal(20, items.Last());
        }

        /// <summary>
        /// ToPagedResultAsync should throw if pageNumber or pageSize are invalid.
        /// </summary>
        [Fact]
        public async Task ToPagedResultAsync_InvalidArgs_Throws()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => Enumerable.Empty<int>().AsQueryable()
                      .ToPagedResultAsync(pageNumber: 0, pageSize: 5));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => Enumerable.Empty<int>().AsQueryable()
                      .ToPagedResultAsync(pageNumber: 1, pageSize: 0));
        }
    }
}