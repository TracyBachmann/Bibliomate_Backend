using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create a new tag.
    /// </summary>
    public class TagCreateDto
    {
        /// <summary>
        /// Name of the tag.
        /// </summary>
        /// <example>Classic</example>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 50 characters.")]
        public string Name { get; set; } = string.Empty;
    }
}