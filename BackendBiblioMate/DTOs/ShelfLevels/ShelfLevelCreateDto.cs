using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to create a new shelf level.
    /// Contains the level details and optional capacity constraints.
    /// </summary>
    public class ShelfLevelCreateDto
    {
        /// <summary>
        /// Gets or sets the level number on the shelf.
        /// </summary>
        /// <remarks>
        /// <c>1</c> corresponds to the bottom level, increasing upwards.
        /// </remarks>
        /// <example>1</example>
        [Required(ErrorMessage = "LevelNumber is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "LevelNumber must be a positive integer.")]
        public int LevelNumber { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the shelf to which this level belongs.
        /// </summary>
        /// <example>10</example>
        [Required(ErrorMessage = "ShelfId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ShelfId must be a positive integer.")]
        public int ShelfId { get; init; }

        /// <summary>
        /// Gets or sets the optional maximum height capacity in centimeters.
        /// </summary>
        /// <remarks>
        /// If not provided, there is no height constraint.
        /// </remarks>
        /// <example>30</example>
        [Range(0, int.MaxValue, ErrorMessage = "MaxHeight must be zero or a positive integer.")]
        public int? MaxHeight { get; init; }

        /// <summary>
        /// Gets or sets the optional maximum number of books this level can hold.
        /// </summary>
        /// <remarks>
        /// If not provided, capacity is considered unlimited.
        /// </remarks>
        /// <example>50</example>
        [Range(0, int.MaxValue, ErrorMessage = "Capacity must be zero or a positive integer.")]
        public int? Capacity { get; init; }

        /// <summary>
        /// Gets or sets the optional current number of books on this level.
        /// </summary>
        /// <remarks>
        /// Used to track real-time occupancy. Typically managed by the system, not by clients.
        /// </remarks>
        /// <example>12</example>
        [Range(0, int.MaxValue, ErrorMessage = "CurrentLoad must be zero or a positive integer.")]
        public int? CurrentLoad { get; init; }
    }
}