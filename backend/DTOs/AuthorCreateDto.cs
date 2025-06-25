using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create a new author.
    /// </summary>
    public class AuthorCreateDto
    {
        /// <summary>
        /// Full name of the author.
        /// </summary>
        /// <example>J.K. Rowling</example>
        [Required(ErrorMessage = "Author name is required.")]
        [MinLength(2, ErrorMessage = "Author name must be at least 2 characters long.")]
        [StringLength(100, ErrorMessage = "Author name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;
    }
}