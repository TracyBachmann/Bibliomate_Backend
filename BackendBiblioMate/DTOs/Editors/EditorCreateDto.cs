using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to create a new editor (publisher).
    /// Contains the minimal required information for editor creation.
    /// </summary>
    public class EditorCreateDto
    {
        /// <summary>
        /// Gets or sets the name of the editor or publisher.
        /// </summary>
        /// <remarks>
        /// Must be between 2 and 100 characters.
        /// </remarks>
        /// <example>Penguin Random House</example>
        [Required(ErrorMessage = "Editor name is required.")]
        [MinLength(2, ErrorMessage = "Editor name must be at least 2 characters long.")]
        [MaxLength(100, ErrorMessage = "Editor name cannot exceed 100 characters.")]
        public string Name { get; init; } = string.Empty;
    }
}