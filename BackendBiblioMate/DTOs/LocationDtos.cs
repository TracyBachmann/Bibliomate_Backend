using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object representing a floor number in the library.
    /// Typically used for populating dropdowns or lists in the UI.
    /// </summary>
    /// <param name="FloorNumber">The floor number (0–100).</param>
    public record FloorReadDto(int FloorNumber);

    /// <summary>
    /// Data Transfer Object representing an aisle code in the library.
    /// Typically used for populating dropdowns or lists in the UI.
    /// </summary>
    /// <param name="AisleCode">The aisle code (1–20 characters).</param>
    public record AisleReadDto(string AisleCode);

    /// <summary>
    /// Data Transfer Object representing a simplified shelf view.
    /// Typically used for lightweight selection lists in the UI.
    /// </summary>
    /// <param name="ShelfId">The unique identifier of the shelf.</param>
    /// <param name="Name">The human-readable name of the shelf.</param>
    public record ShelfMiniReadDto(int ShelfId, string Name);

    /// <summary>
    /// Data Transfer Object representing a shelf level number.
    /// Typically used for populating dropdowns or lists in the UI.
    /// </summary>
    /// <param name="LevelNumber">The shelf level number (1 = bottom, increasing upwards).</param>
    public record LevelReadDto(int LevelNumber);

    /// <summary>
    /// Data Transfer Object used to request an "ensure" operation for a semantic location.
    /// The system will create or retrieve the matching zone, shelf, and shelf level
    /// based on these values.
    /// </summary>
    public class LocationEnsureDto
    {
        /// <summary>
        /// Gets or sets the floor number where the location is found.
        /// </summary>
        /// <remarks>Valid values range from 0 to 100.</remarks>
        /// <example>1</example>
        [Range(0, 100, ErrorMessage = "FloorNumber must be between 0 and 100.")]
        public int FloorNumber { get; init; }

        /// <summary>
        /// Gets or sets the aisle code for the location.
        /// </summary>
        /// <remarks>Must be between 1 and 20 characters.</remarks>
        /// <example>A1</example>
        [Required(ErrorMessage = "AisleCode is required.")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "AisleCode must be between 1 and 20 characters.")]
        public string AisleCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the shelf.
        /// </summary>
        /// <remarks>Must be between 1 and 100 characters.</remarks>
        /// <example>Fantasy Shelf A</example>
        [Required(ErrorMessage = "ShelfName is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "ShelfName must be between 1 and 100 characters.")]
        public string ShelfName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the level number of the shelf (1 = bottom).
        /// </summary>
        /// <remarks>Valid values range from 1 to 100.</remarks>
        /// <example>2</example>
        [Range(1, 100, ErrorMessage = "LevelNumber must be between 1 and 100.")]
        public int LevelNumber { get; init; } = 1;
    }

    /// <summary>
    /// Data Transfer Object returned after an "ensure" operation on a semantic location.
    /// Contains the identifiers of the created or matched zone, shelf, and shelf level,
    /// as well as a reminder of the semantic values used.
    /// </summary>
    public class LocationReadDto
    {
        /// <summary>
        /// Gets or sets the identifier of the zone.
        /// </summary>
        /// <example>1</example>
        public int ZoneId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the shelf.
        /// </summary>
        /// <example>10</example>
        public int ShelfId { get; init; }

        /// <summary>
        /// Gets or sets the identifier of the shelf level.
        /// </summary>
        /// <example>25</example>
        public int ShelfLevelId { get; init; }

        /// <summary>
        /// Gets or sets the floor number of the location.
        /// </summary>
        /// <example>1</example>
        public int FloorNumber { get; init; }

        /// <summary>
        /// Gets or sets the aisle code of the location.
        /// </summary>
        /// <example>A1</example>
        public string AisleCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the shelf.
        /// </summary>
        /// <example>Fantasy Shelf A</example>
        public string ShelfName { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the level number of the shelf.
        /// </summary>
        /// <example>2</example>
        public int LevelNumber { get; init; }
    }
}