using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing shelf.
    /// </summary>
    public class ShelfUpdateDto
    {
        /// <summary>
        /// Unique identifier of the shelf to update.
        /// </summary>
        /// <example>3</example>
        [Required(ErrorMessage = "ShelfId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfId must be a positive integer.")]
        public int ShelfId { get; set; }

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
        /// Updated name of the shelf.
        /// </summary>
        /// <example>Fantasy Shelf A - Updated</example>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Updated maximum number of books the shelf can hold.
        /// </summary>
        /// <example>60</example>
        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be zero or a positive integer.")]
        public int Capacity { get; set; }
    }
}