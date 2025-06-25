namespace backend.Helpers
{
    /// <summary>
    /// Represents a paged result set, including items and pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of the items in the result set.</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public long TotalCount { get; set; }

        /// <summary>
        /// Total number of pages available.
        /// </summary>
        public int TotalPages
            => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// The items for the current page.
        /// </summary>
        public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    }
}