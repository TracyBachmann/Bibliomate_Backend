namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving editor (publisher) data.
    /// </summary>
    public class EditorReadDto
    {
        /// <summary>
        /// Unique identifier of the editor.
        /// </summary>
        /// <example>3</example>
        public int EditorId { get; set; }

        /// <summary>
        /// Name of the editor or publisher.
        /// </summary>
        /// <example>Penguin Random House</example>
        public string Name { get; set; } = string.Empty;
    }
}