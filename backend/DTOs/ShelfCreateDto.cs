using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create a new shelf.
    /// </summary>
    public class ShelfCreateDto
    {
        /// <summary>
        /// Identifier of the zone where the shelf is located.
        /// </summary>
        /// <example>2</example>
        [Required(ErrorMessage = "ZoneId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ZoneId must be a positive integer.")]
        public int ZoneId { get; set; }

        /// <summary>
        /// Identifier of the genre associated with the shelf.
        /// </summary>
        /// <example>5</example>
        [Required(ErrorMessage = "GenreId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be a positive integer.")]
        public int GenreId { get; set; }

        /// <summary>
        /// Name of the shelf.
        /// </summary>
        /// <example>Fantasy Shelf A</example>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Maximum number of books the shelf can hold.
        /// </summary>
        /// <example>50</example>
        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be zero or a positive integer.")]
        public int Capacity { get; set; }
    }
}