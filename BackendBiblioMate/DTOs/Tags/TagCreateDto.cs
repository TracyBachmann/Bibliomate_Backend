using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to create a new tag.
    /// Contains the name of the tag to be added to the system.
    /// </summary>
    public class TagCreateDto
    {
        /// <summary>
        /// Gets the name of the tag.
        /// </summary>
        /// <remarks>
        /// Must be between 1 and 50 characters.
        /// </remarks>
        /// <example>Classic</example>
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(1, ErrorMessage = "Name must be at least 1 character long.")]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string Name { get; init; } = string.Empty;
    }
}