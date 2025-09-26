using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used when creating a new author.
    /// Contains the minimal required information for author creation.
    /// </summary>
    public class AuthorCreateDto
    {
        /// <summary>
        /// Gets or sets the full name of the author.
        /// </summary>
        /// <remarks>
        /// - Required field.  
        /// - Must be between 2 and 100 characters.  
        /// </remarks>
        /// <example>J.K. Rowling</example>
        [Required(ErrorMessage = "Author name is required.")]
        [MinLength(2, ErrorMessage = "Author name must be at least 2 characters long.")]
        [MaxLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
        public string Name { get; init; } = string.Empty;
    }
}