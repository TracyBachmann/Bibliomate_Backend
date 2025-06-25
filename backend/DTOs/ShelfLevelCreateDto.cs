using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create a new shelf level.
    /// </summary>
    public class ShelfLevelCreateDto
    {
        /// <summary>
        /// Level number on the shelf (e.g., 1 for the bottom level).
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "LevelNumber is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "LevelNumber must be a positive integer.")]
        public int LevelNumber { get; set; }

        /// <summary>
        /// Identifier of the shelf to which this level belongs.
        /// </summary>
        /// <example>3</example>
        [Required(ErrorMessage = "ShelfId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfId must be a positive integer.")]
        public int ShelfId { get; set; }
    }
}