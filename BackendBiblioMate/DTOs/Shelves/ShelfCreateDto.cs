using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to create a new shelf.
    /// Contains required zone and genre identifiers, shelf name, and optional capacity.
    /// </summary>
    public class ShelfCreateDto
    {
        /// <summary>
        /// Gets the identifier of the zone where the shelf is located.
        /// </summary>
        /// <example>2</example>
        [Required(ErrorMessage = "ZoneId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ZoneId must be a positive integer.")]
        public int ZoneId { get; init; }

        /// <summary>
        /// Gets the identifier of the genre associated with the shelf.
        /// </summary>
        /// <example>5</example>
        [Required(ErrorMessage = "GenreId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "GenreId must be a positive integer.")]
        public int GenreId { get; init; }

        /// <summary>
        /// Gets the name of the shelf.
        /// </summary>
        /// <remarks>
        /// Maximum length of 100 characters.
        /// </remarks>
        /// <example>Fantasy Shelf A</example>
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(1, ErrorMessage = "Name must be at least 1 character long.")]
        [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the maximum number of books the shelf can hold.
        /// </summary>
        /// <remarks>
        /// If not specified, defaults to 0 (no capacity limit).
        /// </remarks>
        /// <example>50</example>
        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be zero or a positive integer.")]
        public int Capacity { get; init; }
    }
}