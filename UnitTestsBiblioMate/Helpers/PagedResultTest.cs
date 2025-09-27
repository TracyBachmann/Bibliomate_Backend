using BackendBiblioMate.Helpers;

namespace UnitTestsBiblioMate.Helpers
{
    /// <summary>
    /// Unit tests for <see cref="PagedResult{T}"/> and its extension methods.
    /// Validates input validation, metadata calculation, and data slicing.
    /// </summary>
    public class PagedResultTests
    {
        /// <summary>
        /// Verifies that <see cref="PagedResult{T}.Create"/> throws
        /// an <see cref="ArgumentOutOfRangeException"/> when <c>pageNumber &lt; 1</c>.
        /// </summary>
        [Fact]
        public void Create_InvalidPageNumber_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => PagedResult<int>.Create(items: null, pageNumber: 0, pageSize: 10, totalCount: 0));
        }

        /// <summary>
        /// Verifies that <see cref="PagedResult{T}.Create"/> throws
        /// an <see cref="ArgumentOutOfRangeException"/> when <c>pageSize &lt; 1</c>.
        /// </summary>
        [Fact]
        public void Create_InvalidPageSize_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => PagedResult<int>.Create(items: null, pageNumber: 1, pageSize: 0, totalCount: 0));
        }

        /// <summary>
        /// Verifies that <see cref="PagedResult{T}.Create"/> throws
        /// an <see cref="ArgumentOutOfRangeException"/> when <c>totalCount &lt; 0</c>.
        /// </summary>
        [Fact]
        public void Create_NegativeTotalCount_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => PagedResult<int>.Create(items: null, pageNumber: 1, pageSize: 1, totalCount: -1));
        }

        /// <summary>
        /// Ensures that <see cref="PagedResult{T}.Create"/> treats a null <c>items</c> collection as empty.
        /// Also validates that metadata (page number, size, counts) is initialized correctly.
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
        /// Ensures that <see cref="PagedResult{T}.TotalPages"/> is computed
        /// as the mathematical ceiling of <c>totalCount / pageSize</c>.
        /// </summary>
        [Fact]
        public void TotalPages_ComputedCorrectly()
        {
            var result = PagedResult<int>.Create(Enumerable.Range(1, 10), pageNumber: 1, pageSize: 10, totalCount: 23);
            Assert.Equal(3, result.TotalPages);
        }

        /// <summary>
        /// Ensures that <see cref="PagedResultExtensions.ToPagedResultAsync{T}"/> 
        /// returns the correct slice of items and accurate metadata.
        /// </summary>
        [Fact]
        public async Task ToPagedResultAsync_ReturnsCorrectSlice()
        {
            // Arrange: simulate an EF-like IQueryable with 25 elements
            var source = Enumerable.Range(1, 25).AsQueryable();

            // Act
            var paged = await source.ToPagedResultAsync(pageNumber: 2, pageSize: 10, CancellationToken.None);

            // Assert metadata
            Assert.Equal(2, paged.PageNumber);
            Assert.Equal(10, paged.PageSize);
            Assert.Equal(25, paged.TotalCount);
            Assert.Equal(3, paged.TotalPages);

            // Assert slice of items
            var items = paged.Items.ToList();
            Assert.Equal(10, items.Count);
            Assert.Equal(11, items.First());
            Assert.Equal(20, items.Last());
        }

        /// <summary>
        /// Ensures that <see cref="PagedResultExtensions.ToPagedResultAsync{T}"/>
        /// throws an <see cref="ArgumentOutOfRangeException"/> for invalid arguments.
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
