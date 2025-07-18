namespace BackendBiblioMate.DTOs
{
    /// <summary>
    /// DTO returned when retrieving library zone information.
    /// Contains the ID, name, floor, aisle, and optional description details.
    /// </summary>
    public class ZoneReadDto
    {
        /// <summary>
        /// Gets the unique identifier of the zone.
        /// </summary>
        /// <example>4</example>
        public int ZoneId { get; init; }

        /// <summary>
        /// Gets the human-readable name of the zone.
        /// </summary>
        /// <example>Archives</example>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the floor number where the zone is located.
        /// </summary>
        /// <example>1</example>
        public int FloorNumber { get; init; }

        /// <summary>
        /// Gets the code of the aisle for quick identification.
        /// </summary>
        /// <example>A</example>
        public string AisleCode { get; init; } = string.Empty;

        /// <summary>
        /// Gets the optional description of the zone (e.g., thematic section).
        /// </summary>
        /// <remarks>
        /// May be null if no description was provided.
        /// </remarks>
        /// <example>Children’s literature and picture books</example>
        public string? Description { get; init; }
    }
}