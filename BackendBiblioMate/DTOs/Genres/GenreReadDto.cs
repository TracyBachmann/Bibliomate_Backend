namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving genre information.
    /// Contains the identifier and name of the genre.
    /// </summary>
    public class GenreReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the genre.
        /// </summary>
        /// <example>5</example>
        public int GenreId { get; init; }

        /// <summary>
        /// Gets the name of the genre.
        /// </summary>
        /// <example>Science Fiction</example>
        public string Name { get; init; } = string.Empty;
    }
}