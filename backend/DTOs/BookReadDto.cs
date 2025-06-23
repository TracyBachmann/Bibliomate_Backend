namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving detailed book information.
    /// </summary>
    public class BookReadDto
    {
        /// <summary>
        /// Unique identifier of the book.
        /// </summary>
        /// <example>42</example>
        public int BookId { get; set; }

        /// <summary>
        /// Title of the book.
        /// </summary>
        /// <example>The Hobbit</example>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// International Standard Book Number (ISBN).
        /// </summary>
        /// <example>9780261103344</example>
        public string Isbn { get; set; } = string.Empty;

        /// <summary>
        /// Year the book was published.
        /// </summary>
        /// <example>1937</example>
        public int PublicationYear { get; set; }

        /// <summary>
        /// Full name of the author.
        /// </summary>
        /// <example>J.R.R. Tolkien</example>
        public string AuthorName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the book’s genre.
        /// </summary>
        /// <example>Fantasy</example>
        public string GenreName { get; set; } = string.Empty;

        /// <summary>
        /// Name of the editor/publisher.
        /// </summary>
        /// <example>HarperCollins</example>
        public string EditorName { get; set; } = string.Empty;

        /// <summary>
        /// Indicates if the book is currently available for loan.
        /// </summary>
        /// <example>true</example>
        public bool IsAvailable { get; set; }
        
        /// <summary>
        /// URL of the book’s cover image.
        /// </summary>
        /// <example>https://example.com/covers/the-hobbit.jpg</example>
        public string? CoverUrl { get; set; }

        /// <summary>
        /// List of tags associated with the book.
        /// </summary>
        /// <example>["Classic", "Adventure"]</example>
        public List<string> Tags { get; set; } = new();
    }
}
