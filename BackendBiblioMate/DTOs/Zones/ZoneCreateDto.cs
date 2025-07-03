using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO used to create a new library zone.
    /// Contains the zone name, floor number, aisle code, and optional description.
    /// </summary>
    public class ZoneCreateDto
    {
        /// <summary>
        /// Gets the human-readable name of the zone.
        /// </summary>
        /// <example>Archives</example>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the floor number where the zone is located.
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "FloorNumber is required.")]
        [Range(0, 100, ErrorMessage = "FloorNumber must be between 0 and 100.")]
        public int FloorNumber { get; init; }

        /// <summary>
        /// Gets the code of the aisle for quick identification.
        /// </summary>
        /// <remarks>
        /// Must be between 1 and 5 characters.
        /// </remarks>
        /// <example>A</example>
        [Required(ErrorMessage = "AisleCode is required.")]
        [MinLength(1, ErrorMessage = "AisleCode must be at least 1 character long.")]
        [MaxLength(5, ErrorMessage = "AisleCode cannot exceed 5 characters.")]
        public string AisleCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets the optional description of the zone (e.g., thematic section).
        /// </summary>
        /// <remarks>
        /// Maximum length of 200 characters.
        /// </remarks>
        /// <example>Children’s literature and picture books</example>
        [MinLength(1, ErrorMessage = "Description must be at least 1 character long.")]
        [MaxLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string? Description { get; init; }
    }
}
