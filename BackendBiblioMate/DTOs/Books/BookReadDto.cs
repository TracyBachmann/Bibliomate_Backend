namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving detailed book information.
    /// Contains all relevant fields for display purposes.
    /// </summary>
    public class BookReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the book.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; init; }

        /// <summary>
        /// Gets the title of the book.
        /// </summary>
        /// <example>The Hobbit</example>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets the International Standard Book Number (ISBN).
        /// </summary>
        /// <example>9780261103344</example>
        public string Isbn { get; init; } = string.Empty;

        /// <summary>
        /// Gets the description or synopsis of the book.
        /// </summary>
        /// <example>An epic tale of hobbits, dragons, and adventure.</example>
        public string? Description { get; init; }

        /// <summary>
        /// Gets the year the book was published.
        /// </summary>
        /// <example>1937</example>
        public int PublicationYear { get; init; }

        /// <summary>
        /// Gets the full name of the author.
        /// </summary>
        /// <example>J.R.R. Tolkien</example>
        public string AuthorName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the name of the book’s genre.
        /// </summary>
        /// <example>Fantasy</example>
        public string GenreName { get; init; } = string.Empty;

        /// <summary>
        /// Gets the name of the editor/publisher.
        /// </summary>
        /// <example>HarperCollins</example>
        public string EditorName { get; init; } = string.Empty;

        /// <summary>
        /// Gets a value indicating whether the book is currently available for loan.
        /// </summary>
        /// <example>true</example>
        public bool IsAvailable { get; init; }

        /// <summary>
        /// Gets the URL of the book’s cover image.
        /// </summary>
        /// <example>https://example.com/covers/the-hobbit.jpg</example>
        public string? CoverUrl { get; init; }

        /// <summary>
        /// Gets the list of tag names associated with the book.
        /// </summary>
        /// <remarks>
        /// Each entry represents a descriptive label (e.g., “Classic”, “Adventure”).
        /// </remarks>
        /// <example>["Classic", "Adventure"]</example>
        public IList<string> Tags { get; init; } = new List<string>();
    }
}
