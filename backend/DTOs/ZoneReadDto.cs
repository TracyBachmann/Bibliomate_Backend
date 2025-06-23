namespace backend.DTOs
{
    /// <summary>
    /// DTO returned when retrieving library zone information.
    /// </summary>
    public class ZoneReadDto
    {
        /// <summary>
        /// Unique identifier of the zone.
        /// </summary>
        /// <example>4</example>
        public int ZoneId { get; set; }

        /// <summary>
        /// Floor number where the zone is located.
        /// </summary>
        /// <example>1</example>
        public int FloorNumber { get; set; }

        /// <summary>
        /// Code of the aisle for quick identification.
        /// </summary>
        /// <example>A</example>
        public string AisleCode { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the zone (e.g., thematic section).
        /// </summary>
        /// <example>Children’s literature and picture books</example>
        public string? Description { get; set; }
    }
}