namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object returned when retrieving genre information.
    /// Provides the identifier and name of the genre.
    /// </summary>
    public class GenreReadDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the genre.
        /// </summary>
        /// <example>5</example>
        public int GenreId { get; init; }

        /// <summary>
        /// Gets or sets the name of the genre.
        /// </summary>
        /// <example>Science Fiction</example>
        public string Name { get; init; } = string.Empty;
    }
}