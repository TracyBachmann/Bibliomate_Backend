using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to update an existing tag.
    /// Contains the identifier and the new name for the tag.
    /// </summary>
    public class TagUpdateDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the tag to update.
        /// </summary>
        /// <example>10</example>
        [Required(ErrorMessage = "TagId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "TagId must be a positive integer.")]
        public int TagId { get; init; }

        /// <summary>
        /// Gets or sets the updated name of the tag.
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