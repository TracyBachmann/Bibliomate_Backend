namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object returned when retrieving editor (publisher) data.
    /// Contains the unique identifier and name of the editor.
    /// </summary>
    public class EditorReadDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the editor.
        /// </summary>
        /// <example>3</example>
        public int EditorId { get; init; }

        /// <summary>
        /// Gets or sets the name of the editor or publisher.
        /// </summary>
        /// <example>Penguin Random House</example>
        public string Name { get; init; } = string.Empty;
    }
}