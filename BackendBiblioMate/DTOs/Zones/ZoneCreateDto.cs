using System.ComponentModel.DataAnnotations;

namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// Data Transfer Object used to create a new library zone.
    /// Contains the zone name, floor number, aisle code, and optional description.
    /// </summary>
    public class ZoneCreateDto
    {
        /// <summary>
        /// Gets or sets the human-readable name of the zone.
        /// </summary>
        /// <example>Archives</example>
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters.")]
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the floor number where the zone is located.
        /// </summary>
        /// <remarks>
        /// Value must be between 0 and 100.
        /// </remarks>
        /// <example>1</example>
        [Required(ErrorMessage = "FloorNumber is required.")]
        [Range(0, 100, ErrorMessage = "FloorNumber must be between 0 and 100.")]
        public int FloorNumber { get; init; }

        /// <summary>
        /// Gets or sets the code of the aisle for quick identification.
        /// </summary>
        /// <remarks>
        /// Must be between 1 and 5 characters.
        /// </remarks>
        /// <example>A</example>
        [Required(ErrorMessage = "AisleCode is required.")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "AisleCode must be between 1 and 5 characters.")]
        public string AisleCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional description of the zone (e.g., thematic section).
        /// </summary>
        /// <remarks>
        /// Maximum length of 200 characters. May be null if not provided.
        /// </remarks>
        /// <example>Children’s literature and picture books</example>
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 200 characters.")]
        public string? Description { get; init; }
    }
}
