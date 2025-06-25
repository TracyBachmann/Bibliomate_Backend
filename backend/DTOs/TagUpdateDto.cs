using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing tag.
    /// </summary>
    public class TagUpdateDto
    {
        /// <summary>
        /// Unique identifier of the tag to update.
        /// </summary>
        /// <example>10</example>
        [Required(ErrorMessage = "TagId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "TagId must be a positive integer.")]
        public int TagId { get; set; }

        /// <summary>
        /// Updated name of the tag.
        /// </summary>
        /// <example>Classic</example>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 50 characters.")]
        public string Name { get; set; } = string.Empty;
    }
}