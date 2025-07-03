using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to create a new author.
    /// Contains the minimal required information for author creation.
    /// </summary>
    public class AuthorCreateDto
    {
        /// <summary>
        /// Gets the full name of the author.
        /// </summary>
        /// <remarks>
        /// Must be between 2 and 100 characters.
        /// </remarks>
        /// <example>J.K. Rowling</example>
        [Required(ErrorMessage = "Author name is required.")]
        [MinLength(2, ErrorMessage = "Author name must be at least 2 characters long.")]
        [MaxLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
        public string Name { get; init; } = string.Empty;
    }
}