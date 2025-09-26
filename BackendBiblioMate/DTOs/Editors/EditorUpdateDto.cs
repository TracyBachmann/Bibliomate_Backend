using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to update an existing editor (publisher).
    /// Contains the editable information for an editor record.
    /// </summary>
    public class EditorUpdateDto
    {
        /// <summary>
        /// Gets or sets the new name of the editor or publisher.
        /// </summary>
        /// <remarks>
        /// Must be between 2 and 100 characters.
        /// </remarks>
        /// <example>HarperCollins</example>
        [Required(ErrorMessage = "Editor name is required.")]
        [MinLength(2, ErrorMessage = "Editor name must be at least 2 characters long.")]
        [MaxLength(100, ErrorMessage = "Editor name cannot exceed 100 characters.")]
        public string Name { get; init; } = string.Empty;
    }
}