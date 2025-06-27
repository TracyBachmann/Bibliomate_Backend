using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing shelf level.
    /// </summary>
    public class ShelfLevelUpdateDto
    {
        /// <summary>
        /// Unique identifier of the shelf level to update.
        /// </summary>
        /// <example>5</example>
        [Required(ErrorMessage = "ShelfLevelId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfLevelId must be a positive integer.")]
        public int ShelfLevelId { get; set; }

        /// <summary>
        /// Updated numeric level on the shelf (e.g., 1 for bottom).
        /// </summary>
        /// <example>2</example>
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

        /// <summary>
        /// Maximum height (in cm) of books that can fit on this level.
        /// </summary>
        /// <example>30</example>
        [Range(0, int.MaxValue, ErrorMessage = "MaxHeight must be zero or a positive integer.")]
        public int MaxHeight { get; set; }

        /// <summary>
        /// Maximum number of books this level can hold.
        /// </summary>
        /// <example>20</example>
        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be zero or a positive integer.")]
        public int Capacity { get; set; }

        /// <summary>
        /// Current number of books stored on this level.
        /// </summary>
        /// <example>12</example>
        [Range(0, int.MaxValue, ErrorMessage = "CurrentLoad must be zero or a positive integer.")]
        public int CurrentLoad { get; set; }
    }
}
