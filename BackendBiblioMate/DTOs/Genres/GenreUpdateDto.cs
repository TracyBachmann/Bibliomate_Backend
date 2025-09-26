using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to update an existing genre.
    /// Contains the editable information for a genre record.
    /// </summary>
    public class GenreUpdateDto
    {
        /// <summary>
        /// Gets or sets the new name of the genre.
        /// </summary>
        /// <remarks>
        /// Must be between 2 and 50 characters.
        /// </remarks>
        /// <example>Fantasy</example>
        [Required(ErrorMessage = "Genre name is required.")]
        [MinLength(2, ErrorMessage = "Genre name must be at least 2 characters long.")]
        [MaxLength(50, ErrorMessage = "Genre name cannot exceed 50 characters.")]
        public string Name { get; init; } = string.Empty;
    }
}