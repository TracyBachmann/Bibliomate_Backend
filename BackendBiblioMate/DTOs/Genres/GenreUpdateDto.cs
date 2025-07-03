using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to update an existing genre.
    /// Contains the minimal information that can be modified for a genre.
    /// </summary>
    public class GenreUpdateDto
    {
        /// <summary>
        /// Gets the new name of the genre.
        /// </summary>
        /// <remarks>
        /// Must be between 2 and 100 characters.
        /// </remarks>
        /// <example>Science Fiction</example>
        [Required(ErrorMessage = "Genre name is required.")]
        [MinLength(2, ErrorMessage = "Genre name must be at least 2 characters long.")]
        [MaxLength(100, ErrorMessage = "Genre name cannot exceed 100 characters.")]
        public string Name { get; init; } = string.Empty;
    }
}