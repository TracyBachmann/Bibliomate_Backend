namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving genre information.
    /// </summary>
    public class GenreReadDto
    {
        /// <summary>
        /// Unique identifier of the genre.
        /// </summary>
        /// <example>5</example>
        public int GenreId { get; set; }

        /// <summary>
        /// Name of the genre.
        /// </summary>
        /// <example>Science Fiction</example>
        public string Name { get; set; } = string.Empty;
    }
}