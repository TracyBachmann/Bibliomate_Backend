using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to update an existing shelf level.
    /// Contains the fields that can be modified on a shelf level record.
    /// </summary>
    public class ShelfLevelUpdateDto
    {
        /// <summary>
        /// Gets the unique identifier of the shelf level to update.
        /// </summary>
        /// <example>5</example>
        [Required(ErrorMessage = "ShelfLevelId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfLevelId must be a positive integer.")]
        public int ShelfLevelId { get; init; }

        /// <summary>
        /// Gets the updated numeric level on the shelf (e.g., 1 for bottom).
        /// </summary>
        /// <example>2</example>
        [Required(ErrorMessage = "LevelNumber is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "LevelNumber must be a positive integer.")]
        public int LevelNumber { get; init; }

        /// <summary>
        /// Gets the identifier of the shelf to which this level belongs.
        /// </summary>
        /// <example>3</example>
        [Required(ErrorMessage = "ShelfId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfId must be a positive integer.")]
        public int ShelfId { get; init; }

        /// <summary>
        /// Gets the maximum height in centimeters of books that can fit on this level.
        /// </summary>
        /// <remarks>
        /// If unspecified, there is no height constraint.
        /// </remarks>
        /// <example>30</example>
        [Range(0, int.MaxValue, ErrorMessage = "MaxHeight must be zero or a positive integer.")]
        public int? MaxHeight { get; init; }

        /// <summary>/// Gets the maximum number of books this level can hold.
        /// </summary>
        /// <remarks>
        /// If unspecified, capacity is considered unlimited.
        /// </remarks>
        /// <example>20</example>
        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be zero or a positive integer.")]
        public int? Capacity { get; init; }

        /// <summary>
        /// Gets the current number of books stored on this level.
        /// </summary>
        /// <remarks>
        /// Used to track real-time occupancy.
        /// </remarks>
        /// <example>12</example>
        [Range(0, int.MaxValue, ErrorMessage = "CurrentLoad must be zero or a positive integer.")]
        public int? CurrentLoad { get; init; }
    }
}
