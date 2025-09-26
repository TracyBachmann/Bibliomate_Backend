using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to update an existing library zone.
    /// Contains the fields that can be modified on a zone record.
    /// </summary>
    public class ZoneUpdateDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the zone to update.
        /// </summary>
        /// <example>4</example>
        [Required(ErrorMessage = "ZoneId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ZoneId must be a positive integer.")]
        public int ZoneId { get; init; }

        /// <summary>
        /// Gets or sets the updated human-readable name of the zone.
        /// </summary>
        /// <remarks>
        /// Must be between 1 and 100 characters.
        /// </remarks>
        /// <example>Main Hall</example>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the updated floor number where the zone is located.
        /// </summary>
        /// <remarks>
        /// Value must be between 0 and 100.
        /// </remarks>
        /// <example>2</example>
        [Required(ErrorMessage = "FloorNumber is required.")]
        [Range(0, 100, ErrorMessage = "FloorNumber must be between 0 and 100.")]
        public int FloorNumber { get; init; }

        /// <summary>
        /// Gets or sets the updated code of the aisle for quick identification.
        /// </summary>
        /// <remarks>
        /// Must be between 1 and 5 characters.
        /// </remarks>
        /// <example>B</example>
        [Required(ErrorMessage = "AisleCode is required.")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "AisleCode must be between 1 and 5 characters.")]
        public string AisleCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the updated optional description of the zone (e.g., thematic section).
        /// </summary>
        /// <remarks>
        /// Maximum length of 200 characters. May be null if not provided.
        /// </remarks>
        /// <example>Historical archives section</example>
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 200 characters.")]
        public string? Description { get; init; }
    }
}