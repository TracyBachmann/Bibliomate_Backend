using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BackendBiblioMate.Helpers
{
    /// <summary>
    /// Represents a paged result set, including items and pagination metadata.
    /// </summary>
    public sealed class PagedResult<T>
    {
        private PagedResult() { }

        /// <summary>
        /// Factory method to create a new <see cref="PagedResult{T}"/>.
        /// </summary>
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

        public int PageNumber { get; init; }
        public int PageSize   { get; init; }
        public long TotalCount{ get; init; }

        public int TotalPages
            => PageSize > 0
                ? (int)Math.Ceiling(TotalCount / (double)PageSize)
                : 0;

        public IEnumerable<T> Items { get; init; } = Array.Empty<T>();
    }

    public static class PagedResultExtensions
    {
        /// <summary>
        /// Asynchronously creates a <see cref="PagedResult{T}"/> from the given queryable source.
        /// Falls back to synchronous LINQ when the provider does not support IAsyncQueryProvider.
        /// </summary>
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


            if (source.Provider is not IAsyncQueryProvider)
            {
                var totalCount = source.LongCount();
                var items = source
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return PagedResult<T>.Create(items, pageNumber, pageSize, totalCount);
            }

            var countAsync = await source.LongCountAsync(cancellationToken).ConfigureAwait(false);
            var listAsync  = await source
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return PagedResult<T>.Create(listAsync, pageNumber, pageSize, countAsync);
        }
    }
}