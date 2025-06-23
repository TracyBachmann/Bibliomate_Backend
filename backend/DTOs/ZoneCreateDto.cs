using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to create a new library zone.
    /// </summary>
    public class ZoneCreateDto
    {
        /// <summary>
        /// Floor number where the zone is located.
        /// </summary>
        /// <example>1</example>
        [Required(ErrorMessage = "FloorNumber is required.")]
        [Range(0, 100, ErrorMessage = "FloorNumber must be between 0 and 100.")]
        public int FloorNumber { get; set; }

        /// <summary>
        /// Code of the aisle for quick identification.
        /// </summary>
        /// <example>A</example>
        [Required(ErrorMessage = "AisleCode is required.")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "AisleCode must be between 1 and 5 characters.")]
        public string AisleCode { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the zone (e.g., thematic section).
        /// </summary>
        /// <example>Children’s literature and picture books</example>
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string? Description { get; set; }
    }
}