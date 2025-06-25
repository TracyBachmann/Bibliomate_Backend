namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving author information.
    /// </summary>
    public class AuthorReadDto
    {
        /// <summary>
        /// Unique identifier of the author.
        /// </summary>
        /// <example>1</example>
        public int AuthorId { get; set; }

        /// <summary>
        /// Full name of the author.
        /// </summary>
        /// <example>J.K. Rowling</example>
        public string Name { get; set; } = string.Empty;
    }
}