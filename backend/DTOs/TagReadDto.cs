namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving tag information.
    /// </summary>
    public class TagReadDto
    {
        /// <summary>
        /// Unique identifier of the tag.
        /// </summary>
        /// <example>10</example>
        public int TagId { get; set; }

        /// <summary>
        /// Name of the tag.
        /// </summary>
        /// <example>Classic</example>
        public string Name { get; set; } = string.Empty;
    }
}