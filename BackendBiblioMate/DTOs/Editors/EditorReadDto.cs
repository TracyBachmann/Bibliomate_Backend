namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving editor (publisher) data.
    /// Contains the identifier and name of the editor.
    /// </summary>
    public class EditorReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the editor.
        /// </summary>
        /// <example>3</example>
        public int EditorId { get; init; }

        /// <summary>
        /// Gets the name of the editor or publisher.
        /// </summary>
        /// <example>Penguin Random House</example>
        public string Name { get; init; } = string.Empty;
    }
}