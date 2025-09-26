using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BackendBiblioMate.Helpers
{
    /// <summary>
    /// Represents a paged result set, including both the items and pagination metadata.
    /// Useful for returning paginated results from repositories or API endpoints.
    /// </summary>
    /// <typeparam name="T">The type of items contained in the paginated result.</typeparam>
    public sealed class PagedResult<T>
    {
        private PagedResult() { }

        /// <summary>
        /// Factory method to create a new <see cref="PagedResult{T}"/>.
        /// </summary>
        /// <param name="items">The collection of items for the current page.</param>
        /// <param name="pageNumber">The index of the current page (1-based).</param>
        /// <param name="pageSize">The maximum number of items per page.</param>
        /// <param name="totalCount">The total number of items in the full data set.</param>
        /// <returns>A new <see cref="PagedResult{T}"/> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="pageNumber"/> or <paramref name="pageSize"/> are less than 1,
        /// or if <paramref name="totalCount"/> is negative.
        /// </exception>
        public static PagedResult<T> Create(
            IEnumerable<T>? items,
            int pageNumber,
            int pageSize,
            long totalCount)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "PageNumber must be at least 1.");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be at least 1.");
            if (totalCount < 0)
                throw new ArgumentOutOfRangeException(nameof(totalCount), "TotalCount cannot be negative.");

            return new PagedResult<T>
            {
                Items       = items ?? Array.Empty<T>(),
                PageNumber  = pageNumber,
                PageSize    = pageSize,
                TotalCount  = totalCount
            };
        }

        /// <summary>
        /// Gets the current page index (1-based).
        /// </summary>
        public int PageNumber { get; init; }

        /// <summary>
        /// Gets the maximum number of items per page.
        /// </summary>
        public int PageSize { get; init; }

        /// <summary>
        /// Gets the total number of items in the full queryable data set.
        /// </summary>
        public long TotalCount { get; init; }

        /// <summary>
        /// Gets the total number of pages based on <see cref="TotalCount"/> and <see cref="PageSize"/>.
        /// Returns <c>0</c> if <see cref="PageSize"/> is invalid.
        /// </summary>
        public int TotalPages
            => PageSize > 0
                ? (int)Math.Ceiling(TotalCount / (double)PageSize)
                : 0;

        /// <summary>
        /// Gets the items contained in the current page.
        /// Defaults to an empty collection if none are provided.
        /// </summary>
        public IEnumerable<T> Items { get; init; } = Array.Empty<T>();
    }

    /// <summary>
    /// Provides extension methods for transforming queryable collections into paginated results.
    /// </summary>
    public static class PagedResultExtensions
    {
        /// <summary>
        /// Asynchronously creates a <see cref="PagedResult{T}"/> from the given queryable source.
        /// Falls back to synchronous LINQ operations when the underlying provider does not support async queries.
        /// </summary>
        /// <typeparam name="T">The type of items in the queryable source.</typeparam>
        /// <param name="source">The queryable data source to paginate.</param>
        /// <param name="pageNumber">The index of the page to retrieve (1-based).</param>
        /// <param name="pageSize">The maximum number of items per page.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="PagedResult{T}"/> containing the requested page of items.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="pageNumber"/> or <paramref name="pageSize"/> are less than 1.
        /// </exception>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> source,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "PageNumber must be at least 1.");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "PageSize must be at least 1.");

            // Non-async providers (e.g., in-memory LINQ)
            if (source.Provider is not IAsyncQueryProvider)
            {
                var totalCount = source.LongCount();
                var items = source
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return PagedResult<T>.Create(items, pageNumber, pageSize, totalCount);
            }

            // Async-capable providers (e.g., EF Core)
            var totalCountAsync = await source.LongCountAsync(cancellationToken).ConfigureAwait(false);
            var itemsAsync = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return PagedResult<T>.Create(itemsAsync, pageNumber, pageSize, totalCountAsync);
        }
    }
}
