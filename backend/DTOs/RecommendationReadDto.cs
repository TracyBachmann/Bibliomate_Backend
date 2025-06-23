namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving recommended books for a user.
    /// </summary>
    public class RecommendationReadDto
    {
        /// <summary>
        /// Unique identifier of the recommended book.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; set; }

        /// <summary>
        /// Title of the recommended book.
        /// </summary>
        /// <example>The Hobbit</example>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Name of the genre of the recommended book.
        /// </summary>
        /// <example>Fantasy</example>
        public string Genre { get; set; } = string.Empty;

        /// <summary>
        /// Name of the author of the recommended book.
        /// </summary>
        /// <example>J.R.R. Tolkien</example>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// URL of the recommended book’s cover image.
        /// </summary>
        /// <example>https://example.com/covers/the-hobbit.jpg</example>
        public string CoverUrl { get; set; } = string.Empty;
    }
}