using Microsoft.EntityFrameworkCore;

namespace BackendBiblioMate.Helpers
{
    /// <summary>
    /// Represents a paged result set, including items and pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of the items in the result set.</typeparam>
    public sealed class PagedResult<T>
    {
        private PagedResult() { }

        /// <summary>
        /// Factory method to create a new <see cref="PagedResult{T}"/>.
        /// </summary>
        /// <param name="items">The items for the current page. May be <c>null</c>, in which case it's treated as empty.</param>
        /// <param name="pageNumber">Current page number (1-based).</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <param name="totalCount">Total number of items across all pages.</param>
        /// <returns>A new instance of <see cref="PagedResult{T}"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="pageNumber"/> &lt; 1, <paramref name="pageSize"/> &lt; 1, or <paramref name="totalCount"/> &lt; 0.
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
                Items = items ?? Array.Empty<T>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int PageNumber { get; init; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; init; }

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public long TotalCount { get; init; }

        /// <summary>
        /// Total number of pages available.
        /// </summary>
        public int TotalPages
            => PageSize > 0
                ? (int)Math.Ceiling(TotalCount / (double)PageSize)
                : 0;

        /// <summary>
        /// The items for the current page.
        /// </summary>
        public IEnumerable<T> Items { get; init; } = Array.Empty<T>();
    }

    /// <summary>
    /// Extension methods for creating <see cref="PagedResult{T}"/> from an <see cref="IQueryable{T}"/>.
    /// </summary>
    public static class PagedResultExtensions
    {
        /// <summary>
        /// Asynchronously creates a <see cref="PagedResult{T}"/> from the given queryable source.
        /// </summary>
        /// <typeparam name="T">The element type of the source.</typeparam>
        /// <param name="source">Queryable source (e.g., EF Core DbSet).</param>
        /// <param name="pageNumber">Page number (1-based).</param>
        /// <param name="pageSize">Items per page.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="PagedResult{T}"/> containing the requested slice of items
        /// and the total count of items across all pages.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="pageNumber"/> &lt; 1 or <paramref name="pageSize"/> &lt; 1.
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

            var totalCount = await source.LongCountAsync(cancellationToken).ConfigureAwait(false);
            var items = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return PagedResult<T>.Create(items, pageNumber, pageSize, totalCount);
        }
    }
}