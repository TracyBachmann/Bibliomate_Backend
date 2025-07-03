namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving tag information.
    /// Contains the unique identifier and name of the tag.
    /// </summary>
    public class TagReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the tag.
        /// </summary>
        /// <example>10</example>
        public int TagId { get; init; }

        /// <summary>
        /// Gets the name of the tag.
        /// </summary>
        /// <example>Classic</example>
        public string Name { get; init; } = string.Empty;
    }
}