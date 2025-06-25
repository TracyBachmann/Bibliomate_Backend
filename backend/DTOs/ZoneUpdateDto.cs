using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    /// <summary>
    /// DTO used to update an existing library zone.
    /// </summary>
    public class ZoneUpdateDto
    {
        /// <summary>
        /// Unique identifier of the zone to update.
        /// </summary>
        /// <example>4</example>
        [Required(ErrorMessage = "ZoneId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "ZoneId must be a positive integer.")]
        public int ZoneId { get; set; }

        /// <summary>
        /// Updated floor number where the zone is located.
        /// </summary>
        /// <example>2</example>
        [Required(ErrorMessage = "FloorNumber is required.")]
        [Range(0, 100, ErrorMessage = "FloorNumber must be between 0 and 100.")]
        public int FloorNumber { get; set; }

        /// <summary>
        /// Updated code of the aisle for quick identification.
        /// </summary>
        /// <example>B</example>
        [Required(ErrorMessage = "AisleCode is required.")]
        [StringLength(5, MinimumLength = 1, ErrorMessage = "AisleCode must be between 1 and 5 characters.")]
        public string AisleCode { get; set; } = string.Empty;

        /// <summary>
        /// Updated optional description of the zone (e.g., thematic section).
        /// </summary>
        /// <example>Historical archives section</example>
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string? Description { get; set; }
    }
}