namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object returned when retrieving author information.
    /// Provides the identifier and full name of the author.
    /// </summary>
    public class AuthorReadDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the author.
        /// </summary>
        /// <example>1</example>
        public int AuthorId { get; init; }

        /// <summary>
        /// Gets or sets the full name of the author.
        /// </summary>
        /// <example>J.K. Rowling</example>
        public string Name { get; init; } = string.Empty;
    }
}