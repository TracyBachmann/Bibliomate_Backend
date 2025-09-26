namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object returned when retrieving recommended books for a user.
    /// Contains essential details for display in recommendation lists.
    /// </summary>
    public class RecommendationReadDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the recommended book.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; init; }

        /// <summary>
        /// Gets or sets the title of the recommended book.
        /// </summary>
        /// <example>The Hobbit</example>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the genre of the recommended book.
        /// </summary>
        /// <example>Fantasy</example>
        public string Genre { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the author of the recommended book.
        /// </summary>
        /// <example>J.R.R. Tolkien</example>
        public string Author { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL of the recommended book’s cover image.
        /// </summary>
        /// <remarks>
        /// Must be a valid absolute URL.
        /// </remarks>
        /// <example>https://example.com/covers/the-hobbit.jpg</example>
        public string CoverUrl { get; init; } = string.Empty;
    }
}