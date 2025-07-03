namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving author information.
    /// Contains the identifier and full name of the author.
    /// </summary>
    public class AuthorReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the author.
        /// </summary>
        /// <example>1</example>
        public int AuthorId { get; init; }

        /// <summary>
        /// Gets the full name of the author.
        /// </summary>
        /// <example>J.K. Rowling</example>
        public string Name { get; init; } = string.Empty;
    }
}
