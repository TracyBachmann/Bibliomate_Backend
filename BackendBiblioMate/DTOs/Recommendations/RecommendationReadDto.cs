namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving recommended books for a user.
    /// Contains key details for display in recommendation lists.
    /// </summary>
    public class RecommendationReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the recommended book.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; init; }

        /// <summary>
        /// Gets the title of the recommended book.
        /// </summary>
        /// <example>The Hobbit</example>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets the name of the genre of the recommended book.
        /// </summary>
        /// <example>Fantasy</example>
        public string Genre { get; init; } = string.Empty;

        /// <summary>
        /// Gets the name of the author of the recommended book.
        /// </summary>
        /// <example>J.R.R. Tolkien</example>
        public string Author { get; init; } = string.Empty;

        /// <summary>
        /// Gets the URL of the recommended book’s cover image.
        /// </summary>
        /// <example>https://example.com/covers/the-hobbit.jpg</example>
        public string CoverUrl { get; init; } = string.Empty;
    }
}