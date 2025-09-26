using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to create a new genre.
    /// Contains the minimal required information for genre creation.
    /// </summary>
    public class GenreCreateDto
    {
        /// <summary>
        /// Gets or sets the name of the genre.
        /// </summary>
        /// <remarks>
        /// Must be between 2 and 50 characters.
        /// </remarks>
        /// <example>Science Fiction</example>
        [Required(ErrorMessage = "Genre name is required.")]
        [MinLength(2, ErrorMessage = "Genre name must be at least 2 characters long.")]
        [MaxLength(50, ErrorMessage = "Genre name cannot exceed 50 characters.")]
        public string Name { get; init; } = string.Empty;
    }
}