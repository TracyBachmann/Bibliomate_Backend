using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create or update a genre.
    /// </summary>
    public class GenreCreateDto
    {
        /// <summary>
        /// Name of the genre.
        /// </summary>
        /// <example>Science Fiction</example>
        [Required(ErrorMessage = "Genre name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Genre name must be between 2 and 50 characters.")]
        public string Name { get; set; } = string.Empty;
    }
}