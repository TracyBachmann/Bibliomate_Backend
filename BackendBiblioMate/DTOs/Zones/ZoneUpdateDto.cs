using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to update an existing library zone.
    /// Contains the fields that can be modified on a zone record.
    /// </summary>
    public class ZoneUpdateDto
    {
        /// <summary>
        /// Gets the unique identifier of the zone to update.
        /// </summary>
        /// <example>4</example>
        [Required(ErrorMessage = "ZoneId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ZoneId must be a positive integer.")]
        public int ZoneId { get; init; }

        /// <summary>
        /// Gets the updated human-readable name of the zone.
        /// </summary>
        /// <remarks>
        /// Must be between 1 and 100 characters.
        /// </remarks>
        /// <example>Main Hall</example>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the updated floor number where the zone is located.
        /// </summary>
        /// <remarks>
        /// Must be between 0 and 100.
        /// </remarks>
        /// <example>2</example>
        [Required(ErrorMessage = "FloorNumber is required.")]
        [Range(0, 100, ErrorMessage = "FloorNumber must be between 0 and 100.")]
        public int FloorNumber { get; init; }

        /// <summary>
        /// Gets the updated code of the aisle for quick identification.
        /// </summary>
        /// <remarks>
        /// Must be between 1 and 5 characters.
        /// </remarks>
        /// <example>B</example>
        [Required(ErrorMessage = "AisleCode is required.")]
        [MinLength(1, ErrorMessage = "AisleCode must be at least 1 character long.")]
        [MaxLength(5, ErrorMessage = "AisleCode cannot exceed 5 characters.")]
        public string AisleCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets the updated optional description of the zone (e.g., thematic section).
        /// </summary>
        /// <remarks>
        /// Maximum length of 200 characters.
        /// </remarks>
        /// <example>Historical archives section</example>
        [MinLength(1, ErrorMessage = "Description must be at least 1 character long.")]
        [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string? Description { get; init; }
    }
}