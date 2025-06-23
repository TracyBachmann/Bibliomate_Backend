using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create or update an editor (publisher).
    /// </summary>
    public class EditorCreateDto
    {
        /// <summary>
        /// Name of the editor or publisher.
        /// </summary>
        /// <example>Penguin Random House</example>
        [Required(ErrorMessage = "Editor name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Editor name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;
    }
}