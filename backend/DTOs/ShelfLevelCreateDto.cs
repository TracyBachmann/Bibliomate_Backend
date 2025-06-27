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
        [Required(ErrorMessage = "LevelNumber is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "LevelNumber must be a positive integer.")]
        public int LevelNumber { get; set; }

        /// <summary>
        /// Identifier of the shelf to which this level belongs.
        /// </summary>
        [Required(ErrorMessage = "ShelfId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfId must be a positive integer.")]
        public int ShelfId { get; set; }

        /// <summary>
        /// Optional maximum height capacity in centimeters.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "MaxHeight must be zero or a positive integer.")]
        public int MaxHeight { get; set; }

        /// <summary>
        /// Optional maximum number of books this level can hold.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be zero or a positive integer.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Optional current number of books on this level.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "CurrentLoad must be zero or a positive integer.")]
        public int CurrentLoad { get; set; }
    }
}